using AiDocumentRedactor.Core;
using AiDocumentRedactor.Documents;
using PDFtoImage;
using SkiaSharp;
using UglyToad.PdfPig;
using Xunit;

namespace AiDocumentRedactor.Tests;

/// <summary>Tests for reading and redacting PDFs, using the synthetic PDFs in tests/TestCorpus.</summary>
public class PdfTests
{
    /// <summary>Finds a corpus file by walking up from the test folder to the repository root.</summary>
    static string Corpus(string rel)
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "tests", "TestCorpus", rel))) dir = Path.GetDirectoryName(dir);
        return Path.Combine(dir ?? throw new FileNotFoundException(rel), "tests", "TestCorpus", rel);
    }

    /// <summary>Spans for every occurrence of each string in the document text.</summary>
    static List<DetectedEntity> Spans(string text, params string[] needles) =>
        needles.SelectMany(n => Enumerable.Range(0, text.Length).Where(i => string.CompareOrdinal(text, i, n, 0, n.Length) == 0)
            .Select(i => new DetectedEntity("PERSON", i, n.Length, 1, "test"))).ToList();

    /// <summary>The text layer is read with the words' positions and the page size.</summary>
    [Fact]
    public async Task Reads_text_and_word_positions()
    {
        var doc = await new PdfDocumentReader().ReadAsync(Corpus("pdf/01-hr-letter.pdf"), default);
        Assert.Equal("pdf", doc.Format);
        Assert.Contains("Eleanor Whitcombe", doc.Text);
        Assert.Single(doc.PageSizes!);
        Assert.All(doc.Words!, w => Assert.Equal(doc.Text.Substring(w.Start, w.Length).Trim(), doc.Text.Substring(w.Start, w.Length)));
        Assert.Contains(doc.Words!, w => doc.Text.Substring(w.Start, w.Length) == "Eleanor");
    }

    /// <summary>A scanned (image-only) PDF is reported as needing OCR instead of being read as empty text.</summary>
    [Fact]
    public async Task A_scanned_pdf_is_reported_as_having_no_text_layer() =>
        await Assert.ThrowsAsync<NoTextLayerException>(() => new PdfDocumentReader().ReadAsync(Corpus("scans/01-hr-letter-scan.pdf"), default));

    /// <summary>The output has no text at all, the redacted words are solid black, and untouched text is still visible.</summary>
    [Fact]
    public async Task Redacted_pdf_has_no_text_layer_and_black_boxes_over_the_redacted_words()
    {
        var doc = await new PdfDocumentReader().ReadAsync(Corpus("pdf/01-hr-letter.pdf"), default);
        var result = Redactor.Apply(doc.Text, Spans(doc.Text, "Eleanor Whitcombe", "07700 900123"), "[REDACTED:{type}]");
        var outPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".pdf");
        try
        {
            await new PdfDocumentWriter(new PdfOptions()).WriteAsync(doc, result, outPath, default);

            using (var check = PdfDocument.Open(outPath))
                Assert.Equal(0, check.GetPages().Sum(p => p.GetWords().Count()));   // nothing extractable

            using var bmp = Conversion.ToImage(File.OpenRead(outPath), 0, options: new RenderOptions(Dpi: 100));
            var (pw, ph) = doc.PageSizes![0];
            double Darkness(WordBox w)   // share of near-black pixels inside a word's box
            {
                int dark = 0, n = 0;
                for (var x = (int)(w.X / pw * bmp.Width); x < (int)((w.X + w.Width) / pw * bmp.Width); x++)
                    for (var y = (int)((ph - w.Y - w.Height) / ph * bmp.Height); y < (int)((ph - w.Y) / ph * bmp.Height); y++)
                    { var c = bmp.GetPixel(x, y); n++; if (c.Red < 40 && c.Green < 40 && c.Blue < 40) dark++; }
                return (double)dark / n;
            }
            WordBox Word(string t) => doc.Words!.First(w => doc.Text.Substring(w.Start, w.Length) == t);

            Assert.True(Darkness(Word("Eleanor")) > 0.95, "redacted word should be solid black");
            Assert.True(Darkness(Word("Whitcombe")) > 0.95);
            Assert.True(Darkness(Word("Confirmation")) < 0.5, "untouched text must not be blacked out");
        }
        finally { File.Delete(outPath); }
    }

    /// <summary>Every dark pixel of every word's glyphs (including descenders like g, y, p) falls inside the black box drawn for it.</summary>
    [Fact]
    public async Task Black_boxes_cover_the_whole_glyph_including_descenders()
    {
        var path = Corpus("pdf/01-hr-letter.pdf");
        var doc = await new PdfDocumentReader().ReadAsync(path, default);
        var options = new PdfOptions();
        using var bmp = Conversion.ToImage(File.OpenRead(path), 0, options: new RenderOptions(Dpi: 288));   // 4 pixels per point
        var (pw, ph) = doc.PageSizes![0]; var sx = bmp.Width / pw; var sy = bmp.Height / ph;
        var tested = 0;
        foreach (var w in doc.Words!)
        {
            var (l, t, r, b) = PdfDocumentWriter.BoxInPoints(w, ph, options);
            // measure the ink of this word within its own columns, looking up to 2.5pt above and 4pt below the reported box
            double inkTop = double.MaxValue, inkBottom = double.MinValue;
            for (var yy = (int)((ph - w.Y - w.Height - 2.5) * sy); yy < (int)((ph - w.Y + 4) * sy); yy++)
                for (var xx = (int)(w.X * sx); xx < (int)((w.X + w.Width) * sx); xx++)
                    if (bmp.GetPixel(xx, yy).Red < 128) { inkTop = Math.Min(inkTop, yy / sy); inkBottom = Math.Max(inkBottom, yy / sy); }
            if (inkTop == double.MaxValue) continue;
            tested++;
            Assert.True(inkTop >= t - 0.01, $"'{doc.Text.Substring(w.Start, w.Length)}' pokes {t - inkTop:0.0}pt above its box");
            Assert.True(inkBottom <= b + 0.01, $"'{doc.Text.Substring(w.Start, w.Length)}' pokes {inkBottom - b:0.0}pt below its box");
        }
        Assert.True(tested > 50, "should have measured many words");
    }

    /// <summary>Attack test: take the saved PDF apart the way someone trying to recover the text would, and find nothing.
    /// Searches every byte (raw and decompressed) for the original strings, lists every object type in the file,
    /// and extracts the page image to confirm the redacted areas are solid black in the image itself.</summary>
    [Fact]
    public async Task Attack_extracting_or_editing_the_saved_pdf_reveals_nothing()
    {
        var doc = await new PdfDocumentReader().ReadAsync(Corpus("pdf/01-hr-letter.pdf"), default);
        var secrets = new[] { "Eleanor", "Whitcombe", "07700 900123", "QQ 12 34 56 C", "marcus.delaney" };
        var result = Redactor.Apply(doc.Text, Spans(doc.Text, "Eleanor Whitcombe", "07700 900123", "QQ 12 34 56 C", "marcus.delaney@brightwater-analytics.example"), "[REDACTED:{type}]");
        using var ms = new MemoryStream();
        await new PdfDocumentWriter(new PdfOptions()).WriteAsync(doc, result, ms, default);
        var bytes = ms.ToArray();

        // 1. No original string anywhere in the file: raw bytes, or inside any decompressed stream.
        var haystacks = new List<byte[]> { bytes };
        foreach (var m in System.Text.RegularExpressions.Regex.Matches(System.Text.Encoding.Latin1.GetString(bytes), @"stream\r?\n(.*?)\r?\nendstream", System.Text.RegularExpressions.RegexOptions.Singleline))
        {
            var raw = System.Text.Encoding.Latin1.GetBytes(((System.Text.RegularExpressions.Match)m).Groups[1].Value);
            try { using var z = new System.IO.Compression.ZLibStream(new MemoryStream(raw), System.IO.Compression.CompressionMode.Decompress); var o = new MemoryStream(); z.CopyTo(o); haystacks.Add(o.ToArray()); } catch { }
        }
        foreach (var secret in secrets)
            foreach (var h in haystacks)
                Assert.False(System.Text.Encoding.UTF8.GetString(h).Contains(secret, StringComparison.OrdinalIgnoreCase), $"'{secret}' found in the file");

        // 2. Structure: one page image per page, no text operators, no fonts, no annotations, no embedded files, no outline.
        var text = System.Text.Encoding.Latin1.GetString(bytes);
        foreach (var banned in new[] { "/Font", "/ToUnicode", "/Annots", "/EmbeddedFile", "/Outlines", "/AcroForm", "/OCProperties" })
            Assert.DoesNotContain(banned, text);
        using var pdf = PdfDocument.Open(bytes);
        Assert.Equal(1, pdf.NumberOfPages);
        var page = pdf.GetPage(1);
        var images = page.GetImages().ToList();
        Assert.Single(images);                                   // the page is ONE picture: no separate box object to delete
        Assert.Empty(page.Letters);

        // 3. Extract that picture, as a tool would, and look at the pixels under the redactions.
        using var img = SKBitmap.Decode(images[0].RawBytes.ToArray());
        Assert.NotNull(img);
        var (pw, ph) = doc.PageSizes![0]; var sx = img.Width / pw; var sy = img.Height / ph;
        foreach (var w in WordsInSpans(doc, result))
        {
            var (l, t, r, b) = PdfDocumentWriter.BoxInPoints(w, ph, new PdfOptions());
            double max = 0;
            for (var x = (int)(l * sx) + 1; x < (int)(r * sx) - 1; x++)
                for (var y = (int)(t * sy) + 1; y < (int)(b * sy) - 1; y++)
                { var c = img.GetPixel(x, y); max = Math.Max(max, Math.Max(c.Red, Math.Max(c.Green, c.Blue))); }
            Assert.True(max < 40, $"'{doc.Text.Substring(w.Start, w.Length)}': the extracted image still has light pixels (max {max}) under the box");
        }
    }

    /// <summary>The words that overlap any edit's span.</summary>
    static IEnumerable<WordBox> WordsInSpans(ExtractedDocument doc, RedactionResult result) => PdfDocumentWriter.WordsToCover(doc, result);

    /// <summary>A rectangle drawn by hand on a page is painted solid black into the output, even where there is no word to redact.</summary>
    [Fact]
    public async Task Hand_drawn_area_is_painted_black()
    {
        var doc = await new PdfDocumentReader().ReadAsync(Corpus("pdf/01-hr-letter.pdf"), default);
        var (pw, ph) = doc.PageSizes![0];
        var area = new AreaBox(0, 100, 100, 150, 60);   // points, from the bottom-left
        var result = Redactor.Apply(doc.Text, [], "[REDACTED:{type}]") with { Areas = [area] };
        var outPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".pdf");
        try
        {
            await new PdfDocumentWriter(new PdfOptions()).WriteAsync(doc, result, outPath, default);
            using var bmp = Conversion.ToImage(File.OpenRead(outPath), 0, options: new RenderOptions(Dpi: 100));
            double sx = bmp.Width / pw, sy = bmp.Height / ph; int dark = 0, total = 0;
            for (var x = (int)((area.X + 3) * sx); x < (int)((area.X + area.Width - 3) * sx); x++)
                for (var y = (int)((ph - area.Y - area.Height + 3) * sy); y < (int)((ph - area.Y - 3) * sy); y++) { total++; if (bmp.GetPixel(x, y).Red < 40) dark++; }
            Assert.True(dark > total * 0.98, $"only {dark} of {total} pixels are black");
        }
        finally { File.Delete(outPath); }
    }
}
