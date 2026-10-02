using AiDocumentRedactor.Core;
using AiDocumentRedactor.Eval;
using Xunit;

namespace AiDocumentRedactor.Tests;

/// <summary>Tests for the evaluation's error bars, by-type table, answer-key-gap list, and the voting used to combine detectors.</summary>
public class EvalExtrasTests
{
    /// <summary>The 95% range for 95 of 100 is about 88.8% to 97.8%, an empty sample says nothing, and a full score is not 100% sure.</summary>
    [Fact]
    public void Wilson_ranges()
    {
        var (lo, hi) = MarkdownReport.Wilson(95, 100);
        Assert.InRange(lo, 0.885, 0.892);
        Assert.InRange(hi, 0.975, 0.980);
        Assert.Equal((0.0, 1.0), MarkdownReport.Wilson(0, 0));
        Assert.True(MarkdownReport.Wilson(100, 100).Low < 1);
        Assert.Equal(1.0, MarkdownReport.Wilson(100, 100).High, 6);
    }

    static DetectedEntity Span(int start, int length, string source = "llm", bool flag = false) => new("PERSON", start, length, 1, source, flag);

    /// <summary>Spans from different detectors are grouped where they overlap; a group needs enough different detectors, and the rest are flagged or dropped.</summary>
    [Fact]
    public void Vote_needs_enough_detectors()
    {
        IReadOnlyList<IReadOnlyList<DetectedEntity>> voters = [[Span(0, 5), Span(20, 4)], [Span(1, 6), Span(40, 3)]];
        var agreed = Combos.Vote(voters, 2, false);
        var one = Assert.Single(agreed);
        Assert.Equal(0, one.Start);
        Assert.False(one.Flag);

        var flagged = Combos.Vote(voters, 2, true);
        Assert.Equal(3, flagged.Count);
        Assert.Equal(2, flagged.Count(f => f.Flag && f.Source == "single-detector"));
        Assert.Single(flagged, f => !f.Flag);

        Assert.Equal(3, Combos.Vote(voters, 1, false).Count);   // the two overlapping spans form one group
    }

    /// <summary>One detector's own flagged spans pass through a vote unchanged.</summary>
    [Fact]
    public void Vote_passes_existing_flags_through()
    {
        var result = Combos.Vote([[Span(0, 5, "llm", true)], [Span(0, 5)]], 2, false);
        Assert.Contains(result, r => r.Flag);
    }

    static DocScore Doc(string model, string file, string type, int present, int caught, params string[] over)
    {
        var s = new DocScore { File = file, Model = model, Group = "Plain text", DocType = type, Present = present, Caught = caught, Edits = caught + over.Length, TruePositives = caught, EntitiesPresent = 1 };
        s.FalsePositives.AddRange(over.Select(o => new FalsePositive("AGE", o)));
        return s;
    }

    /// <summary>The report shows ranges, a by-type table, no-model baselines kept out of the headline, and answer-key gaps when most models over-redact the same text.</summary>
    [Fact]
    public void Report_has_ranges_types_baselines_and_key_gaps()
    {
        var models = new List<string> { "rules only", "a", "b", "c" };
        var scores = new List<DocScore>();
        foreach (var m in models)
        {
            scores.Add(Doc(m, "x/1.txt", "Email", 10, m == "rules only" ? 6 : 10, m == "rules only" ? [] : ["61"]));
            scores.Add(Doc(m, "x/2.txt", "Loan", 5, 5));
        }

        var run = new RunInfo(DateTime.Now, TimeSpan.FromMinutes(1), "test", "c", 2, 15, new RedactorOptions(), models.Select(m => (m, (ModelInfo?)null)).ToList(), true, false, []);
        var md = MarkdownReport.Build(run, scores);
        Assert.Contains("Recall (95% range)", md);
        Assert.Contains("## Recall by document type", md);
        Assert.Contains("Email", md);
        Assert.Contains("### Likely answer-key gaps", md);
        Assert.Contains("“61”", md);
        var headline = md[md.IndexOf("## Headline findings", StringComparison.Ordinal)..];
        Assert.DoesNotContain("**Best at finding sensitive data:** rules only", headline[..400]);
        Assert.True(MarkdownReport.IsPlain("phi4"));
        Assert.False(MarkdownReport.IsPlain("phi4 + GLiNER"));
        Assert.False(MarkdownReport.IsPlain("rules only"));
    }
}
