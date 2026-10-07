using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.App.ViewModels;

/// <summary>One model's redaction of one document. Kept in memory (and later as offsets only, FR9).</summary>
public record ModelResult(Guid Id, string Model, ModelInfo? Info, RedactionResult Result, TimeSpan Elapsed,
    long PromptTokens, long OutputTokens, int Discarded, DateTime AtUtc)
{
    /// <summary>Every call made to the model for this result (empty if the detector does not record them).</summary>
    public IReadOnlyList<ModelCall> Calls { get; init; } = [];
    /// <summary>What the clean-up rules left out of this run (positions only; the text is read from the document). Empty when no rule is switched on.</summary>
    public IReadOnlyList<SuppressedItem> Suppressed { get; init; } = [];
    /// <summary>What the model itself produced, before any review changes. Result is this with the reviewer's changes applied.</summary>
    public RedactionResult? BaseResult
    {
        get; init;
    }
    /// <summary>Where this result came from if it is a stored result from an earlier evaluation run (null for a live result).</summary>
    public PreviousRunInfo? PreviousRun
    {
        get; init;
    }
    /// <summary>True for a stored result from an earlier run. These are read-only.</summary>
    public bool IsPreviousRun => PreviousRun is not null;
    /// <summary>True for the result a person builds by hand, with no model involved.</summary>
    public bool IsManual => Model == ManualModelName;
    /// <summary>The name shown for the hand-made result.</summary>
    public const string ManualModelName = "Manual";

    /// <summary>Number of redactions that are currently applied.</summary>
    public int EditCount => Result.Edits.Count(e => e.Status == EditStatus.Active) + Result.AreaList.Count;
    /// <summary>Model output speed.</summary>
    public double TokensPerSecond => Elapsed.TotalSeconds > 0 ? OutputTokens / Elapsed.TotalSeconds : 0;
}

/// <summary>A row in the bookmark list. Built from the REDACTED text, so it never contains sensitive text.</summary>
public record Bookmark(int Id, string Type, string Snippet, int Line, int OriginalStart, int RedactedStart, EditConfidence Confidence, bool Flagged = false, string Source = "llm", bool Rejected = false, bool IsArea = false, int Page = 0)
{
    /// <summary>Numbers for hand-drawn areas start here so they never clash with edit numbers.</summary>
    public const int AreaIdBase = 1_000_000;
}
