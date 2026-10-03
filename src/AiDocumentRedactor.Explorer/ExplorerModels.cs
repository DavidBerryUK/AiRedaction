namespace AiDocumentRedactor.Explorer;

/// <summary>Which results to look at. Empty means no restriction (<c>IncludeBaselines</c> false drops the no-model setups). <c>Variants</c> picks the kinds of setup (plain, with GLiNER and so on); <c>Configs</c> picks setups by name.</summary>
public record Filter(string? Corpus = null, string? DocType = null, string? FormatGroup = null, string? Search = null, IReadOnlyList<string>? Variants = null,
    IReadOnlyList<string>? Configs = null, string? Status = null, string? Source = null, bool IncludeBaselines = true);

/// <summary>The standard figures worked out from counts.</summary>
public static class Stats
{
    /// <summary>The 95% Wilson score range for <paramref name="successes"/> out of <paramref name="n"/>. An empty sample says nothing (0 to 1).</summary>
    public static (double Low, double High) Wilson(int successes, int n)
    {
        if (n == 0)
        {
            return (0, 1);
        }

        const double z = 1.96;
        var p = (double)successes / n;
        var denominator = 1 + z * z / n;
        var centre = (p + z * z / (2 * n)) / denominator;
        var margin = z * Math.Sqrt(p * (1 - p) / n + z * z / (4.0 * n * n)) / denominator;
        return (Math.Max(0, centre - margin), Math.Min(1, centre + margin));
    }

    /// <summary>The harmonic mean of recall and precision.</summary>
    public static double F1(double recall, double precision) => recall + precision == 0 ? 0 : 2 * recall * precision / (recall + precision);
}

/// <summary>The totals for one detector setup over the documents that match a filter.</summary>
public record ConfigSummary(string Config, string Model, string Variant, int Documents, int Failed, int Present, int Caught, int Edits, int TruePositives, int Unjudged,
    int PreserveTotal, int PreserveBroken, int FlagsRaised, int FlagsCorrect, double? AvgDetectSeconds, double? AvgGlinerSeconds)
{
    /// <summary>True for the no-model setups (rules only, GLiNER only, rules + GLiNER).</summary>
    public bool IsBaseline => Model.Length == 0;
    public int Missed => Present - Caught;
    public int OverRedactions => Edits - TruePositives;
    public double Recall => Present == 0 ? 0 : (double)Caught / Present;
    public double Precision => Edits == 0 ? 0 : (double)TruePositives / Edits;
    public double F1 => Stats.F1(Recall, Precision);
    public (double Low, double High) RecallRange => Stats.Wilson(Caught, Present);
    public (double Low, double High) PrecisionRange => Stats.Wilson(TruePositives, Edits);
}

/// <summary>One row of the results grid: a detector setup's result on one document.</summary>
public record GridRow(string DocId, string Corpus, string FormatGroup, string DocType, string Config, string Variant, string Status, string? Error, int? Present, int? Caught,
    int? Edits, int? TruePositives, int? Unjudged, int? FlagsRaised, int? FlagsCorrect, int? PreserveBroken, double? DetectSeconds, string Source, string Model = "")
{
    public int? Missed => Present - Caught;
    public int? OverRedactions => Edits - TruePositives;
    public double? Recall => Present is null or 0 ? null : (double)Caught!.Value / Present.Value;
    public double? Precision => Edits is null or 0 ? null : (double)TruePositives!.Value / Edits.Value;
}

/// <summary>A page of rows, with how many there are in all.</summary>
public record Page<T>(IReadOnlyList<T> Rows, int Total);

/// <summary>How hard a document is, from how the language models did on it.</summary>
public enum Difficulty
{
    Easy, Moderate, Hard, Problem
}

/// <summary>One document with its difficulty figures, taken over the language models (not the baselines).</summary>
public record DocumentSummary(string DocId, string Corpus, string FormatGroup, string DocType, int Chars, int Occurrences, int Models, double? AvgRecall, int ModelsWithMisses,
    double AvgMissed, double AvgOverRedactions, int PreserveBroken, int MissedByAll)
{
    /// <summary>The rating, with the rule written out in <see cref="RatingRule"/>: any item every model missed makes a Problem; a low average recall makes it Hard.</summary>
    public Difficulty Rating => Models == 0 ? Difficulty.Easy
        : MissedByAll > 0 ? Difficulty.Problem
        : AvgRecall is < 0.9 || ModelsWithMisses * 2 > Models ? Difficulty.Hard
        : ModelsWithMisses > 0 || AvgOverRedactions >= 3 ? Difficulty.Moderate
        : Difficulty.Easy;

    /// <summary>The rule behind <see cref="Rating"/>, shown beside the ratings.</summary>
    public const string RatingRule = "Problem: at least one item that every model missed. Hard: average recall under 90%, or more than half of the models missed something. "
        + "Moderate: some model missed something, or an average of 3 or more over-redactions. Easy: otherwise. Taken over the language models only.";
}

/// <summary>A key item or redaction, as the document view shows it.</summary>
public record Marker(string Kind, string Type, string Text, int Start, int Length, string? SpanSource, double? Confidence);

/// <summary>A document with its text, how every setup did on it, and the key.</summary>
public record DocumentDetail(string DocId, string KeyId, string Corpus, string FormatGroup, string DocType, string Text, IReadOnlyList<GridRow> Results, IReadOnlyList<KeyItem> Key);

/// <summary>An answer-key row for the document view.</summary>
public record KeyItem(string Role, string Type, string Text, int? Occurrences, string Origin);

/// <summary>What one setup produced on one document: its redactions and flags, and the judged facts.</summary>
public record ConfigView(string Config, IReadOnlyList<Marker> Markers);

/// <summary>Recall and over-redaction for one setup and one category.</summary>
public record CategoryCell(string Config, string Type, int Caught, int Missed, int OverRedactions)
{
    public int Present => Caught + Missed;
    public double? Recall => Present == 0 ? null : (double)Caught / Present;
}

/// <summary>A key item or a redaction of one place, listed with the words around it.</summary>
public record OutcomeItem(string DocId, string Config, string Kind, string Type, string Text, string Before, string After);

/// <summary>Something models keep missing (or keep wrongly redacting), with how many setups and documents it affected.</summary>
public record RepeatedItem(string Type, string Text, int Setups, int Documents, int Occurrences);

/// <summary>What the dataset says about itself: from run.json.</summary>
public record DatasetInfo(string DatasetId, string Status, string CreatedAt, string? GitCommit, string? KeyVersions, string? OllamaVersion, string? Machine, string? Notes, int Documents, int Results);

/// <summary>How one setup treated an item everywhere it occurs: <c>Found</c> is the occurrences it dealt with as the list's kind says (removed a sensitive item, or redacted a non-sensitive
/// one), <c>NotFound</c> the occurrences it did not. For a missed item, <c>Found</c> means it was removed; for a wrongly redacted one, <c>Found</c> means it was redacted.</summary>
public record SetupVerdict(string Config, string Model, string Variant, int Found, int NotFound)
{
    public int Total => Found + NotFound;
}

/// <summary>An item that models miss or wrongly redact, with each setup's record on it and some of the places it occurs.</summary>
public record ItemDetail(string Kind, string Type, string Text, int Occurrences, int Documents, IReadOnlyList<SetupVerdict> Setups, IReadOnlyList<OutcomeItem> Places);
