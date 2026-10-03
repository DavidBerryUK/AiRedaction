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

/// <summary>Tests for scoring against a key that judges only some categories, and for the rules that audit a generated answer key.</summary>
public class KeyAuditTests
{
    const string Text = "Call Eleanor on 07700 900123. She works at Acme Ltd and he agreed.";

    static RedactionResult Redact(params (string Type, string Text)[] spans) =>
        Redactor.Apply(Text, spans.SelectMany(s => Scoring.Find(Text, s.Text).Select(p => new DetectedEntity(s.Type, p.Start, p.Length, 1, "test"))), "[REDACTED:{type}]");

    /// <summary>With a key that does not judge GENDER, redacting "She" is neither right nor wrong; an unjudged category stays wrong only when the key judges it.</summary>
    [Fact]
    public void Unjudged_categories_and_ignored_text_are_not_counted()
    {
        var key = new GroundTruth("t", "t", [new("body", "PHONE", "07700 900123", 1)], [], ["PHONE", "COMPANY"], ["Eleanor"]);
        var s = Scoring.Score(Text, Redact(("PHONE", "07700 900123"), ("GENDER", "She"), ("PERSON", "Eleanor"), ("COMPANY", "Acme Ltd")), key, "Plain text");
        Assert.Equal(2, s.Edits);              // the phone number and Acme Ltd
        Assert.Equal(1, s.TruePositives);      // only the phone number is on the key
        Assert.Equal(2, s.Unjudged);           // GENDER (not judged) and Eleanor (ignored)
        Assert.Single(s.FalsePositives);       // Acme Ltd: the key judges COMPANY and does not list it
        Assert.True(key.Judges("COMPANY"));
        Assert.False(key.Judges("GENDER"));
        Assert.True(new GroundTruth("t", "t", [], null).Judges("GENDER"));   // no list: everything is judged
    }

    /// <summary>Things the models redact that are not sensitive are rejected, real ones added, and what a rule cannot decide is left out.</summary>
    [Theory]
    [InlineData("COMPANY", "[Company Name]", AuditDecision.Reject)]
    [InlineData("COMPANY", "HMRC", AuditDecision.Reject)]
    [InlineData("COMPANY", "Borrower", AuditDecision.Reject)]
    [InlineData("COMPANY", "United Bank of Canada", AuditDecision.Add)]
    [InlineData("COMPANY", "XMYTGBDH508", AuditDecision.Add)]
    [InlineData("COMPANY", "CHASUS33", AuditDecision.Ignore)]
    [InlineData("COMPANY", "Netflix", AuditDecision.Ignore)]
    [InlineData("GENDER", "She", AuditDecision.Ignore)]
    [InlineData("PHONE", "+44 973 771 5733", AuditDecision.Add)]
    [InlineData("EMAIL", "a@b.example", AuditDecision.Add)]
    [InlineData("ID_NUMBER", "12345678", AuditDecision.Add)]
    [InlineData("ID_NUMBER", "08-32-10", AuditDecision.Add)]
    [InlineData("ID_NUMBER", "**** 1234", AuditDecision.Reject)]
    [InlineData("ID_NUMBER", "ABC017", AuditDecision.Reject)]
    [InlineData("ID_NUMBER", "US1234567890", AuditDecision.Reject)]
    [InlineData("ADDRESS", "654 Pine St", AuditDecision.Add)]
    [InlineData("ADDRESS", "London, UK", AuditDecision.Ignore)]
    [InlineData("PERSON", "Heidi", AuditDecision.Add)]
    [InlineData("DATE_OF_BIRTH", "1989-12-12", AuditDecision.Add)]
    [InlineData("DATE_OF_BIRTH", "01 June 2023", AuditDecision.Reject)]
    [InlineData("ONLINE_ID", "https://accounts.example.com/reset", AuditDecision.Reject)]
    public void Gap_rules(string type, string text, AuditDecision expected) => Assert.Equal(expected, KeyAudit.ClassifyGap(type, text));

    /// <summary>A key item most models missed is removed when it is noise, and kept when it is a real item.</summary>
    [Theory]
    [InlineData("ADDRESS", "address", AuditDecision.Remove)]
    [InlineData("DATE_OF_BIRTH", "[Date of Birth]", AuditDecision.Remove)]
    [InlineData("COMPANY", "---------------------", AuditDecision.Remove)]
    [InlineData("COMPANY", "Educational Institution", AuditDecision.Remove)]
    [InlineData("COMPANY", "Fernleigh Surgery", AuditDecision.Keep)]
    [InlineData("ADDRESS", "London, UK", AuditDecision.Remove)]
    [InlineData("ADDRESS", "San Francisco, CA 94112", AuditDecision.Keep)]
    [InlineData("ID_NUMBER", "568", AuditDecision.Remove)]
    [InlineData("ONLINE_ID", "a741:45da:c53e:2f8:835a:e766:162b:4220", AuditDecision.Keep)]
    public void Miss_rules(string type, string text, AuditDecision expected) => Assert.Equal(expected, KeyAudit.ClassifyMiss(type, text));
}
