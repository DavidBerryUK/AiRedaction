using System.Text;
using AiDocumentRedactor.Core;
using PDFtoImage;
using SkiaSharp;
using UglyToad.PdfPig;

namespace AiDocumentRedactor.Documents;

/// <summary>Reads a PDF into text with each word's position on its page. Pages with a text layer are read directly;
/// scanned pages (no text layer) are rendered and read by OCR when an OCR engine is available.</summary>
public class PdfDocumentReader(IOcrEngine? ocr = null, OcrOptions? ocrOptions = null) : IDocumentReader
{
    readonly OcrOptions oo = ocrOptions ?? new();
    // OCR is slow, so each scanned file is read once per session. Held in memory only (never written to disk).
    static readonly System.Collections.Concurrent.ConcurrentDictionary<string, (long Size, DateTime Written, ExtractedDocument Doc)> cache = new();

    /// <summary>True for .pdf files.</summary>
    public bool CanRead(string path) => Path.GetExtension(path).Equals(".pdf", StringComparison.OrdinalIgnoreCase);

    /// <summary>Reads the file, reusing an earlier OCR result if the file has not changed.</summary>
    public async Task<ExtractedDocument> ReadAsync(string path, CancellationToken ct)
    {
        var info = new FileInfo(path);
        if (cache.TryGetValue(path, out var hit) && hit.Size == info.Length && hit.Written == info.LastWriteTimeUtc)
        {
            return hit.Doc;
        }

        var doc = await ReadCoreAsync(path, ct);
        if (doc.OcrConfidence is not null)
        {
            cache[path] = (info.Length, info.LastWriteTimeUtc, doc);
        }

        return doc;
    }

    /// <summary>Builds the flat text page by page: words joined by spaces, lines by line breaks, pages by a blank line.
    /// A page with no text layer is OCR'd if possible. Throws <see cref="NoTextLayerException"/> if nothing could be read,
    /// and a clear error for encrypted or rotated PDFs.</summary>
    async Task<ExtractedDocument> ReadCoreAsync(string path, CancellationToken ct)
    {
        var bytes = await File.ReadAllBytesAsync(path, ct);
        PdfDocument pdf;
        try
        {
            pdf = PdfDocument.Open(bytes);
        }
        catch (Exception ex) when (ex.GetType().Name.Contains("Encrypted", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("password", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("This PDF is password-protected.");
        }
        using (pdf)
        {
            var sb = new StringBuilder();
            var words = new List<WordBox>();
            var sizes = new List<(double, double)>();
            var confidences = new List<double>();
            var blankPages = 0;
            foreach (var page in pdf.GetPages())
            {
                ct.ThrowIfCancellationRequested();
                if (page.Rotation.Value != 0)
                {
                    throw new NotSupportedException("Rotated PDF pages are not supported yet.");
                }

                sizes.Add((page.Width, page.Height));
                var lines = Lines(page.GetWords().Where(w => !string.IsNullOrWhiteSpace(w.Text))
                    .Select(w => new PageWord(w.Text, w.BoundingBox.Left, w.BoundingBox.Bottom, w.BoundingBox.Width, w.BoundingBox.Height)));
                if (lines.Count == 0)
                {
                    if (ocr is null || !oo.Enabled)
                    {
                        blankPages++;
                    }
                    else
                    {
                        // A scanned page: render it and read it with OCR.
                        using var src = new MemoryStream(bytes);
                        using var bmp = Conversion.ToImage(src, page.Number - 1, options: new RenderOptions(Dpi: oo.RenderDpi));
                        using var png = SKImage.FromBitmap(bmp).Encode(SKEncodedImageFormat.Png, 100);
                        var read = await ocr.RecognizeAsync(png.AsSpan().ToArray(), ct);
                        lines = OcrMapping.ToPageLines(read, page.Width, page.Height);
                        confidences.AddRange(read.Lines.SelectMany(l => l.Words).Select(w => w.Confidence));
                        if (lines.Count == 0)
                        {
                            blankPages++;
                        }
                    }
                }
                OcrMapping.AppendPage(sb, words, page.Number - 1, lines);
            }
            if (words.Count == 0)
            {
                throw new NoTextLayerException(ocr is null || !oo.Enabled
                    ? "This PDF has no text layer (it looks like a scan), and OCR is switched off."
                    : "No text could be read from this PDF, even with OCR.");
            }

            return new ExtractedDocument(path, "pdf", sb.ToString(), words, sizes, confidences.Count > 0 ? confidences.Average() : null);
        }
    }

    /// <summary>Groups words into lines (top to bottom, left to right within a line).</summary>
    static List<List<PageWord>> Lines(IEnumerable<PageWord> words)
    {
        var lines = new List<(double Mid, double H, List<PageWord> Words)>();
        foreach (var w in words.OrderByDescending(w => w.Y + w.H / 2))
        {
            var mid = w.Y + w.H / 2;
            var i = lines.FindIndex(l => Math.Abs(l.Mid - mid) <= 0.5 * Math.Max(l.H, w.H));
            if (i < 0)
            {
                lines.Add((mid, w.H, [w]));
            }
            else
            {
                lines[i].Words.Add(w);
            }
        }
        return lines.OrderByDescending(l => l.Mid).Select(l => l.Words.OrderBy(w => w.X).ToList()).ToList();
    }
}

/// <summary>Writes a redacted PDF as a brand-new image-only PDF: each page is rendered to a picture, the redacted words are
/// covered with solid black boxes, and the pictures are assembled into a new file. The original text is physically gone
/// (nothing is merely hidden under a box), and the output carries no metadata from the source.</summary>
public class PdfDocumentWriter(PdfOptions options, IOcrEngine? ocr = null, OcrOptions? ocrOptions = null) : IDocumentWriter, IStreamDocumentWriter
{
    /// <summary>True for documents read by the PDF reader.</summary>
    public bool CanWrite(ExtractedDocument source) => source.Format == "pdf";

    /// <summary>Renders to memory, checks the result has no text layer, then writes the file.</summary>
    public async Task WriteAsync(ExtractedDocument source, RedactionResult result, string outputPath, CancellationToken ct)
    {
        using var ms = new MemoryStream();
        await WriteAsync(source, result, ms, ct);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
        await File.WriteAllBytesAsync(outputPath, ms.ToArray(), ct);
    }

    /// <summary>Renders the redacted PDF into the stream (used for the in-app viewer as well as saving).</summary>
    public async Task WriteAsync(ExtractedDocument source, RedactionResult result, Stream output, CancellationToken ct)
    {
        var original = await File.ReadAllBytesAsync(source.SourcePath, ct);
        var bytes = await Task.Run(() => Render(original, source, result, ct), ct);
        // For documents read by OCR, also read the OUTPUT with OCR: if redacted words can still be read, refuse to return it.
        if (ocr is not null && (ocrOptions ?? new()).VerifyOutput && source.OcrConfidence is not null)
        {
            var outputWords = new List<string>();
            for (var p = 0; p < source.PageSizes!.Count; p++)
            {
                using var src = new MemoryStream(bytes);
                using var bmp = Conversion.ToImage(src, p, options: new RenderOptions(Dpi: (ocrOptions ?? new()).RenderDpi));
                using var png = SKImage.FromBitmap(bmp).Encode(SKEncodedImageFormat.Png, 100);
                outputWords.AddRange((await ocr.RecognizeAsync(png.AsSpan().ToArray(), ct)).Lines.SelectMany(l => l.Words).Select(w => w.Text));
            }
            var leaks = OcrMapping.CountLeaks(source, result, outputWords);
            if (leaks > 0)
            {
                throw new InvalidOperationException($"{leaks} redacted word(s) can still be read in the output (checked by OCR); refusing to write it.");
            }
        }
        await output.WriteAsync(bytes, ct);
    }

    /// <summary>The words (by position in the text) that any active edit touches. Whole words are covered so no partial letters show.</summary>
    public static IEnumerable<WordBox> WordsToCover(ExtractedDocument doc, RedactionResult result) =>
        (doc.Words ?? []).Where(w => result.Edits.Any(e => e.Status == EditStatus.Active
            && w.Start < e.OriginalStart + e.OriginalLength && w.Start + w.Length > e.OriginalStart));

    /// <summary>The rectangle to black out for a word, in points measured from the page's top-left. It is the word's reported box
    /// plus padding on every side, and (for text-layer PDFs, whose boxes stop short of descenders) extra room below for descenders.
    /// OCR boxes already include the whole line height, so they get no extra room; adding it would clip neighbouring words.</summary>
    public static (double Left, double Top, double Right, double Bottom) BoxInPoints(WordBox w, double pageHeight, PdfOptions o, bool boxIncludesDescenders = false) =>
        (w.X - o.BoxPaddingPoints, pageHeight - (w.Y + w.Height) - o.BoxPaddingPoints,
         w.X + w.Width + o.BoxPaddingPoints, pageHeight - w.Y + o.BoxPaddingPoints + (boxIncludesDescenders ? 0 : o.DescenderFactor * w.Height));

    /// <summary>The word's true outline, padded outward by <c>pad</c> points along its own axes, as 4 (x, y) points measured from the page's top-left;
    /// null if the word has no outline (text-layer PDFs). Used for OCR words so a box on a tilted scan is tilted too and does not clip the next line.</summary>
    public static (double X, double Y)[]? QuadInPoints(WordBox w, double pageHeight, double pad)
    {
        if (w.Quad is not { Length: 8 } q)
        {
            return null;
        }

        var p = Enumerable.Range(0, 4).Select(i => (X: q[i * 2], Y: pageHeight - q[i * 2 + 1])).ToArray();   // top-left origin, clockwise from top-left
        (double X, double Y) Unit((double X, double Y) a, (double X, double Y) b)
        {
            var dx = b.X - a.X;
            var dy = b.Y - a.Y;
            var len = Math.Sqrt(dx * dx + dy * dy);
            return len < 1e-9 ? (0, 0) : (dx / len, dy / len);
        }
        var u = Unit(p[0], p[1]);
        var v = Unit(p[0], p[3]);       // along the word, and down the word
        (double X, double Y) Move((double X, double Y) pt, double su, double sv) => (pt.X + u.X * su * pad + v.X * sv * pad, pt.Y + u.Y * su * pad + v.Y * sv * pad);
        return [Move(p[0], -1, -1), Move(p[1], 1, -1), Move(p[2], 1, 1), Move(p[3], -1, 1)];
    }

    /// <summary>Paints one word black on a bitmap: its tilted outline if it has one, otherwise an upright rectangle. Scales are bitmap pixels per point.</summary>
    public static void PaintWord(SKCanvas canvas, SKPaint black, WordBox w, double pageHeight, double sx, double sy, PdfOptions options, bool fromOcr)
    {
        if (QuadInPoints(w, pageHeight, options.BoxPaddingPoints) is { } quad)
        {
            using var path = new SKPath();
            path.MoveTo((float)(quad[0].X * sx), (float)(quad[0].Y * sy));
            for (var i = 1; i < 4; i++)
            {
                path.LineTo((float)(quad[i].X * sx), (float)(quad[i].Y * sy));
            }

            path.Close();
            canvas.DrawPath(path, black);
            return;
        }
        var (l, t, r, b) = BoxInPoints(w, pageHeight, options, fromOcr);
        canvas.DrawRect(SKRect.Create((float)(l * sx), (float)(t * sy), (float)((r - l) * sx), (float)((b - t) * sy)), black);
    }

    /// <summary>Paints a hand-drawn rectangle solid black on a bitmap. Scales are bitmap pixels per point.</summary>
    public static void PaintArea(SKCanvas canvas, SKPaint black, AreaBox a, double pageHeight, double sx, double sy) =>
        canvas.DrawRect(SKRect.Create((float)(a.X * sx), (float)((pageHeight - a.Y - a.Height) * sy), (float)(a.Width * sx), (float)(a.Height * sy)), black);

    /// <summary>Draws black boxes on rendered pages, builds the new PDF, and refuses to return it if any text survived.
    /// Returns the finished PDF bytes.</summary>
    byte[] Render(byte[] pdfBytes, ExtractedDocument doc, RedactionResult result, CancellationToken ct)
    {
        if (doc.PageSizes is not { Count: > 0 } sizes)
        {
            throw new InvalidOperationException("The document has no page layout to redact.");
        }

        var cover = WordsToCover(doc, result).GroupBy(w => w.Page).ToDictionary(g => g.Key, g => g.ToList());
        using var built = new MemoryStream();
        using (var pdf = SKDocument.CreatePdf(built))
        {
            for (var p = 0; p < sizes.Count; p++)
            {
                ct.ThrowIfCancellationRequested();
                var (pw, ph) = sizes[p];
                using var src = new MemoryStream(pdfBytes);
                using var bmp = Conversion.ToImage(src, p, options: new RenderOptions(Dpi: options.RenderDpi));
                var sx = bmp.Width / pw;
                var sy = bmp.Height / ph;
                using (var canvas = new SKCanvas(bmp))
                using (var black = new SKPaint { Color = SKColors.Black, Style = SKPaintStyle.Fill, IsAntialias = false })
                {
                    foreach (var w in cover.GetValueOrDefault(p) ?? [])
                    {
                        PaintWord(canvas, black, w, ph, sx, sy, options, doc.OcrConfidence is not null);
                    }

                    foreach (var a in result.AreaList.Where(a => a.Page == p))
                    {
                        PaintArea(canvas, black, a, ph, sx, sy);
                    }
                }
                using var img = SKImage.FromBitmap(bmp);
                using var jpg = img.Encode(SKEncodedImageFormat.Jpeg, options.JpegQuality);
                using var page = SKImage.FromEncodedData(jpg);
                using var c = pdf.BeginPage((float)pw, (float)ph);
                c.DrawImage(page, SKRect.Create(0, 0, (float)pw, (float)ph));
                pdf.EndPage();
            }
            pdf.Close();
        }
        var bytes = built.ToArray();
        VerifyNoTextLayer(bytes);
        return bytes;
    }

    /// <summary>Safety check: the output must contain no extractable text at all, otherwise it is not written.</summary>
    static void VerifyNoTextLayer(byte[] pdfBytes)
    {
        using var check = PdfDocument.Open(pdfBytes);
        var leftover = check.GetPages().Sum(pg => pg.GetWords().Count());
        if (leftover > 0)
        {
            throw new InvalidOperationException($"Redacted PDF still contains {leftover} words of text; refusing to write it.");
        }
    }
}
