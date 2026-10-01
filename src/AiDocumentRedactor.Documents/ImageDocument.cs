using AiDocumentRedactor.Core;
using SkiaSharp;

namespace AiDocumentRedactor.Documents;

/// <summary>Reads a scanned page saved as a PNG or JPG using OCR, keeping where each word sits so it can be blacked out.</summary>
public class ImageDocumentReader(IOcrEngine ocr, OcrOptions? ocrOptions = null) : IDocumentReader
{
    readonly OcrOptions oo = ocrOptions ?? new();
    // OCR is slow, so each image is read once per session. Held in memory only.
    static readonly System.Collections.Concurrent.ConcurrentDictionary<string, (long Size, DateTime Written, ExtractedDocument Doc)> cache = new();

    /// <summary>True for .png, .jpg and .jpeg files.</summary>
    public bool CanRead(string path) => DocumentFiles.ContentType(path) is { } t && t.StartsWith("image/");

    /// <summary>Reads the image with OCR (or reuses the earlier result if the file is unchanged). The page size is the image size
    /// at the configured DPI; throws <see cref="NoTextLayerException"/> if no text is found.</summary>
    public async Task<ExtractedDocument> ReadAsync(string path, CancellationToken ct)
    {
        var info = new FileInfo(path);
        if (cache.TryGetValue(path, out var hit) && hit.Size == info.Length && hit.Written == info.LastWriteTimeUtc) return hit.Doc;
        var bytes = await File.ReadAllBytesAsync(path, ct);
        var read = await ocr.RecognizeAsync(bytes, ct);
        double pw = read.WidthPx * 72.0 / oo.ImageDpi, ph = read.HeightPx * 72.0 / oo.ImageDpi;
        var lines = OcrMapping.ToPageLines(read, pw, ph);
        if (lines.Count == 0) throw new NoTextLayerException("No text was found in this image.");
        var sb = new System.Text.StringBuilder(); var words = new List<WordBox>();
        OcrMapping.AppendPage(sb, words, 0, lines);
        var doc = new ExtractedDocument(path, "image", sb.ToString(), words, [(pw, ph)], read.Lines.SelectMany(l => l.Words).Average(w => w.Confidence));
        cache[path] = (info.Length, info.LastWriteTimeUtc, doc);
        return doc;
    }
}

/// <summary>Writes a redacted scan as a new image of the same type (PNG stays PNG, JPG stays JPG). The redacted words are painted
/// solid black into the pixels, and re-encoding drops all metadata (EXIF, GPS, camera, dates). The output is OCR'd again and
/// refused if redacted words can still be read.</summary>
public class ImageDocumentWriter(PdfOptions options, IOcrEngine? ocr = null, OcrOptions? ocrOptions = null) : IDocumentWriter, IStreamDocumentWriter
{
    /// <summary>True for documents read by the image reader.</summary>
    public bool CanWrite(ExtractedDocument source) => source.Format == "image";

    /// <summary>Renders to memory, then writes the file.</summary>
    public async Task WriteAsync(ExtractedDocument source, RedactionResult result, string outputPath, CancellationToken ct)
    {
        using var ms = new MemoryStream();
        await WriteAsync(source, result, ms, ct);
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
        await File.WriteAllBytesAsync(outputPath, ms.ToArray(), ct);
    }

    /// <summary>Paints the black boxes into the image, encodes it, checks it with OCR, and writes it to the stream.</summary>
    public async Task WriteAsync(ExtractedDocument source, RedactionResult result, Stream output, CancellationToken ct)
    {
        var (pw, ph) = source.PageSizes![0];
        var bytes = await Task.Run(() =>
        {
            using var bmp = SKBitmap.Decode(File.ReadAllBytes(source.SourcePath)) ?? throw new InvalidOperationException("The image could not be decoded.");
            double sx = bmp.Width / pw, sy = bmp.Height / ph;
            using (var canvas = new SKCanvas(bmp))
            using (var black = new SKPaint { Color = SKColors.Black, Style = SKPaintStyle.Fill, IsAntialias = false })
                foreach (var w in PdfDocumentWriter.WordsToCover(source, result))
                {
                    PdfDocumentWriter.PaintWord(canvas, black, w, ph, sx, sy, options, fromOcr: true);   // tilted outline for OCR words
                }
            var png = Path.GetExtension(source.SourcePath).Equals(".png", StringComparison.OrdinalIgnoreCase);
            using var img = SKImage.FromBitmap(bmp);
            return img.Encode(png ? SKEncodedImageFormat.Png : SKEncodedImageFormat.Jpeg, png ? 100 : options.JpegQuality).ToArray();
        }, ct);

        if (ocr is not null && (ocrOptions ?? new()).VerifyOutput)
        {
            var words = (await ocr.RecognizeAsync(bytes, ct)).Lines.SelectMany(l => l.Words).Select(w => w.Text);
            var leaks = OcrMapping.CountLeaks(source, result, words);
            if (leaks > 0) throw new InvalidOperationException($"{leaks} redacted word(s) can still be read in the output (checked by OCR); refusing to write it.");
        }
        await output.WriteAsync(bytes, ct);
    }
}
