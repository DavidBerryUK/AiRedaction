using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.App.ViewModels;

/// <summary>Where a stored result came from: the evaluation dataset and the date that dataset was made.</summary>
public record PreviousRunInfo(string DatasetId, string DatasetCreatedAt);

/// <summary>How a stored run scored against the document's answer key, as the evaluation recorded it. An occurrence is one appearance in the text; an item is one distinct thing on the key
/// (it can appear several times). The flag figures are null for a run that used no GLiNER.</summary>
public record PreviousRunScores(int Present, int Caught, int ItemsPresent, int ItemsFullyCaught, int LostToExtraction, int Redactions, int Matching, int RightCategory, int NotJudged,
    int OverRedactions, int MustKeepTotal, int MustKeepDamaged, int? FlagsRaised, int? FlagsCorrect, double DetectSeconds, double? SecondOpinionSeconds)
{
    /// <summary>Occurrences that should have been removed and were not (leaks).</summary>
    public int Missed => Math.Max(0, Present - Caught);

    /// <summary>Share of the sensitive occurrences that were removed (null if the key has none).</summary>
    public double? Recall => Present == 0 ? null : (double)Caught / Present;

    /// <summary>Share of the redactions that match something on the answer key (null if there were none).</summary>
    public double? Precision => Redactions == 0 ? null : (double)Matching / Redactions;

    /// <summary>One score combining recall and precision (null if either is not defined).</summary>
    public double? F1 => Recall is { } r && Precision is { } p ? (r + p == 0 ? 0 : 2 * r * p / (r + p)) : null;
}

/// <summary>One model's stored result for a document: the redactions and flags it produced as positions in the document's text, the hash of the text those positions refer to, how it
/// scored and its timings. The dataset's own text is deliberately not carried: it is never shown.</summary>
public record PreviousRunData(PreviousRunInfo Info, string Model, string TextHash, IReadOnlyList<DetectedEntity> Spans, TimeSpan Elapsed, long PromptTokens, long OutputTokens, int Discarded)
{
    /// <summary>How the run scored, if the dataset recorded it.</summary>
    public PreviousRunScores? Scores
    {
        get; init;
    }
}

/// <summary>Finds the stored results for a document. Implemented by the web host (over the evaluation datasets); the session only uses this interface so the view models do not depend on the
/// results explorer.</summary>
public interface IPreviousRunSource
{
    /// <summary>One stored result per model for the document at <paramref name="relativePath"/> (relative to the input folder, with / separators), or an empty list.</summary>
    Task<IReadOnlyList<PreviousRunData>> FindAsync(string relativePath, CancellationToken ct);
}

/// <summary>Rebuilds a stored result so it can be shown in the same panels as a live one. Only reads: the document is never changed.</summary>
public static class PreviousRunBuilder
{
    /// <summary>The SHA-256 of a text as lower-case hex, the way the datasets record it.</summary>
    public static string HashText(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    /// <summary>True if the stored positions refer to exactly this text.</summary>
    public static bool Matches(PreviousRunData data, string originalText) => string.Equals(HashText(originalText), data.TextHash, StringComparison.OrdinalIgnoreCase);

    /// <summary>Applies the stored spans to the document's text with the same code a live run uses, so the redacted text and edits are what the run produced. Null if the stored result
    /// was not made from this text (nothing is built, so nothing can be shown out of line).</summary>
    public static ModelResult? Build(PreviousRunData data, string originalText, string placeholderTemplate)
    {
        if (!Matches(data, originalText))
        {
            return null;
        }

        var result = Redactor.Apply(originalText, data.Spans, placeholderTemplate);
        var at = DateTime.TryParse(data.Info.DatasetCreatedAt, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var created) ? created : DateTime.UtcNow;
        return new ModelResult(Guid.NewGuid(), data.Model, null, result, data.Elapsed, data.PromptTokens, data.OutputTokens, data.Discarded, at) { PreviousRun = data.Info, Scores = data.Scores };
    }
}
