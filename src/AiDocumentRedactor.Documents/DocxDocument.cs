using System.IO.Compression;
using System.Text;
using AiDocumentRedactor.Core;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace AiDocumentRedactor.Documents;

/// <summary>One piece of text in a Word file (a single &lt;w:t&gt; element): its place in the flattened document text and its original length.</summary>
internal record TextSegment(Text Element, int Start, int Length);

/// <summary>How a Word file is turned into plain text and mapped back. The reader and writer both use this so their offsets always agree.</summary>
internal static class DocxModel
{
    /// <summary>A growable in-memory copy of the bytes (a MemoryStream over an array cannot grow when the package is saved).</summary>
    public static MemoryStream ExpandableCopy(byte[] bytes)
    {
        var ms = new MemoryStream();
        ms.Write(bytes);
        ms.Position = 0;
        return ms;
    }

    /// <summary>The roots of every part that carries text: body, headers, footers, footnotes, endnotes and comments.</summary>
    public static IEnumerable<OpenXmlElement> TextRoots(WordprocessingDocument doc)
    {
        var main = doc.MainDocumentPart!;
        if (main.Document is { } d)
        {
            yield return d;
        }

        foreach (var h in main.HeaderParts)
        {
            if (h.Header is { } hr)
            {
                yield return hr;
            }
        }

        foreach (var f in main.FooterParts)
        {
            if (f.Footer is { } fr)
            {
                yield return fr;
            }
        }

        if (main.FootnotesPart?.Footnotes is { } fn)
        {
            yield return fn;
        }

        if (main.EndnotesPart?.Endnotes is { } en)
        {
            yield return en;
        }

        if (main.WordprocessingCommentsPart?.Comments is { } c)
        {
            yield return c;
        }
    }

    /// <summary>Accepts all tracked changes in memory: deleted and moved-from text is removed and insertions become ordinary text,
    /// so text someone deleted cannot hide in the file or be read back.</summary>
    public static void AcceptChanges(WordprocessingDocument doc)
    {
        foreach (var root in TextRoots(doc))
        {
            foreach (var e in root.Descendants<DeletedRun>().ToList())
            {
                e.Remove();
            }

            foreach (var e in root.Descendants<MoveFromRun>().ToList())
            {
                e.Remove();
            }

            foreach (var e in root.Descendants<RunPropertiesChange>().ToList())
            {
                e.Remove();
            }

            foreach (var e in root.Descendants<ParagraphPropertiesChange>().ToList())
            {
                e.Remove();
            }

            foreach (var ins in root.Descendants<InsertedRun>().ToList())
            {
                Unwrap(ins);
            }

            foreach (var mv in root.Descendants<MoveToRun>().ToList())
            {
                Unwrap(mv);
            }
        }
    }

    /// <summary>Replaces a wrapper element by its children.</summary>
    static void Unwrap(OpenXmlElement wrapper)
    {
        foreach (var child in wrapper.ChildElements.ToList())
        {
            child.Remove();
            wrapper.InsertBeforeSelf(child);
        }
        wrapper.Remove();
    }

    /// <summary>Flattens the document: paragraphs in order, one per line, tabs and line breaks as characters. Returns the text and,
    /// in reading order, every text piece with the offset where it starts.</summary>
    public static (string Text, List<TextSegment> Segments) Flatten(WordprocessingDocument doc)
    {
        var sb = new StringBuilder();
        var segments = new List<TextSegment>();
        var first = true;
        foreach (var root in TextRoots(doc))
        {
            foreach (var p in root.Descendants<Paragraph>())
            {
                if (!first)
                {
                    sb.Append('\n');
                }

                first = false;
                // Only this paragraph's own text: not a text box's inner paragraphs (visited on their own) and not a drawing's unused fallback copy.
                foreach (var e in p.Descendants().Where(e => e is Text or TabChar or Break))
                {
                    if (e.Ancestors<Paragraph>().First() != p || e.Ancestors<AlternateContentFallback>().Any())
                    {
                        continue;
                    }

                    if (e is Text t)
                    {
                        segments.Add(new TextSegment(t, sb.Length, t.Text.Length));
                        sb.Append(t.Text);
                    }
                    else
                    {
                        sb.Append(e is TabChar ? '\t' : ' ');
                    }
                }
            }
        }

        return (sb.ToString(), segments);
    }
}

/// <summary>Reads a Word document into plain text, covering the body, tables, headers, footers, footnotes, comments and text boxes.
/// Tracked changes are accepted first, so deleted text is not read.</summary>
public class DocxDocumentReader : IDocumentReader
{
    /// <summary>True for .docx files.</summary>
    public bool CanRead(string path) => Path.GetExtension(path).Equals(".docx", StringComparison.OrdinalIgnoreCase);

    /// <summary>Reads the file into text on a background thread.</summary>
    public Task<ExtractedDocument> ReadAsync(string path, CancellationToken ct) => Task.Run(() =>
    {
        using var ms = DocxModel.ExpandableCopy(File.ReadAllBytes(path));
        WordprocessingDocument doc;
        try
        {
            doc = WordprocessingDocument.Open(ms, true);
        }
        catch (Exception ex) when (ex is OpenXmlPackageException or InvalidDataException or FileFormatException or IOException)
        {
            throw new InvalidOperationException("This Word file could not be opened. It may be password-protected or damaged.");
        }
        using (doc)
        {
            if (doc.MainDocumentPart is null)
            {
                throw new InvalidOperationException("This Word file has no document body.");
            }

            DocxModel.AcceptChanges(doc);
            var (text, _) = DocxModel.Flatten(doc);
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new NoTextLayerException("This Word document has no text.");
            }

            return new ExtractedDocument(path, "docx", text);
        }
    }, ct);
}

/// <summary>Writes a redacted Word document. Redacted text is replaced inside the original formatting (the placeholder takes the formatting
/// of the first text piece of each redacted span), tracked changes are accepted, and metadata and hidden content are removed. The
/// result is checked: if any redacted string can still be found anywhere in the file, it is refused.</summary>
public class DocxDocumentWriter : IDocumentWriter, IStreamDocumentWriter
{
    /// <summary>True for documents read by the Word reader.</summary>
    public bool CanWrite(ExtractedDocument source) => source.Format == "docx";

    /// <summary>Renders to memory, then writes the file.</summary>
    public async Task WriteAsync(ExtractedDocument source, RedactionResult result, string outputPath, CancellationToken ct)
    {
        using var ms = new MemoryStream();
        await WriteAsync(source, result, ms, ct);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
        await File.WriteAllBytesAsync(outputPath, ms.ToArray(), ct);
    }

    /// <summary>Redacts, scrubs and verifies the document, then writes it to the stream.</summary>
    public Task WriteAsync(ExtractedDocument source, RedactionResult result, Stream output, CancellationToken ct) =>
        Task.Run(() => output.Write(Render(source, result)), ct);

    /// <summary>The strings that must not survive anywhere in the output: the text of every active edit long enough to search for safely.</summary>
    static List<string> Secrets(RedactionResult result) =>
        result.Edits.Where(e => e.Status == EditStatus.Active && e.OriginalText is { Length: >= 4 }).Select(e => e.OriginalText!)
            .Distinct(StringComparer.OrdinalIgnoreCase).OrderByDescending(s => s.Length).ToList();

    /// <summary>The whole job: accept changes, check the text still matches what was read, replace the redacted spans, scrub, save, verify.</summary>
    static byte[] Render(ExtractedDocument source, RedactionResult result)
    {
        var secrets = Secrets(result);
        using var ms = DocxModel.ExpandableCopy(File.ReadAllBytes(source.SourcePath));
        using (var doc = WordprocessingDocument.Open(ms, true))
        {
            DocxModel.AcceptChanges(doc);
            var (text, segments) = DocxModel.Flatten(doc);
            if (text != source.Text)
            {
                throw new InvalidOperationException("The Word file changed since it was read; read it again before redacting.");
            }

            ApplyEdits(segments, result);
            Scrub(doc, secrets);
            doc.Save();
        }
        var bytes = ms.ToArray();
        Verify(bytes, secrets);
        return bytes;
    }

    /// <summary>Rebuilds the text of every piece an edit touches, in one pass each, from the original offsets. The placeholder goes into the first
    /// piece the edit touches (so it takes that piece's formatting); later pieces simply lose the covered text. Words split across several
    /// formatting runs are therefore redacted completely.</summary>
    static void ApplyEdits(List<TextSegment> segments, RedactionResult result)
    {
        var edits = result.Edits.Where(e => e.Status == EditStatus.Active).OrderBy(e => e.OriginalStart).ToList();
        foreach (var seg in segments)
        {
            int segStart = seg.Start, segEnd = seg.Start + seg.Length;
            var touching = edits.Where(e => e.OriginalStart < segEnd && e.OriginalStart + e.OriginalLength > segStart).ToList();
            if (touching.Count == 0)
            {
                continue;
            }

            var original = seg.Element.Text;
            var sb = new StringBuilder();
            var pos = 0;
            foreach (var e in touching)
            {
                var a = Math.Max(e.OriginalStart, segStart) - segStart;
                var b = Math.Min(e.OriginalStart + e.OriginalLength, segEnd) - segStart;
                sb.Append(original, pos, a - pos);
                // Is this the first piece with any of the edit's text?
                var firstPiece = segments.First(g => e.OriginalStart < g.Start + g.Length && e.OriginalStart + e.OriginalLength > g.Start);
                if (firstPiece == seg)
                {
                    sb.Append(e.Replacement);
                }

                pos = b;
            }
            sb.Append(original, pos, original.Length - pos);
            seg.Element.Text = sb.ToString();
            seg.Element.Space = SpaceProcessingModeValues.Preserve;
        }
    }

    /// <summary>Removes what could carry the redacted data outside the visible text: document properties, thumbnail, custom XML, comment authors,
    /// image descriptions, hyperlink addresses, field codes and any attribute or text that still contains a redacted string.</summary>
    static void Scrub(WordprocessingDocument doc, List<string> secrets)
    {
        var props = doc.PackageProperties;
        props.Creator = null;
        props.LastModifiedBy = null;
        props.Title = null;
        props.Subject = null;
        props.Keywords = null;
        props.Description = null;
        props.Category = null;
        props.ContentStatus = null;
        props.Created = null;
        props.Modified = null;
        props.Revision = null;
        props.LastPrinted = null;
        props.Language = null;
        props.Version = null;
        props.ContentType = null;
        if (doc.ExtendedFilePropertiesPart is { } ext)
        {
            doc.DeletePart(ext);
        }

        if (doc.CustomFilePropertiesPart is { } cust)
        {
            doc.DeletePart(cust);
        }

        foreach (var t in doc.GetPartsOfType<ThumbnailPart>().ToList())
        {
            doc.DeletePart(t);
        }

        var main = doc.MainDocumentPart!;
        foreach (var x in main.CustomXmlParts.ToList())
        {
            main.DeletePart(x);
        }

        if (main.DocumentSettingsPart?.Settings is { } settings)
        {
            foreach (var a in settings.Descendants<AttachedTemplate>().ToList())
            {
                a.Remove();
            }
        }

        // Comment authors and times identify people.
        if (main.WordprocessingCommentsPart?.Comments is { } comments)
        {
            foreach (var c in comments.Descendants<Comment>())
            {
                c.Author = "Reviewer";
                c.Initials = "R";
                c.Date = null;
            }
        }

        // Image descriptions ("alt text") often describe who or what is shown.
        foreach (var root in DocxModel.TextRoots(doc))
        {
            foreach (var dp in root.Descendants<DocumentFormat.OpenXml.Drawing.Wordprocessing.DocProperties>())
            {
                dp.Description = "";
                dp.Title = "";
            }
        }

        // Links to a redacted address (for example mailto: links) must not keep it.
        foreach (var part in new OpenXmlPart[] { main }.Concat(main.HeaderParts).Concat(main.FooterParts))
        {
            foreach (var hl in part.HyperlinkRelationships.ToList())
            {
                if (secrets.Any(s => hl.Uri.OriginalString.Contains(s, StringComparison.OrdinalIgnoreCase)))
                {
                    var id = hl.Id;
                    part.DeleteReferenceRelationship(id);
                    part.AddHyperlinkRelationship(new Uri("https://redacted.invalid/"), true, id);
                }
            }
        }

        // Anything left in text, field codes or attributes (bookmark names, content-control tags, ...) that still contains a redacted string.
        foreach (var root in DocxModel.TextRoots(doc))
        {
            foreach (var el in root.Descendants().ToList())
            {
                if (el is OpenXmlLeafTextElement leaf)
                {
                    var t = Replace(leaf.Text, secrets);
                    if (t != leaf.Text)
                    {
                        leaf.Text = t;
                    }
                }
                foreach (var attr in el.GetAttributes().ToList())
                {
                    var v = Replace(attr.Value ?? "", secrets);
                    if (v != attr.Value)
                    {
                        el.SetAttribute(new OpenXmlAttribute(attr.Prefix, attr.LocalName, attr.NamespaceUri, v));
                    }
                }
            }
        }
    }

    /// <summary>Replaces every occurrence (ignoring case) of each secret with a generic marker.</summary>
    static string Replace(string s, List<string> secrets)
    {
        foreach (var secret in secrets)
        {
            s = s.Replace(secret, "[REDACTED]", StringComparison.OrdinalIgnoreCase);
        }

        return s;
    }

    /// <summary>Last line of defence: opens every part of the finished file and refuses it if any redacted string is still there. Reports a count only.</summary>
    static void Verify(byte[] bytes, List<string> secrets)
    {
        using var zip = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);
        var hits = 0;
        foreach (var entry in zip.Entries.Where(e => e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) || e.FullName.EndsWith(".rels", StringComparison.OrdinalIgnoreCase)))
        {
            using var reader = new StreamReader(entry.Open());
            var content = reader.ReadToEnd();
            hits += secrets.Count(s => content.Contains(s, StringComparison.OrdinalIgnoreCase));
        }
        if (hits > 0)
        {
            throw new InvalidOperationException($"{hits} redacted string(s) are still present in the Word file; refusing to write it.");
        }
    }
}
