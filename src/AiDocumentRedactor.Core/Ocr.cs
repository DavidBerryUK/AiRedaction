namespace AiDocumentRedactor.Core;

/// <summary>One word read by OCR, with its upright bounding box in image pixels (origin top-left) and the engine's confidence (0 to 1).
/// Corners are the word's true outline (4 points clockwise from top-left), which is tilted on a skewed scan; null if the engine gives none.</summary>
public record OcrWord(string Text, double X, double Y, double Width, double Height, double Confidence, IReadOnlyList<(double X, double Y)>? Corners = null);

/// <summary>One line of text read by OCR, as a list of words in reading order.</summary>
public record OcrLine(IReadOnlyList<OcrWord> Words);

/// <summary>The result of reading one page image: its size in pixels and the lines found, top to bottom.</summary>
public record OcrPage(int WidthPx, int HeightPx, IReadOnlyList<OcrLine> Lines);

/// <summary>Reads text from a picture, entirely on this machine. One implementation per OCR engine.</summary>
public interface IOcrEngine
{
    /// <summary>The engine's name, shown in the UI banner (for example "RapidOCR PP-OCRv5").</summary>
    string Name { get; }

    /// <summary>Reads a PNG or JPEG image and returns the lines and words found, with their positions.</summary>
    Task<OcrPage> RecognizeAsync(ReadOnlyMemory<byte> encodedImage, CancellationToken ct);
}
