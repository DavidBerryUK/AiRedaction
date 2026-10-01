using AiDocumentRedactor.Core;
using AiDocumentRedactor.Eval;
using Xunit;

namespace AiDocumentRedactor.Tests;

/// <summary>Tests for the evaluation scoring: recall, precision, over-redaction of must-keep text, and the Markdown report.</summary>
public class EvalTests
{
    const string Text = "Eleanor Whitcombe of Acme Ltd lives in London.\nCall 0113 496 0123. Eleanor again.";
    static readonly GroundTruth Key = new("t", "t",
    [
        new("body", "PERSON", "Eleanor Whitcombe", 1), new("body", "PERSON", "Eleanor", 2), new("body", "COMPANY", "Acme Ltd", 1),
        new("body", "PHONE", "0113 496 0123", 1),
    ], ["London"]);

    /// <summary>Redacts the given strings (every occurrence) with the given type.</summary>
    static RedactionResult Redact(params (string Type, string Text)[] spans) =>
        Redactor.Apply(Text, spans.SelectMany(s => Scoring.Find(Text, s.Text).Select(p => new DetectedEntity(s.Type, p.Start, p.Length, 1, "test"))), "[REDACTED:{type}]");

    /// <summary>A perfect redaction scores full recall and precision and leaves the must-keep text alone.</summary>
    [Fact]
    public void Perfect_redaction()
    {
        var s = Scoring.Score(Text, Redact(("PERSON", "Eleanor Whitcombe"), ("PERSON", "Eleanor"), ("COMPANY", "Acme Ltd"), ("PHONE", "0113 496 0123")), Key, "Plain text");
        Assert.Equal(s.Present, s.Caught);
        Assert.Equal(s.Edits, s.TruePositives);
        Assert.Empty(s.Leaks); Assert.Empty(s.PreserveBroken);
        Assert.Equal(1, s.MustPreserve);
    }

    /// <summary>A missed item is a leak, a redaction off the key is a false positive, and redacting must-keep text is counted.</summary>
    [Fact]
    public void Misses_false_positives_and_damage()
    {
        var s = Scoring.Score(Text, Redact(("PERSON", "Eleanor Whitcombe"), ("ADDRESS", "London")), Key, "Plain text");
        Assert.Contains(s.Leaks, l => l is { Type: "COMPANY", Text: "Acme Ltd" });
        Assert.Contains(s.Leaks, l => l is { Type: "PHONE" });
        Assert.Contains(s.FalsePositives, f => f.Text == "London");
        Assert.Contains("London", s.PreserveBroken);
        Assert.True(s.Caught < s.Present);
        Assert.Equal(1, s.TruePositives);
    }

    /// <summary>Matching tolerates line breaks inside an item, as in a wrapped address, and ignores partial words.</summary>
    [Fact]
    public void Matching_is_whole_word_and_spacing_tolerant()
    {
        Assert.Single(Scoring.Find("Unit 4,\nKingfisher House", "Unit 4, Kingfisher House"));
        Assert.Empty(Scoring.Find("Whitcombes", "Whitcombe"));
        Assert.Single(Scoring.Find("QQ 12 34 56 C", "QQ 12 34 56 C"));
    }

    /// <summary>An item the extracted text never contained (an OCR misread) is counted separately and not as a miss.</summary>
    [Fact]
    public void Items_lost_before_the_model_are_counted_separately()
    {
        var s = Scoring.Score("Eleanor Whitc0mbe", Redactor.Apply("Eleanor Whitc0mbe", [], "[R]"), new("t", "t", [new("body", "PERSON", "Eleanor Whitcombe", 1)], []), "Scan: clean image");
        Assert.Equal(1, s.LostToExtraction); Assert.Equal(0, s.Present);
    }

    /// <summary>Every corpus file name finds its answer key and a format group.</summary>
    [Fact]
    public void Corpus_files_map_to_answer_keys()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null && !Directory.Exists(Path.Combine(dir, "tests", "TestCorpus"))) dir = Path.GetDirectoryName(dir);
        var corpus = Path.Combine(dir!, "tests", "TestCorpus");
        var keys = GroundTruthStore.Load(corpus);
        Assert.Equal(12, keys.Count);
        Assert.Equal("01-hr-letter", GroundTruthStore.For("01-hr-letter-scan-degraded.jpg", keys)!.Id);
        Assert.Equal("Scan: image-only PDF", GroundTruthStore.FormatGroup("x/08-invoice-scan.pdf"));
        Assert.Equal("Word", GroundTruthStore.FormatGroup("x/02-services-agreement.docx"));
    }

    /// <summary>The report has the main sections and, unless asked, contains no document text.</summary>
    [Fact]
    public void Report_has_sections_and_no_text_by_default()
    {
        var score = Scoring.Score(Text, Redact(("PERSON", "Eleanor Whitcombe")), Key, "Plain text"); score.File = "a.txt"; score.Model = "m1";
        var run = new RunInfo(DateTime.Now, TimeSpan.FromMinutes(1), "test", "c", 1, 4, new RedactorOptions(), [("m1", null)], false, false, []);
        var md = MarkdownReport.Build(run, [score]);
        foreach (var h in new[] { "# Redaction evaluation report", "## Summary", "## Headline findings", "## Recall by category", "## Recall by document format", "## Per document", "## How to read this" })
            Assert.Contains(h, md);
        Assert.DoesNotContain("Acme", md);   // missed in this run, so it would show only with text on
        Assert.Contains("Acme Ltd", MarkdownReport.Build(run with { ShowText = true }, [score]));
    }
}
