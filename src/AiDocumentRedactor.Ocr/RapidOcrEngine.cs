using AiDocumentRedactor.Core;
using RapidOcrNet;
using SkiaSharp;

namespace AiDocumentRedactor.Ocr;

/// <summary>Local OCR using RapidOCR (PaddleOCR PP-OCRv5 models run through ONNX Runtime). The models ship inside the NuGet
/// package, nothing is downloaded, and nothing leaves this machine. Works on macOS, Windows and Linux.</summary>
public sealed class RapidOcrEngine : IOcrEngine, IDisposable
{
    readonly RapidOcr ocr = new();
    readonly SemaphoreSlim gate = new(1, 1);   // the engine is used one image at a time
    bool ready;

    /// <summary>Shown in the model banner.</summary>
    public string Name => "RapidOCR PP-OCRv5 (local)";

    /// <summary>Loads the models on first use (a second or two), then reads the image.</summary>
    public async Task<OcrPage> RecognizeAsync(ReadOnlyMemory<byte> encodedImage, CancellationToken ct)
    {
        await gate.WaitAsync(ct);
        try
        {
            if (!ready)
            {
                InitModels();
                ready = true;
            }
            using var bmp = SKBitmap.Decode(encodedImage.Span) ?? throw new InvalidOperationException("The image could not be decoded.");
            var result = await ocr.DetectAsync(bmp, RapidOcrOptions.Default with {
                ReturnWordBox = true
            }, ct);
            return new OcrPage(bmp.Width, bmp.Height, Lines(result));
        }
        finally { gate.Release(); }
    }

    /// <summary>Loads the models bundled with the package from the app's own folder (not the working folder, which can be anywhere).</summary>
    void InitModels()
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "models", "v5");
        string F(string name) => Path.Combine(dir, name);
        ocr.InitModels(F("ch_PP-OCRv5_mobile_det.onnx"), F("ch_PP-LCNet_x0_25_textline_ori_cls_mobile.onnx"),
            F("latin_PP-OCRv5_rec_mobile_infer.onnx"), F("ppocrv5_latin_dict.txt"), Math.Max(2, Environment.ProcessorCount / 2));
    }

    /// <summary>Turns the engine's blocks into lines of words, ordered top to bottom and left to right.</summary>
    static List<OcrLine> Lines(OcrResult result) =>
        (result.TextBlocks ?? [])
            .Where(b => !string.IsNullOrWhiteSpace(b.Text))
            .Select(b => (Top: b.BoxPoints.Min(p => p.Y), Left: b.BoxPoints.Min(p => p.X), Line: new OcrLine(Words(b))))
            .Where(x => x.Line.Words.Count > 0)
            .OrderBy(x => x.Top).ThenBy(x => x.Left)
            .Select(x => x.Line).ToList();

    /// <summary>The words of one line. If the engine gave no word boxes, the line's box is divided between its words by character count.</summary>
    static List<OcrWord> Words(TextBlock b)
    {
        if (b.WordResults is { Length: > 0 } wr)
        {
            return wr.Where(w => !string.IsNullOrWhiteSpace(w.Text)).Select(w => ToWord(w.Text, w.BoxPoints, w.Score)).ToList();
        }

        var parts = b.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var left = b.BoxPoints.Min(p => p.X);
        var top = b.BoxPoints.Min(p => p.Y);
        var width = b.BoxPoints.Max(p => p.X) - left;
        var height = b.BoxPoints.Max(p => p.Y) - top;
        var total = parts.Sum(s => s.Length + 1);
        var x = (double)left;
        var score = b.CharScores is { Length: > 0 } cs ? cs.Average() : b.BoxScore;
        return parts.Select(s => { var w = width * (s.Length + 1) / total; var word = new OcrWord(s, x, top, w * s.Length / (s.Length + 1), height, score); x += w; return word; }).ToList();   // no outline for this fallback
    }

    /// <summary>The axis-aligned box around a word's corner points.</summary>
    static OcrWord ToWord(string text, SKPointI[] pts, double score)
    {
        var l = pts.Min(p => p.X);
        var t = pts.Min(p => p.Y);
        return new OcrWord(text, l, t, pts.Max(p => p.X) - l, pts.Max(p => p.Y) - t, score, pts.Length == 4 ? pts.Select(p => ((double)p.X, (double)p.Y)).ToList() : null);
    }

    /// <summary>Releases the ONNX sessions.</summary>
    public void Dispose()
    {
        ocr.Dispose();
        gate.Dispose();
    }
}
