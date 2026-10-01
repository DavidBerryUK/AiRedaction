using System.Text.RegularExpressions;

namespace CorpusGenerator;

/// <summary>The kinds of content block a document is built from.</summary>
public enum BlockKind
{
    Heading, Para, Table
}

/// <summary>Content authored with inline markup [[TYPE|text]] so ground truth is exact by construction.</summary>
public record Block(BlockKind Kind, string Text = "", string[][]? Rows = null);

/// <summary>One test document: id, title, the formats to render it to, and its content blocks (plus optional Word-only extras below).</summary>
public record DocDef(string Id, string Title, string[] Formats, List<Block> Blocks)
{
    /// <summary>Docx only: header text (markup allowed).</summary>
    public string? Header
    {
        get; init;
    }
    /// <summary>Docx only: footer text.</summary>
    public string? Footer
    {
        get; init;
    }
    /// <summary>Docx only: a reviewer comment (markup allowed), anchored to the first paragraph.</summary>
    public string? Comment
    {
        get; init;
    }              // docx only: comment text (markup allowed) anchored to first paragraph
    /// <summary>Docx only: text that was deleted with change-tracking on (should be scrubbed).</summary>
    public string? TrackedDeletion
    {
        get; init;
    }      // docx only: text that was deleted with tracking on
    /// <summary>Docx only: fake author stored in the document properties.</summary>
    public string? MetadataAuthor
    {
        get; init;
    }       // docx only: fake author in document properties
    /// <summary>Docx only: split text into many small runs, as Word often does, to test run-level redaction.</summary>
    public bool SplitRuns
    {
        get; init;
    }               // docx only: split text across many runs
    /// <summary>Strings that must NOT be redacted (tests over-redaction).</summary>
    public string[] MustPreserve { get; init; } = [];  // hard negatives that must NOT be redacted
    /// <summary>Csv/json documents: the whole file authored as markup.</summary>
    public string? RawText
    {
        get; init;
    }              // csv/json: whole file authored as markup
    /// <summary>Which scan variants to produce: clean, degraded, pdf.</summary>
    public string[] ScanKinds { get; init; } = [];     // clean, degraded, pdf
}

/// <summary>A sensitive item and its category, as written in the markup.</summary>
public record Entity(string Type, string Text);

/// <summary>Reads the inline markup [[TYPE|text]] used to author documents, so the expected answers are exact.</summary>
public static class Markup
{
    static readonly Regex Rx = new(@"\[\[([A-Z_]+)\|([^\]]+)\]\]", RegexOptions.Compiled);

    /// <summary>Removes the markup, leaving plain text.</summary>
    public static string Strip(string s) => Rx.Replace(s, "$2");
    /// <summary>Lists the sensitive items marked up in a string.</summary>
    public static IEnumerable<Entity> Entities(string s) => Rx.Matches(s).Select(m => new Entity(m.Groups[1].Value, m.Groups[2].Value));

    /// <summary>Splits into (text, entityType?) segments, for run-level rendering.</summary>
    public static List<(string Text, string? Type)> Segments(string s)
    {
        var res = new List<(string, string?)>();
        var pos = 0;
        foreach (Match m in Rx.Matches(s))
        {
            if (m.Index > pos)
            {
                res.Add((s[pos..m.Index], null));
            }

            res.Add((m.Groups[2].Value, m.Groups[1].Value));
            pos = m.Index + m.Length;
        }
        if (pos < s.Length)
        {
            res.Add((s[pos..], null));
        }

        return res;
    }
}
