using AiDocumentRedactor.App.ViewModels;
using AiDocumentRedactor.Core;
using AiDocumentRedactor.Documents;
using Xunit;

namespace AiDocumentRedactor.Tests;

/// <summary>Tests for manual redaction and review: adding, rejecting, restoring, undo/redo, saving and the offsets-only sidecar.</summary>
public class ReviewTests : IDisposable
{
    readonly string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
    string In => Path.Combine(root, "in");
    readonly RedactorOptions options;
    const string Text = "Sarah Jones met Sarah at Acme on Monday.";

    /// <summary>Creates the temporary input folder and the options used by every test.</summary>
    public ReviewTests()
    {
        Directory.CreateDirectory(In);
        options = new RedactorOptions
        {
            Input = { Include = ["*.txt"] }, Output = { Directory = Path.Combine(root, "out") },
            Llm = { Model = "phi4", CandidateModels = ["phi4"], ChunkChars = 1000, ChunkOverlapChars = 0 },
        };
        File.WriteAllText(Path.Combine(In, "a.txt"), Text);
    }
    /// <summary>Deletes the temporary folders.</summary>
    public void Dispose() => Directory.Delete(root, true);

    /// <summary>A session with the document selected; the fake model finds the given strings.</summary>
    async Task<RedactionSession> Open(ReviewStore? store = null, params (string Type, string Text)[] finds)
    {
        var s = new RedactionSession(options, In, [new TextDocumentReader()], [new TextDocumentWriter()], new SessionTests.FakeCatalog("phi4"), _ => new SessionTests.FakeModel(finds), store);
        await s.LoadModelsAsync(); s.Refresh();
        await s.SelectAsync(s.Documents.Items.Single());
        return s;
    }

    /// <summary>Without any model, selecting text redacts it in a hand-made result, and the output file can be saved.</summary>
    [Fact]
    public async Task Manual_redaction_works_without_a_model_and_saves()
    {
        var s = await Open();
        Assert.Equal(1, s.AddManual(Text.IndexOf("Acme"), 4, "COMPANY", false));
        Assert.True(s.ActiveResult!.IsManual);
        Assert.Equal("Sarah Jones met Sarah at [REDACTED:COMPANY] on Monday.", s.ActiveResult.Result.RedactedText);
        Assert.True(s.IsModified);
        await s.SaveAsync();
        Assert.False(s.IsModified);
        Assert.Equal("Sarah Jones met Sarah at [REDACTED:COMPANY] on Monday.", File.ReadAllText(Directory.GetFiles(options.Output.Directory, "a*").Single()));
    }

    /// <summary>"All occurrences" redacts every identical piece of text; trailing spaces in a selection are ignored.</summary>
    [Fact]
    public async Task All_occurrences_and_trimming()
    {
        var s = await Open();
        Assert.Equal(2, s.AddManual(0, 6, "PERSON", true));   // "Sarah " including the trailing space
        Assert.Equal("[REDACTED:PERSON] Jones met [REDACTED:PERSON] at Acme on Monday.", s.ActiveResult!.Result.RedactedText);
    }

    /// <summary>Rejecting an AI edit leaves the text visible; restoring brings it back; undo and redo step through changes.</summary>
    [Fact]
    public async Task Reject_restore_undo_redo()
    {
        var s = await Open(null, ("PERSON", "Sarah"));
        await s.RunAsync();
        var first = s.ActiveResult!.Result.Edits.First(e => e.Status == EditStatus.Active);
        s.RejectEdit(first.Id);
        Assert.Equal(1, s.ActiveResult!.EditCount);
        Assert.StartsWith("Sarah Jones met [REDACTED:PERSON]", s.ActiveResult.Result.RedactedText);
        var rejected = s.ActiveResult.Result.Edits.Single(e => e.Status == EditStatus.Rejected);
        Assert.Equal(0, rejected.RedactedStart);

        s.RestoreEdit(rejected.Id);
        Assert.Equal(2, s.ActiveResult!.EditCount);
        s.Undo();   // undo the restore
        Assert.Equal(1, s.ActiveResult!.EditCount);
        s.Undo();   // undo the reject
        Assert.Equal(2, s.ActiveResult!.EditCount);
        Assert.False(s.IsModified);
        s.Redo();
        Assert.Equal(1, s.ActiveResult!.EditCount);
    }

    /// <summary>A reviewer's changes apply to every model's result, and a person's own edits are High confidence.</summary>
    [Fact]
    public async Task Review_carries_across_results_and_human_edits_are_high_confidence()
    {
        var s = await Open(null, ("PERSON", "Sarah"));
        await s.RunAsync();
        s.AddManual(Text.IndexOf("Monday"), 6, "OTHER", false);
        var e = s.ActiveResult!.Result.Edits.Single(x => x.Source == "human");
        Assert.Equal(ConfidenceLevel.High, s.Confidence()[e.Id].Level);
        s.ChangeType(e.Id, "DATE_OF_BIRTH");
        Assert.Contains("[REDACTED:DATE_OF_BIRTH]", s.ActiveResult!.Result.RedactedText);
        s.RejectEdit(s.ActiveResult.Result.Edits.Single(x => x.Source == "human").Id);   // removing a manual edit deletes it
        Assert.DoesNotContain("DATE_OF_BIRTH", s.ActiveResult!.Result.RedactedText);
        Assert.DoesNotContain(s.ActiveResult.Result.Edits, x => x.Status == EditStatus.Rejected);
    }

    /// <summary>The saved review holds offsets only (no document text) and comes back for the same file in a new session.</summary>
    [Fact]
    public async Task Review_is_saved_as_offsets_only_and_restored()
    {
        var store = new ReviewStore(Path.Combine(root, "review"));
        var s = await Open(store);
        s.AddManual(Text.IndexOf("Acme"), 4, "COMPANY", false);
        var json = File.ReadAllText(Directory.GetFiles(Path.Combine(root, "review")).Single());
        Assert.DoesNotContain("Acme", json);
        Assert.Contains("COMPANY", json);

        var again = await Open(store);   // a new session: the edit returns, as a hand-made result
        Assert.Equal("Sarah Jones met Sarah at [REDACTED:COMPANY] on Monday.", again.ActiveResult!.Result.RedactedText);
    }
}
