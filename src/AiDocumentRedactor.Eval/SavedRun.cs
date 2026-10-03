using System.Text.Json;
using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.Eval;

/// <summary>A model in a saved run, with what Ollama said about it.</summary>
public record SavedModel(string Model, ModelInfo? Info);

/// <summary>Everything needed to rebuild a report: the run details and every document score. Written beside each report as a .scores.json file so runs can be merged later.</summary>
public record SavedRun(DateTime Started, double ElapsedSeconds, string Machine, string CorpusDir, int Documents, int GroundTruthEntities, RedactorOptions Options,
    List<SavedModel> Models, bool ShowText, bool WroteOutputs, List<string> Skipped, string? OllamaVersion, List<DocScore> Scores)
{
    static readonly JsonSerializerOptions Json = new() { IncludeFields = true, WriteIndented = true };

    /// <summary>The run as it was, ready to save.</summary>
    public static SavedRun From(RunInfo run, List<DocScore> scores) => new(run.Started, run.Elapsed.TotalSeconds, run.Machine, run.CorpusDir, run.Documents, run.GroundTruthEntities,
        run.Options, run.Models.Select(m => new SavedModel(m.Model, m.Info)).ToList(), run.ShowText, run.WroteOutputs, run.Skipped, run.OllamaVersion, scores);

    /// <summary>The report's view of this run.</summary>
    public RunInfo ToRunInfo() => new(Started, TimeSpan.FromSeconds(ElapsedSeconds), Machine, CorpusDir, Documents, GroundTruthEntities, Options,
        Models.Select(m => (m.Model, m.Info)).ToList(), ShowText, WroteOutputs, Skipped, OllamaVersion);

    public string ToJson() => JsonSerializer.Serialize(this, Json);

    /// <summary>Reads a saved run from a .scores.json file, or from a gzip-compressed .scores.json.gz (used for the versioned copies, which are about a tenth of the size).</summary>
    public static SavedRun Load(string path)
    {
        string json;
        if (path.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
        {
            using var gz = new System.IO.Compression.GZipStream(File.OpenRead(path), System.IO.Compression.CompressionMode.Decompress);
            using var reader = new StreamReader(gz);
            json = reader.ReadToEnd();
        }
        else
        {
            json = File.ReadAllText(path);
        }

        return JsonSerializer.Deserialize<SavedRun>(json, Json) ?? throw new InvalidDataException($"{path} is empty.");
    }

    /// <summary>Combines runs. A model that appears in a later file replaces the same model from earlier files, with its failures; everything else is kept.</summary>
    public static SavedRun Merge(IReadOnlyList<SavedRun> runs)
    {
        var models = new List<SavedModel>();
        var scores = new List<DocScore>();
        var skipped = new List<string>();
        foreach (var r in runs)
        {
            foreach (var m in r.Models)
            {
                models.RemoveAll(x => x.Model == m.Model);
                scores.RemoveAll(s => s.Model == m.Model);
                skipped.RemoveAll(s => s.Contains($"with {m.Model}:"));
                models.Add(m);
            }

            scores.AddRange(r.Scores);
            skipped.AddRange(r.Skipped);
        }

        var last = runs[^1];
        return last with
        {
            Started = runs.Min(r => r.Started),
            ElapsedSeconds = runs.Sum(r => r.ElapsedSeconds),
            Models = models,
            Scores = scores,
            Skipped = skipped,
            ShowText = runs.All(r => r.ShowText),
            WroteOutputs = runs.Any(r => r.WroteOutputs),
        };
    }
}
