using AiDocumentRedactor.Core;
using AiDocumentRedactor.Documents;
using AiDocumentRedactor.Ocr;
using PDFtoImage;
using SkiaSharp;
using UglyToad.PdfPig;
using Xunit;

namespace AiDocumentRedactor.Tests;

/// <summary>Tests for reading and redacting scanned PDFs and images with OCR, using the synthetic scans in tests/TestCorpus.</summary>
public class OcrDocumentTests
{
    // One shared engine for all tests, because loading the models takes a moment.
    static readonly Lazy<RapidOcrEngine> Engine = new(() => new RapidOcrEngine());

    /// <summary>Finds a corpus file by walking up from the test folder.</summary>
    static string Corpus(string rel)
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "tests", "TestCorpus", rel))) dir = Path.GetDirectoryName(dir);
        return Path.Combine(dir ?? throw new FileNotFoundException(rel), "tests", "TestCorpus", rel);
    }

    /// <summary>Spans for every occurrence of each string in the text.</summary>
    static List<DetectedEntity> Spans(string text, params string[] needles) =>
        needles.SelectMany(n => Enumerable.Range(0, text.Length).Where(i => string.CompareOrdinal(text, i, n, 0, n.Length) == 0)
            .Select(i => new DetectedEntity("PERSON", i, n.Length, 1, "test"))).ToList();

    /// <summary>A scanned (image-only) PDF is read with OCR into text with word positions and a confidence.</summary>
    [Fact]
    public async Task A_scanned_pdf_is_read_with_ocr()
    {
        var doc = await new PdfDocumentReader(Engine.Value).ReadAsync(Corpus("scans/01-hr-letter-scan.pdf"), default);
        Assert.Contains("Eleanor Whitcombe", doc.Text);
        Assert.Contains("07700 900123", doc.Text);
        Assert.True(doc.OcrConfidence > 0.9, $"confidence {doc.OcrConfidence}");
        var (pw, ph) = doc.PageSizes![0];
        Assert.All(doc.Words!, w => { Assert.InRange(w.X, -2, pw + 2); Assert.InRange(w.Y, -2, ph + 2); });
    }

    /// <summary>Without an OCR engine a scan still reports that it has no text layer, as before.</summary>
    [Fact]
    public async Task Without_ocr_a_scan_is_reported_as_unreadable() =>
        await Assert.ThrowsAsync<NoTextLayerException>(() => new PdfDocumentReader().ReadAsync(Corpus("scans/01-hr-letter-scan.pdf"), default));

    /// <summary>A redacted scanned PDF has no text layer, solid black over the redacted words, and passes its own OCR re-check.</summary>
    [Fact]
    public async Task A_scanned_pdf_is_redacted_with_black_boxes_and_rechecked_by_ocr()
    {
        var doc = await new PdfDocumentReader(Engine.Value).ReadAsync(Corpus("scans/01-hr-letter-scan.pdf"), default);
        var result = Redactor.Apply(doc.Text, Spans(doc.Text, "Eleanor Whitcombe", "07700 900123", "QQ 12 34 56 C"), "[REDACTED:{type}]");
        using var ms = new MemoryStream();
        await new PdfDocumentWriter(new PdfOptions(), Engine.Value, new OcrOptions()).WriteAsync(doc, result, ms, default);   // throws if OCR can still read redacted words

        using (var check = PdfDocument.Open(ms.ToArray())) Assert.Equal(0, check.GetPages().Sum(p => p.GetWords().Count()));
        ms.Position = 0;
        using var bmp = Conversion.ToImage(ms, 0, options: new RenderOptions(Dpi: 100));
        var (pw, ph) = doc.PageSizes![0];
        var word = doc.Words!.First(w => doc.Text.Substring(w.Start, w.Length) == "Eleanor");
        int dark = 0, n = 0;
        for (var x = (int)(word.X / pw * bmp.Width); x < (int)((word.X + word.Width) / pw * bmp.Width); x++)
            for (var y = (int)((ph - word.Y - word.Height) / ph * bmp.Height); y < (int)((ph - word.Y) / ph * bmp.Height); y++)
            { var c = bmp.GetPixel(x, y); n++; if (c.Red < 40) dark++; }
        Assert.True((double)dark / n > 0.95);
    }

    /// <summary>Clean PNG and degraded JPG scans are read, redacted into an image of the same type and size, and the redacted
    /// words are no longer readable by OCR while the rest of the text still is.</summary>
    [Theory]
    [InlineData("scans/01-hr-letter-scan-clean.png", ".png")]
    [InlineData("scans/01-hr-letter-scan-degraded.jpg", ".jpg")]
    public async Task Images_are_read_and_redacted_to_the_same_type(string file, string ext)
    {
        var doc = await new ImageDocumentReader(Engine.Value).ReadAsync(Corpus(file), default);
        Assert.Equal("image", doc.Format);
        Assert.Contains("Eleanor", doc.Text);
        var result = Redactor.Apply(doc.Text, Spans(doc.Text, "Eleanor Whitcombe", "07700 900123", "QQ 12 34 56 C", "12 March 1986"), "[REDACTED:{type}]");

        var outPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ext);
        try
        {
            await new ImageDocumentWriter(new PdfOptions(), Engine.Value, new OcrOptions()).WriteAsync(doc, result, outPath, default);
            var bytes = await File.ReadAllBytesAsync(outPath);
            using var original = SKBitmap.Decode(Corpus(file)); using var redacted = SKBitmap.Decode(bytes);
            Assert.Equal((original.Width, original.Height), (redacted.Width, redacted.Height));
            Assert.Equal(ext == ".png", bytes.Take(4).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 }));   // really the same type
            Assert.DoesNotContain("Exif", System.Text.Encoding.Latin1.GetString(bytes));                          // no metadata block

            var after = string.Join(" ", (await Engine.Value.RecognizeAsync(bytes, default)).Lines.SelectMany(l => l.Words).Select(w => w.Text));
            Assert.DoesNotContain("Eleanor", after, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("900123", after);
            Assert.Contains("promotion", after, StringComparison.OrdinalIgnoreCase);          // unredacted text is still readable
        }
        finally { File.Delete(outPath); }
    }

    /// <summary>An image with no text raises the "no text" error instead of returning an empty document.</summary>
    [Fact]
    public async Task A_blank_image_has_no_text()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".png");
        using (var bmp = new SKBitmap(400, 300)) { bmp.Erase(SKColors.White); using var fs = File.Create(path); bmp.Encode(fs, SKEncodedImageFormat.Png, 100); }
        try { await Assert.ThrowsAsync<NoTextLayerException>(() => new ImageDocumentReader(Engine.Value).ReadAsync(path, default)); }
        finally { File.Delete(path); }
    }
}
