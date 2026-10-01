using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.App.ViewModels;

/// <summary>One model's redaction of one document. Kept in memory (and later as offsets only, FR9).</summary>
public record ModelResult(Guid Id, string Model, ModelInfo? Info, RedactionResult Result, TimeSpan Elapsed,
    long PromptTokens, long OutputTokens, int Discarded, DateTime AtUtc)
{
    /// <summary>Every call made to the model for this result (empty if the detector does not record them).</summary>
    public IReadOnlyList<ModelCall> Calls { get; init; } = [];
    /// <summary>What the model itself produced, before any review changes. Result is this with the reviewer's changes applied.</summary>
    public RedactionResult? BaseResult { get; init; }
    /// <summary>True for the result a person builds by hand, with no model involved.</summary>
    public bool IsManual => Model == ManualModelName;
    /// <summary>The name shown for the hand-made result.</summary>
    public const string ManualModelName = "Manual";

    /// <summary>Number of redactions that are currently applied.</summary>
    public int EditCount => Result.Edits.Count(e => e.Status == EditStatus.Active);
    /// <summary>Model output speed.</summary>
    public double TokensPerSecond => Elapsed.TotalSeconds > 0 ? OutputTokens / Elapsed.TotalSeconds : 0;
}

/// <summary>A row in the bookmark list. Built from the REDACTED text, so it never contains sensitive text.</summary>
public record Bookmark(int Id, string Type, string Snippet, int Line, int OriginalStart, int RedactedStart, EditConfidence Confidence, bool Flagged = false, string Source = "llm", bool Rejected = false);
