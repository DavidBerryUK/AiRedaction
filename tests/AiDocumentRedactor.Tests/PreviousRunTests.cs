using AiDocumentRedactor.App.ViewModels;
using AiDocumentRedactor.Core;
using AiDocumentRedactor.Documents;
using Xunit;

namespace AiDocumentRedactor.Tests;

/// <summary>Tests for viewing a stored result from an earlier evaluation run: rebuilding it, keeping it apart from live results, and never changing the document.</summary>
public class PreviousRunTests : IDisposable
{
    const string Template = "[REDACTED:{type}]";
    const string Text = "Sarah Jones lives in Leeds. Sarah Jones works for Acme.";
    static readonly PreviousRunInfo Info = new("final-20261004", "2026-10-04T15:11:22Z");

    readonly string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
    string In => Path.Combine(root, "in");
    readonly RedactorOptions options;

    /// <summary>A stored-result source that returns what it is given and records what it was asked.</summary>
    sealed class FakeSource(params PreviousRunData[] runs) : IPreviousRunSource
    {
        public List<string> Asked { get; } = [];
        public Exception? Fail { get; set; }
        public TaskCompletionSource? Gate { get; set; }

        public async Task<IReadOnlyList<PreviousRunData>> FindAsync(string relativePath, CancellationToken ct)
        {
            Asked.Add(relativePath);
            if (Gate is not null)
            {
                await Gate.Task;
            }

            return Fail is null ? runs : throw Fail;
        }
    }

    public PreviousRunTests()
    {
        Directory.CreateDirectory(In);
        options = new RedactorOptions {
            Input = { Include = ["*.txt"] },
            Output = { Directory = Path.Combine(root, "out") },
            Llm = { Model = "phi4", CandidateModels = ["phi4"], ChunkChars = 1000, ChunkOverlapChars = 0 },
        };
    }

    public void Dispose() => Directory.Delete(root, true);

    static List<DetectedEntity> Spans(params (string Type, string Word)[] finds) =>
        [.. finds.SelectMany(f => Enumerable.Range(0, Text.Length).Where(i => string.CompareOrdinal(Text, i, f.Word, 0, f.Word.Length) == 0).Select(i => new DetectedEntity(f.Type, i, f.Word.Length, 1, "llm")))];

    static PreviousRunData Stored(string model, IReadOnlyList<DetectedEntity> spans, string? hash = null) =>
        new(Info, model, hash ?? PreviousRunBuilder.HashText(Text), spans, TimeSpan.FromSeconds(2), 10, 5, 0);

    RedactionSession NewSession(IPreviousRunSource? source, params (string Type, string Text)[] liveFinds) =>
        new(options, In, [new TextDocumentReader()], [new TextDocumentWriter()], new SessionTests.FakeCatalog("phi4"), _ => new SessionTests.FakeModel(liveFinds)) { PreviousRunSource = source };

    async Task<DocumentItem> SelectAsync(RedactionSession s, string name = "a.txt", string text = Text)
    {
        var path = Path.Combine(In, name);
        if (!File.Exists(path))
        {
            File.WriteAllText(path, text);
        }

        s.Refresh();
        var item = s.Documents.Items.Single(i => i.RelativePath == name);
        await s.SelectAsync(item);
        await s.PreviousRunsReady;
        return item;
    }

    /// <summary>The rebuilt result is exactly what the live code makes from the same spans.</summary>
    [Fact]
    public void Rebuilt_result_equals_what_a_live_run_makes_from_the_same_spans()
    {
        var spans = Spans(("PERSON", "Sarah Jones"), ("ORG", "Acme"));

        var built = PreviousRunBuilder.Build(Stored("phi4", spans), Text, Template)!;

        var live = Redactor.Apply(Text, spans, Template);
        Assert.Equal(live.RedactedText, built.Result.RedactedText);
        Assert.Equal(live.Edits, built.Result.Edits);
        Assert.True(built.IsPreviousRun);
        Assert.Equal(Info, built.PreviousRun);
    }

    /// <summary>A result made from a different text is refused, so nothing can be shown out of line.</summary>
    [Fact]
    public void A_text_that_does_not_match_is_refused()
    {
        var stored = Stored("phi4", Spans(("PERSON", "Sarah Jones")), hash: PreviousRunBuilder.HashText("something else"));

        Assert.Null(PreviousRunBuilder.Build(stored, Text, Template));
    }

    /// <summary>Flagged spans stay in the text and become Flagged edits, as in a live run.</summary>
    [Fact]
    public void Flagged_spans_stay_in_the_text()
    {
        var flagged = new DetectedEntity("PERSON", 0, 5, 0.9, "gliner-only", Flag: true);

        var built = PreviousRunBuilder.Build(Stored("phi4", [flagged]), Text, Template)!;

        Assert.Equal(Text, built.Result.RedactedText);
        Assert.Equal(EditStatus.Flagged, Assert.Single(built.Result.Edits).Status);
    }

    /// <summary>Picking a document shows one stored result per model, shown through the same ActiveResult the panels read.</summary>
    [Fact]
    public async Task Picking_a_document_pre_populates_the_stored_results()
    {
        var source = new FakeSource(Stored("phi4", Spans(("PERSON", "Sarah Jones"))), Stored("gemma", Spans(("PERSON", "Sarah Jones"), ("ORG", "Acme"))));
        var s = NewSession(source);

        await SelectAsync(s);

        Assert.Equal(["a.txt"], source.Asked);
        Assert.Equal(["phi4", "gemma"], s.PreviousRunsForSelected.Select(r => r.Model));
        Assert.Empty(s.ResultsForSelected);
        s.SelectResult(s.PreviousRunsForSelected[1].Id);
        Assert.True(s.IsPreviousRunActive);
        Assert.Equal(s.PreviousRunsForSelected[1].Id, s.ActiveResult!.Id);
        Assert.Equal(3, s.Bookmarks().Count);
    }

    /// <summary>The path asked for is relative to the input folder and uses / on every platform, which is how datasets name documents.</summary>
    [Fact]
    public async Task The_document_is_looked_up_by_its_relative_path_with_slashes()
    {
        var source = new FakeSource();
        var s = NewSession(source);
        Directory.CreateDirectory(Path.Combine(In, "sub"));

        await SelectAsync(s, Path.Combine("sub", "b.txt"));

        Assert.Equal(["sub/b.txt"], source.Asked);
    }

    /// <summary>With no source, or one that fails, the document behaves exactly as it always did.</summary>
    [Fact]
    public async Task No_source_or_a_failing_source_changes_nothing()
    {
        var none = NewSession(null);
        await SelectAsync(none);
        Assert.Empty(none.PreviousRunsForSelected);
        Assert.False(none.PreviousRunsLoading);

        var failing = NewSession(new FakeSource { Fail = new InvalidDataException("bad dataset") });
        await SelectAsync(failing);
        Assert.Empty(failing.PreviousRunsForSelected);
        Assert.False(failing.PreviousRunsLoading);
        Assert.NotNull(failing.OriginalText);
    }

    /// <summary>Running a model live adds its own result and leaves the stored one for the same model in place, unmerged.</summary>
    [Fact]
    public async Task A_live_run_does_not_replace_a_stored_result_for_the_same_model()
    {
        var s = NewSession(new FakeSource(Stored("phi4", Spans(("PERSON", "Sarah Jones")))), ("PERSON", "Sarah"));
        await SelectAsync(s);
        await s.LoadModelsAsync();

        await s.RunAsync();

        Assert.Single(s.ResultsForSelected);
        var stored = Assert.Single(s.PreviousRunsForSelected);
        Assert.Equal("phi4", stored.Model);
        Assert.NotEqual(s.ResultsForSelected[0].Id, stored.Id);
        Assert.False(s.ResultsForSelected[0].IsPreviousRun);
    }

    /// <summary>A stored result does not take part in the live results' confidence voting, and its own grade is not "run more models" advice.</summary>
    [Fact]
    public async Task A_stored_result_is_graded_on_its_own()
    {
        var s = NewSession(new FakeSource(Stored("gemma", Spans(("PERSON", "Sarah Jones")))));
        await SelectAsync(s);
        s.SelectResult(s.PreviousRunsForSelected[0].Id);

        var grades = s.Confidence();

        Assert.Equal([s.PreviousRunsForSelected[0].Id], s.Voters.Select(v => v.Id));
        Assert.All(grades.Values, g => Assert.Contains("stored run", g.Reason));
    }

    /// <summary>Selecting another document while a lookup is slow does not put the first document's results on the second.</summary>
    [Fact]
    public async Task A_late_lookup_does_not_land_on_another_document()
    {
        var gate = new TaskCompletionSource();
        var source = new FakeSource(Stored("phi4", Spans(("PERSON", "Sarah Jones")))) { Gate = gate };
        var s = NewSession(source);
        File.WriteAllText(Path.Combine(In, "a.txt"), Text);
        File.WriteAllText(Path.Combine(In, "b.txt"), "different text");
        s.Refresh();
        await s.SelectAsync(s.Documents.Items.Single(i => i.Name == "a.txt"));   // the lookup is now waiting on the gate
        await s.SelectAsync(s.Documents.Items.Single(i => i.Name == "b.txt"));

        gate.SetResult();
        await s.PreviousRunsReady;

        Assert.Empty(s.PreviousRunsForSelected);   // b.txt is selected, and a.txt's results are not shown on it
    }

    /// <summary>A stored result is read-only: no edit, review change, undo or save does anything, and nothing is written to the output folder.</summary>
    [Fact]
    public async Task A_stored_result_cannot_be_changed_or_saved()
    {
        var flagged = new DetectedEntity("ORG", Text.IndexOf("Acme", StringComparison.Ordinal), 4, 0.5, "gliner-only", Flag: true);
        var s = NewSession(new FakeSource(Stored("phi4", [.. Spans(("PERSON", "Sarah Jones")), flagged])));
        await SelectAsync(s);
        var stored = s.PreviousRunsForSelected[0];
        s.SelectResult(stored.Id);
        var before = s.ActiveResult!.Result;
        var firstEdit = before.Edits.First(e => e.Status == EditStatus.Active).Id;
        var flaggedEdit = before.Edits.First(e => e.Status == EditStatus.Flagged).Id;

        Assert.False(s.CanReview);
        Assert.Equal(0, s.AddManual(0, 5, "PERSON", true));
        Assert.False(s.AddArea(0, 10, 10, 50, 50));
        s.RejectEdit(firstEdit);
        s.AcceptFlag(flaggedEdit);
        s.RestoreEdit(firstEdit);
        s.ChangeType(firstEdit, "ORG");
        s.RemoveArea(Bookmark.AreaIdBase);
        s.Undo();
        s.Redo();
        await s.SaveAsync();
        await s.UseAsOutputAsync(stored.Id);

        Assert.Same(before, s.ActiveResult!.Result);
        Assert.Equal(EditStatus.Active, s.ActiveResult!.Result.Edits.First(e => e.Id == firstEdit).Status);
        Assert.False(s.CanUndo);
        Assert.False(s.CanRedo);
        Assert.False(s.IsModified);
        Assert.Null(s.OutputResultId);
        Assert.False(Directory.Exists(options.Output.Directory) && Directory.GetFiles(options.Output.Directory, "*", SearchOption.AllDirectories).Length > 0);
    }

    /// <summary>A stored result on a PDF can be shown as page images: the redacted PDF is built in memory from the stored edits, has no text layer, and the source file is left alone.</summary>
    [Fact]
    public async Task A_stored_result_on_a_pdf_renders_the_redacted_pages_without_changing_the_file()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "tests", "TestCorpus", "pdf", "01-hr-letter.pdf")))
        {
            dir = Path.GetDirectoryName(dir);
        }

        var path = Path.Combine(In, "letter.pdf");
        File.Copy(Path.Combine(dir ?? throw new FileNotFoundException("01-hr-letter.pdf"), "tests", "TestCorpus", "pdf", "01-hr-letter.pdf"), path);
        var pdfText = (await new PdfDocumentReader().ReadAsync(path, default)).Text;
        var at = pdfText.IndexOf("Eleanor Whitcombe", StringComparison.Ordinal);
        var stored = new PreviousRunData(Info, "phi4", PreviousRunBuilder.HashText(pdfText), [new DetectedEntity("PERSON", at, "Eleanor Whitcombe".Length, 1, "llm")], TimeSpan.FromSeconds(1), 1, 1, 0);
        var s = new RedactionSession(new RedactorOptions { Input = { Include = ["*.pdf"] }, Output = { Directory = Path.Combine(root, "out") } }, In,
            [new PdfDocumentReader()], [new PdfDocumentWriter(new PdfOptions())], new SessionTests.FakeCatalog("phi4"), _ => new SessionTests.FakeModel()) { PreviousRunSource = new FakeSource(stored) };
        var bytesBefore = File.ReadAllBytes(path);
        s.Refresh();
        await s.SelectAsync(s.Documents.Items.Single());
        await s.PreviousRunsReady;
        var run = Assert.Single(s.PreviousRunsForSelected);

        var rendered = await s.RenderRedactedAsync(run.Id);

        Assert.NotNull(rendered);
        Assert.Equal("application/pdf", rendered.Value.ContentType);
        using (var check = UglyToad.PdfPig.PdfDocument.Open(rendered.Value.Bytes))
        {
            Assert.Equal(0, check.GetPages().Sum(p => p.GetWords().Count()));   // redacted output has no text layer
        }

        Assert.NotEqual(0, s.RenderStamp(run.Id));
        Assert.Equal(bytesBefore, File.ReadAllBytes(path));
    }

    /// <summary>Loading and viewing a stored result never touches the input file or the document the session holds.</summary>
    [Fact]
    public async Task Viewing_a_stored_result_never_changes_the_source_document()
    {
        var s = NewSession(new FakeSource(Stored("phi4", Spans(("PERSON", "Sarah Jones")))));
        var path = Path.Combine(In, "a.txt");
        File.WriteAllText(path, Text);
        var bytesBefore = File.ReadAllBytes(path);
        var stampBefore = File.GetLastWriteTimeUtc(path);

        await SelectAsync(s);
        s.SelectResult(s.PreviousRunsForSelected[0].Id);
        _ = s.Bookmarks();

        Assert.Equal(Text, s.OriginalText);
        Assert.Equal(bytesBefore, File.ReadAllBytes(path));
        Assert.Equal(stampBefore, File.GetLastWriteTimeUtc(path));
        Assert.Equal(["a.txt"], Directory.GetFiles(In).Select(Path.GetFileName));
        Assert.False(Directory.Exists(Path.Combine(root, "out")) && Directory.GetFiles(Path.Combine(root, "out")).Length > 0);
    }
}
