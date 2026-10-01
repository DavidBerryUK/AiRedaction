namespace AiDocumentRedactor.Core;

/// <summary>Canonical entity type tokens; also the TYPE in [REDACTED:TYPE].</summary>
public static class EntityTypes
{
    public const string Person = "PERSON", Phone = "PHONE", Email = "EMAIL", Address = "ADDRESS",
        IdNumber = "ID_NUMBER", OnlineId = "ONLINE_ID", Age = "AGE", DateOfBirth = "DATE_OF_BIRTH",
        Gender = "GENDER", Company = "COMPANY", CompanyId = "COMPANY_ID", Domain = "DOMAIN",
        Contextual = "CONTEXTUAL", Secret = "SECRET", Other = "OTHER";
}

/// <summary>A span found in the extracted text. Detection never edits text. Source says where it came from: "llm" (returned by the model),
/// "llm-variant" (a shorter form of something the model returned, such as a surname alone), "custom-list" or "human".
/// Flag means: report it for review but do not redact it.</summary>
public record DetectedEntity(string Type, int Start, int Length, double Confidence, string Source, bool Flag = false);

/// <summary>Whether an edit is applied (Active), was rejected by a reviewer (Rejected), or was found but only flagged for review and left in the text (Flagged).</summary>
public enum EditStatus { Active, Rejected, Flagged }

/// <summary>One redaction, with positions in both original and redacted text.</summary>
public record RedactionEdit(
    int Id, string Type,
    int OriginalStart, int OriginalLength,
    int RedactedStart, int RedactedLength,
    string Replacement,
    string? OriginalText,   // in memory only; NEVER persisted (FR9)
    double Confidence, string Source, EditStatus Status);

/// <summary>Where one word of the extracted text sits on its page (PDF points; origin bottom-left, as in PDF files).</summary>
/// Confidence (0 to 1) is set for words read by OCR. Quad is the word's true outline as 8 numbers (4 corners x,y, clockwise from top-left,
/// in the same points space) for OCR words, which on a tilted scan is a tilted rectangle rather than an upright one.
public record WordBox(int Page, int Start, int Length, double X, double Y, double Width, double Height, double? Confidence = null, double[]? Quad = null);

/// <summary>Flattened text of a document plus enough information to write it back.
/// For PDFs and images, Words maps text offsets to page positions and PageSizes gives each page's size in points.
/// OcrConfidence is set (0 to 1) when any text came from OCR instead of a text layer.</summary>
public record ExtractedDocument(string SourcePath, string Format, string Text,
    IReadOnlyList<WordBox>? Words = null, IReadOnlyList<(double Width, double Height)>? PageSizes = null,
    double? OcrConfidence = null);

/// <summary>Raised when a PDF has no text layer (a scan), so it cannot be read without OCR.</summary>
public class NoTextLayerException(string message) : Exception(message);

/// <summary>The steps of a redaction run, reported through <see cref="RedactionProgress"/>.</summary>
public enum RedactionStage { Reading, Chunking, Detecting, Locating, Redacting, Verifying, Writing, Done, Failed, Cancelled }

/// <summary>Progress event. Never contains sensitive text.</summary>
public record RedactionProgress(
    RedactionStage Stage, string Message, int Step, int TotalSteps,
    int ItemsFoundSoFar, TimeSpan Elapsed,
    IReadOnlyList<DetectedEntity>? NewDetections = null);

/// <summary>The outcome of redacting one document: the redacted text and the list of edits made.</summary>
public record RedactionResult(string RedactedText, IReadOnlyList<RedactionEdit> Edits);
