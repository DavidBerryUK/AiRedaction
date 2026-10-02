using AiDocumentRedactor.App.ViewModels;
using AiDocumentRedactor.Core;
using AiDocumentRedactor.Documents;
using Xunit;

namespace AiDocumentRedactor.Tests;

/// <summary>Tests for the review of things only the second-opinion model (GLiNER) found: how they are described, and accepting or dismissing them.</summary>
public class FlagReviewTests : IDisposable
{
    const string Text = "Sarah Jones met Kestrel team at Acme on Monday.";
    readonly string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
    string In => Path.Combine(root, "in");
    readonly RedactorOptions options;

    /// <summary>A detector that finds "Sarah Jones" as the main model, and "Kestrel" only as the second opinion (flagged).</summary>
    class FlaggingModel : IEntityDetector
    {
        public Task<IReadOnlyList<DetectedEntity>> DetectAsync(string text, IProgress<RedactionProgress>? p, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<DetectedEntity>>([
                new("PERSON", text.IndexOf("Sarah", StringComparison.Ordinal), 11, 1.0, "llm+gliner"),
                new("CONTEXTUAL", text.IndexOf("Kestrel", StringComparison.Ordinal), 7, 0.62, "gliner-only", true),
            ]);
    }

    public FlagReviewTests()
    {
        Directory.CreateDirectory(In);
        File.WriteAllText(Path.Combine(In, "a.txt"), Text);
        options = new RedactorOptions {
            Input = { Include = ["*.txt"] },
            Output = { Directory = Path.Combine(root, "out") },
            Llm = { Model = "phi4", CandidateModels = ["phi4"], ChunkChars = 1000, ChunkOverlapChars = 0 },
        };
    }

    public void Dispose() => Directory.Delete(root, true);

    async Task<RedactionSession> Run()
    {
        var s = new RedactionSession(options, In, [new TextDocumentReader()], [new TextDocumentWriter()], new SessionTests.FakeCatalog("phi4"), _ => new FlaggingModel());
        await s.LoadModelsAsync();
        s.Refresh();
        await s.SelectAsync(s.Documents.Items.Single());
        await s.RunAsync();
        return s;
    }

    /// <summary>The source chips are plain-language, and agreement with GLiNER is shown.</summary>
    [Fact]
    public void Source_labels_are_plain_language()
    {
        Assert.Equal("AI", EditSource.Label("llm"));
        Assert.Equal("AI", EditSource.Label("llm-variant"));
        Assert.Equal("Rule", EditSource.Label("rule"));
        Assert.Equal("Manual", EditSource.Label("human"));
        Assert.Equal("AI + GLiNER", EditSource.Label("llm+gliner"));
        Assert.Equal("Rule + GLiNER", EditSource.Label("rule+gliner"));
        Assert.Equal("GLiNER only", EditSource.Label("gliner-only"));
        Assert.Contains("confirmed by the second-opinion model", EditSource.Tooltip("llm+gliner"));
        Assert.Contains("did not find it", EditSource.GlinerOnlyReason(0.62));
        Assert.Contains("0.62", EditSource.GlinerOnlyReason(0.62));
    }

    /// <summary>A GLiNER-only find is listed as flagged and low confidence, with the reason and score, and the text stays visible.</summary>
    [Fact]
    public async Task Flag_is_listed_with_its_reason_and_left_in_the_text()
    {
        var s = await Run();
        Assert.Equal("[REDACTED:PERSON] met Kestrel team at Acme on Monday.", s.ActiveResult!.Result.RedactedText);
        var flag = s.Bookmarks().Single(b => b.Flagged);
        Assert.Equal("gliner-only", flag.Source);
        Assert.Equal(ConfidenceLevel.Low, flag.Confidence.Level);
        Assert.Contains("0.62", flag.Confidence.Reason);
        Assert.Contains("Redact this", flag.Confidence.Reason);
    }

    /// <summary>Accepting a flag redacts the text, and the edit is then a reviewer's own (manual) redaction.</summary>
    [Fact]
    public async Task Accepting_a_flag_redacts_it()
    {
        var s = await Run();
        s.AcceptFlag(s.Bookmarks().Single(b => b.Flagged).Id);
        Assert.Equal("[REDACTED:PERSON] met [REDACTED:CONTEXTUAL] team at Acme on Monday.", s.ActiveResult!.Result.RedactedText);
        Assert.DoesNotContain(s.Bookmarks(), b => b.Flagged);
        Assert.Contains(s.Bookmarks(), b => b.Type == "CONTEXTUAL" && b.Source == "human");
        Assert.True(s.IsModified);
    }

    /// <summary>Dismissing a flag keeps the text visible and lists it as rejected; accepting does nothing for an edit that is not flagged.</summary>
    [Fact]
    public async Task Dismissing_a_flag_leaves_the_text_and_accept_ignores_other_edits()
    {
        var s = await Run();
        var flag = s.Bookmarks().Single(b => b.Flagged);
        var person = s.Bookmarks().Single(b => b.Type == "PERSON");
        s.AcceptFlag(person.Id);   // not flagged: no change
        Assert.False(s.IsModified);
        s.RejectEdit(flag.Id);
        Assert.Equal("[REDACTED:PERSON] met Kestrel team at Acme on Monday.", s.ActiveResult!.Result.RedactedText);
        Assert.Contains(s.Bookmarks(), b => b.Rejected && b.Type == "CONTEXTUAL");
    }
}

/// <summary>Tests for the session-only second-opinion switch in the Categories dialog.</summary>
public class SecondOpinionSwitchTests : IDisposable
{
    readonly string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
    string In => Path.Combine(root, "in");

    public SecondOpinionSwitchTests() => Directory.CreateDirectory(In);

    public void Dispose() => Directory.Delete(root, true);

    RedactionSession Make(string modelDir)
    {
        var o = new RedactorOptions {
            Input = { Include = ["*.txt"] },
            Output = { Directory = Path.Combine(root, "out") },
            Llm = { Model = "phi4", CandidateModels = ["phi4"] },
            Gliner = { ModelDirectory = modelDir },
        };
        return new RedactionSession(o, In, [new TextDocumentReader()], [new TextDocumentWriter()], new SessionTests.FakeCatalog("phi4"), _ => new SessionTests.FakeModel());
    }

    /// <summary>Without the model files the switch cannot be turned on; with them it can, and Reset puts it back.</summary>
    [Fact]
    public void Switch_needs_the_model_files_and_resets_to_the_config()
    {
        var missing = Make(Path.Combine(root, "nothing"));
        Assert.False(missing.SecondOpinionAvailable);
        missing.SetSecondOpinion(true);
        Assert.False(missing.SecondOpinionOn);

        var models = Path.Combine(root, "models");
        Directory.CreateDirectory(models);
        File.WriteAllText(Path.Combine(models, "model_quint8.onnx"), "x");
        var s = Make(models);
        Assert.True(s.SecondOpinionAvailable);
        Assert.False(s.SecondOpinionOn);
        Assert.False(s.CategoriesChanged);
        s.SetSecondOpinion(true);
        s.SetSecondOpinionSoloAction("redact");
        Assert.True(s.SecondOpinionOn);
        Assert.Equal("redact", s.SecondOpinionSoloAction);
        Assert.True(s.CategoriesChanged);
        s.ResetCategories();
        Assert.False(s.SecondOpinionOn);
        Assert.Equal("flag", s.SecondOpinionSoloAction);
        Assert.False(s.CategoriesChanged);
    }
}
