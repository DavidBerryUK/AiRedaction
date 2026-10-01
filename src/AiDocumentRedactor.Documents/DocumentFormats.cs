using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.Documents;

/// <summary>The one place that lists every supported document format, so the app and the command line stay in step.</summary>
public static class DocumentFormats
{
    /// <summary>Readers for text files, Word documents, PDFs (with OCR for scanned pages) and, when an OCR engine is given, PNG/JPG scans.</summary>
    public static List<IDocumentReader> Readers(RedactorOptions options, IOcrEngine? ocr)
    {
        var readers = new List<IDocumentReader> { new TextDocumentReader(), new PdfDocumentReader(ocr, options.Ocr), new DocxDocumentReader() };
        if (ocr is not null) readers.Add(new ImageDocumentReader(ocr, options.Ocr));
        return readers;
    }

    /// <summary>Writers matching the readers: text, redacted PDF, redacted image and redacted Word document.</summary>
    public static List<IDocumentWriter> Writers(RedactorOptions options, IOcrEngine? ocr) =>
        [new TextDocumentWriter(), new PdfDocumentWriter(options.Pdf, ocr, options.Ocr), new ImageDocumentWriter(options.Pdf, ocr, options.Ocr), new DocxDocumentWriter()];
}
