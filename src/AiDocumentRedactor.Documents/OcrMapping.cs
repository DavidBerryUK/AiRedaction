using System.Text;
using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.Documents;

/// <summary>A word on a page in PDF points (origin bottom-left), from either a text layer or OCR.</summary>
public record PageWord(string Text, double X, double Y, double W, double H, double? Confidence = null, double[]? Quad = null);

/// <summary>Shared helpers for turning OCR output into the same text-plus-word-positions form used for PDF text layers.</summary>
public static class OcrMapping
{
    /// <summary>Converts OCR lines (pixels, origin top-left) to lines in PDF points (origin bottom-left) for a page of the given size.</summary>
    public static List<List<PageWord>> ToPageLines(OcrPage page, double pageWidthPts, double pageHeightPts)
    {
        double sx = pageWidthPts / page.WidthPx, sy = pageHeightPts / page.HeightPx;
        return page.Lines
            .Select(l => l.Words.Where(w => !string.IsNullOrWhiteSpace(w.Text))
                .Select(w => new PageWord(w.Text, w.X * sx, pageHeightPts - (w.Y + w.Height) * sy, w.Width * sx, w.Height * sy, w.Confidence,
                    w.Corners is { Count: 4 } c ? c.SelectMany(p => new[] { p.X * sx, pageHeightPts - p.Y * sy }).ToArray() : null)).ToList())
            .Where(l => l.Count > 0).ToList();
    }

    /// <summary>Appends one page to the running text and word list: words separated by spaces, lines by line breaks, pages by a blank line.</summary>
    public static void AppendPage(StringBuilder sb, List<WordBox> words, int pageIndex, List<List<PageWord>> lines)
    {
        if (pageIndex > 0) sb.Append("\n\n");
        var firstLine = true;
        foreach (var line in lines)
        {
            if (!firstLine) sb.Append('\n'); firstLine = false;
            for (var i = 0; i < line.Count; i++)
            {
                if (i > 0) sb.Append(' ');
                words.Add(new WordBox(pageIndex, sb.Length, line[i].Text.Length, line[i].X, line[i].Y, line[i].W, line[i].H, line[i].Confidence, line[i].Quad));
                sb.Append(line[i].Text);
            }
        }
    }

    /// <summary>Lower-case letters and digits only, so OCR punctuation differences do not matter when comparing words.</summary>
    public static string Normalise(string s) => new string(s.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

    /// <summary>After redaction, OCR the output and count redacted words (4+ characters) that can still be read. A word counts as
    /// leaked only if it appears more often in the output than the unredacted words of that spelling in the source, so a word that is
    /// legitimately left elsewhere in the document is not mistaken for a leak. Returns only a count, never the words.</summary>
    public static int CountLeaks(ExtractedDocument doc, RedactionResult result, IEnumerable<string> outputWords)
    {
        var covered = PdfDocumentWriter.WordsToCover(doc, result).ToHashSet();
        string Text(WordBox w) => doc.Text.Substring(w.Start, w.Length);
        var redacted = covered.Select(w => Normalise(Text(w))).Where(n => n.Length >= 4).Distinct().ToList();
        var remaining = (doc.Words ?? []).Where(w => !covered.Contains(w)).Select(w => Normalise(Text(w))).GroupBy(n => n).ToDictionary(g => g.Key, g => g.Count());
        var found = outputWords.Select(Normalise).GroupBy(n => n).ToDictionary(g => g.Key, g => g.Count());
        return redacted.Sum(n => Math.Max(0, found.GetValueOrDefault(n) - remaining.GetValueOrDefault(n)));
    }
}
