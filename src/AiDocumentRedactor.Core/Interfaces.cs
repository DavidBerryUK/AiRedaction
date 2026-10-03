namespace AiDocumentRedactor.Core;

/// <summary>Reads a document file and extracts its text. One implementation per file format.</summary>
public interface IDocumentReader
{
    /// <summary>True if this reader handles the file (decided by extension).</summary>
    bool CanRead(string path);
    /// <summary>Reads the file and returns its text and format.</summary>
    Task<ExtractedDocument> ReadAsync(string path, CancellationToken ct);
}

/// <summary>Writes a redacted document to disk in its original format.</summary>
public interface IDocumentWriter
{
    /// <summary>True if this writer handles the given document's format.</summary>
    bool CanWrite(ExtractedDocument source);
    /// <summary>Writes the redacted result to <c>outputPath</c>.</summary>
    Task WriteAsync(ExtractedDocument source, RedactionResult result, string outputPath, CancellationToken ct);
}

/// <summary>Finds sensitive data in text. Returns positions only; it never edits the text (spec §7.2).</summary>
public interface IEntityDetector
{
    /// <summary>Detects sensitive spans in <c>text</c>, reporting progress as it goes.</summary>
    Task<IReadOnlyList<DetectedEntity>> DetectAsync(string text, IProgress<RedactionProgress>? progress, CancellationToken ct);
}

/// <summary>Optional: writers that can also render a redacted document straight to a stream (for the in-app viewer, no file on disk).</summary>
public interface IStreamDocumentWriter
{
    /// <summary>Renders the redacted result to the stream.</summary>
    Task WriteAsync(ExtractedDocument source, RedactionResult result, Stream output, CancellationToken ct);
}

/// <summary>Something a clean-up rule decided not to redact: which rule, the category the detector gave it, and where it is. Positions only; the text is read from the document.</summary>
public record SuppressedItem(string Rule, string Type, int Start, int Length);

/// <summary>The names of the clean-up rules and a plain-English description of each, for the review screen.</summary>
public static class CleanUpRuleNames
{
    public const string Bracketed = "bracketed-placeholder", Masked = "masked-value", Generic = "generic-term", Birth = "birth-date-context";

    /// <summary>A plain-English name for a rule.</summary>
    public static string Label(string rule) => rule switch
    {
        Bracketed => "a template placeholder (text in brackets, or a date template)",
        Masked => "a masked or blank value",
        Generic => "a generic term for a role in a contract",
        Birth => "a date with no birth wording near it",
        _ => rule,
    };
}

/// <summary>Optional: detectors that can say what their clean-up rules left out of the last run, so a reviewer can check them.</summary>
public interface IDetectorSuppression
{
    /// <summary>What the clean-up rules left out in the last detection.</summary>
    IReadOnlyList<SuppressedItem> LastSuppressed { get; }
}

/// <summary>Optional: detectors that can report per-run token usage (FR33).</summary>
public interface IDetectorMetrics
{
    /// <summary>Tokens sent to the model.</summary>
    long PromptTokens
    {
        get;
    }
    /// <summary>Tokens the model generated.</summary>
    long OutputTokens
    {
        get;
    }
    /// <summary>Model answers dropped because they were not found verbatim in the text.</summary>
    int Discarded
    {
        get;
    }
}
