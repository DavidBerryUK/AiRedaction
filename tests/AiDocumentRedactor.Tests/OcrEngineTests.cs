using AiDocumentRedactor.Core;
using AiDocumentRedactor.Ocr;
using Xunit;
using Xunit.Abstractions;

namespace AiDocumentRedactor.Tests;

/// <summary>Tests that the local OCR engine runs here and reads the synthetic scans in tests/TestCorpus.</summary>
public class OcrEngineTests(ITestOutputHelper output)
{
    /// <summary>Finds a corpus file by walking up from the test folder.</summary>
    static string Corpus(string rel)
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "tests", "TestCorpus", rel))) dir = Path.GetDirectoryName(dir);
        return Path.Combine(dir ?? throw new FileNotFoundException(rel), "tests", "TestCorpus", rel);
    }

    /// <summary>The clean 300 DPI scan and the degraded photocopy-style scan are both read, with word boxes and confidences.</summary>
    [Theory]
    [InlineData("scans/01-hr-letter-scan-clean.png")]
    [InlineData("scans/01-hr-letter-scan-degraded.jpg")]
    public async Task Reads_scans_with_word_positions(string file)
    {
        using var engine = new RapidOcrEngine();
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var page = await engine.RecognizeAsync(await File.ReadAllBytesAsync(Corpus(file)), default);
        var words = page.Lines.SelectMany(l => l.Words).ToList();
        var text = string.Join("\n", page.Lines.Select(l => string.Join(" ", l.Words.Select(w => w.Text))));
        output.WriteLine($"{file}: {page.WidthPx}x{page.HeightPx}, {page.Lines.Count} lines, {words.Count} words, mean confidence {words.Average(w => w.Confidence):0.00}, {sw.Elapsed.TotalSeconds:0.0}s");
        output.WriteLine(text);
        Assert.True(words.Count > 50);
        Assert.All(words, w => { Assert.True(w.Width > 0 && w.Height > 0); Assert.InRange(w.X, -5, page.WidthPx + 5); });
        foreach (var expected in new[] { "Eleanor", "Whitcombe", "promotion", "Insurance" })
            Assert.Contains(words, w => w.Text.Contains(expected, StringComparison.OrdinalIgnoreCase));
    }
}
