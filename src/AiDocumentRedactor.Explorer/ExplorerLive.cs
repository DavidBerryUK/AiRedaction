namespace AiDocumentRedactor.Explorer;

/// <summary>A model installed in the local Ollama that a live run can use.</summary>
public record LiveModel(string Name, string Summary);

/// <summary>What a live run produced: the setups it added to the dataset, anything the reader should know before trusting the numbers, and how long it took.</summary>
public record LiveRunResult(IReadOnlyList<string> Configs, IReadOnlyList<string> Warnings, double Seconds);

/// <summary>The live run's own failures, with a message fit to show: the input copy differs, another run is going, Ollama is not reachable, and so on.</summary>
public class LiveRunException(string message) : Exception(message);

/// <summary>Runs one document through the real pipeline with a chosen local model and adds the result to the dataset as a live result. Implemented by the web host, because it needs
/// Ollama and the document readers; the explorer pages only use this interface.</summary>
public interface ILiveRunner
{
    /// <summary>Whether GLiNER can be used (its model files are present).</summary>
    bool GlinerAvailable { get; }

    /// <summary>The models installed in the local Ollama.</summary>
    Task<IReadOnlyList<LiveModel>> ModelsAsync(CancellationToken ct);

    /// <summary>Runs <paramref name="docId"/> of the dataset with <paramref name="model"/>, scores it against the answer key, saves it as live results and rebuilds the database.
    /// <paramref name="progress"/> receives short messages. Throws <see cref="LiveRunException"/> for problems the user can act on.</summary>
    Task<LiveRunResult> RunAsync(string datasetId, string docId, string model, bool withGliner, IProgress<string>? progress, CancellationToken ct);
}

public partial class ExplorerService
{
    /// <summary>How many live runs of this model on this document already exist, plus one: the number for the next run's name.</summary>
    public async Task<int> NextLiveNumberAsync(string docId, string model) => 1 + (await QueryAsync(cmd =>
    {
        cmd.Parameters.AddWithValue("@d", docId);
        cmd.Parameters.AddWithValue("@m", model);
        return "SELECT COUNT(*) FROM results WHERE doc_id = @d AND model = @m AND source = 'live' AND variant = 'plain'";
    }, r => Int(r, 0))).Single();

    /// <summary>The settings fingerprints recorded for the dataset's source runs (from run.json), to tell whether a live run used the same settings.</summary>
    public async Task<IReadOnlyList<string>> SettingsHashesAsync()
    {
        var json = (await QueryAsync("SELECT value FROM meta WHERE key = 'run'", r => r.GetString(0))).Single();
        var run = System.Text.Json.JsonDocument.Parse(json).RootElement;
        return run.TryGetProperty("sources", out var sources)
            ? sources.EnumerateArray().Select(s => s.TryGetProperty("settingsHash", out var h) ? h.GetString() ?? string.Empty : string.Empty).Where(h => h.Length > 0).ToList()
            : [];
    }

    /// <summary>The answer-key checksum recorded for a corpus in run.json (null if there is none).</summary>
    public async Task<string?> KeyChecksumAsync(string corpus)
    {
        var json = (await QueryAsync("SELECT value FROM meta WHERE key = 'run'", r => r.GetString(0))).Single();
        var run = System.Text.Json.JsonDocument.Parse(json).RootElement;
        return run.TryGetProperty("corpora", out var corpora)
            ? corpora.EnumerateArray().Where(c => c.GetProperty("corpus").GetString() == corpus).Select(c => c.TryGetProperty("keyChecksum", out var k) ? k.GetString() : null).FirstOrDefault()
            : null;
    }
}
