using AiDocumentRedactor.App.ViewModels;
using AiDocumentRedactor.Core;
using AiDocumentRedactor.Documents;
using Xunit;

namespace AiDocumentRedactor.Tests;

/// <summary>Tests for the redaction session (what the UI drives), using a temporary folder and a fake model instead of Ollama.</summary>
public class SessionTests : IDisposable
{
    readonly string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
    string In => Path.Combine(root, "in");
    readonly RedactorOptions options;

    /// <summary>A model that "finds" a fixed list of strings in whatever text it is given.</summary>
    internal class FakeModel(params (string Type, string Text)[] finds) : IEntityDetector, IDetectorTrace
    {
        /// <summary>One recorded call per run, like the real detector (the session uses it to measure speed).</summary>
        public IReadOnlyList<ModelCall> Calls { get; private set; } = [];
        public Task<IReadOnlyList<DetectedEntity>> DetectAsync(string text, IProgress<RedactionProgress>? p, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<DetectedEntity>>(Record(text) ?? finds.SelectMany(f => Enumerable.Range(0, text.Length)
                .Where(i => string.CompareOrdinal(text, i, f.Text, 0, f.Text.Length) == 0).Select(i => new DetectedEntity(f.Type, i, f.Text.Length, 1, "llm"))).ToList());

        /// <summary>Records the call; returns null so the caller carries on to build the result.</summary>
        List<DetectedEntity>? Record(string text) { Calls = [new ModelCall(1, 0, text.Length, "", "", [], 0, 0, TimeSpan.FromSeconds(1), 1)]; return null; }
    }

    /// <summary>A catalog that reports the given models as installed.</summary>
    internal class FakeCatalog(params string[] names) : IModelCatalog
    {
        public Task<IReadOnlyList<ModelInfo>> ListAsync(CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<ModelInfo>>(names.Select(n => new ModelInfo(n, "1B", "Q4", "x", 1 << 30, "abcdef123456", 8192)).ToList());
    }

    /// <summary>Creates the temporary input folder and the options used by every test.</summary>
    public SessionTests()
    {
        Directory.CreateDirectory(In);
        options = new RedactorOptions
        {
            Input = { Include = ["*.txt"] },
            Output = { Directory = Path.Combine(root, "out") },
            Llm = { Model = "phi4", CandidateModels = ["phi4"], ChunkChars = 1000, ChunkOverlapChars = 0 },
        };
    }
    /// <summary>Deletes the temporary folders.</summary>
    public void Dispose() => Directory.Delete(root, true);

    /// <summary>A session over the temporary folder whose model finds the given strings.</summary>
    async Task<RedactionSession> Session(params (string Type, string Text)[] finds)
    {
        var s = new RedactionSession(options, In, [new TextDocumentReader()], [new TextDocumentWriter()], new FakeCatalog("phi4"), _ => new FakeModel(finds));
        await s.LoadModelsAsync();
        return s;
    }

    /// <summary>Writes a file into the input folder and selects it.</summary>
    static async Task<DocumentItem> Select(RedactionSession s, string name, string text)
    {
        File.WriteAllText(Path.Combine(s.Options.Input.Directory is { } _ ? Path.Combine(Path.GetDirectoryName(s.Options.Output.Directory)!, "in") : "", name), text);
        s.Refresh();
        var item = s.Documents.Items.Single(i => i.Name == name);
        await s.SelectAsync(item);
        return item;
    }

    /// <summary>The estimate counts chunks from the text size, guesses the time before any run, and uses the measured speed afterwards.</summary>
    [Fact]
    public async Task Estimate_counts_chunks_and_becomes_measured_after_a_run()
    {
        var s = await Session(("PERSON", "Sarah"));
        await Select(s, "a.txt", new string('x', 2500) + " Sarah");
        var before = s.Estimate()!;
        Assert.Equal(3, before.Chunks);                 // 2,506 characters in chunks of 1,000
        Assert.False(before.Measured);
        Assert.Equal(24, before.Seconds, 1);            // 3 chunks x the 8 s default
        Assert.Null(before.Warning);

        await s.RunAsync();
        Assert.True(s.Estimate()!.Measured);
    }

    /// <summary>A document over the size limits gets a plain-English warning with a time forecast.</summary>
    [Fact]
    public async Task Large_documents_get_a_warning()
    {
        options.Ui.WarnChars = 1000;
        var s = await Session();
        await Select(s, "big.txt", new string('y', 5000));
        var est = s.Estimate()!;
        Assert.NotNull(est.Warning);
        Assert.Contains("Large document", est.Warning);
        Assert.Contains("5,000 characters", est.Warning);
    }
}
