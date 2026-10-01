using PDFtoImage;
using SkiaSharp;

namespace AiDocumentRedactor.Documents;

/// <summary>Turns one page of a PDF (or an image file) into a PNG for the in-app page view. Everything stays in memory.</summary>
public static class PageRenderer
{
    /// <summary>Renders page <paramref name="page"/> (0-based) of a PDF at the given resolution. An image file is returned as it is (it has one page).</summary>
    public static byte[] RenderPng(byte[] fileBytes, bool isPdf, int page, int dpi)
    {
        if (!isPdf) return fileBytes;
        using var src = new MemoryStream(fileBytes);
        using var bmp = Conversion.ToImage(src, page, options: new RenderOptions(Dpi: dpi));
        using var png = SKImage.FromBitmap(bmp).Encode(SKEncodedImageFormat.Png, 100);
        return png.ToArray();
    }
}
