using System.Text.RegularExpressions;

namespace AiDocumentRedactor.Detection;

/// <summary>Finds a piece of text in a document while treating any run of spaces or line breaks as equal, because PDFs and scans
/// often wrap a name or address onto a new line where the model's answer has a plain space.</summary>
public static class TextMatch
{
    /// <summary>A pattern for the text with flexible spacing. <paramref name="wholeWord"/> stops it matching inside a longer word.</summary>
    public static Regex Pattern(string text, bool ignoreCase, bool wholeWord)
    {
        var body = string.Join(@"\s+", text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Select(Regex.Escape));
        var lead = wholeWord && char.IsLetterOrDigit(text.TrimStart()[0]) ? @"(?<![\p{L}\p{N}])" : "";
        var trail = wholeWord && char.IsLetterOrDigit(text.TrimEnd()[^1]) ? @"(?![\p{L}\p{N}])" : "";
        return new Regex(lead + body + trail, RegexOptions.CultureInvariant | (ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None));
    }

    /// <summary>Every place the text occurs, as (start, length) in the document.</summary>
    public static IEnumerable<(int Start, int Length)> Find(string document, string text, bool ignoreCase = false, bool wholeWord = false) =>
        string.IsNullOrWhiteSpace(text) ? [] : Pattern(text, ignoreCase, wholeWord).Matches(document).Select(m => (m.Index, m.Length));

    /// <summary>True if the text occurs in the document, allowing for different spacing.</summary>
    public static bool Contains(string document, string text) => Find(document, text).Any();
}
