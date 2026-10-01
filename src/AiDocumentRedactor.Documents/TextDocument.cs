using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.Documents;

/// <summary>Reads plain-text formats (txt, md, csv, json, xml, yaml, html, log) as text.</summary>
public class TextDocumentReader : IDocumentReader
{
    static readonly HashSet<string> Exts = [".txt", ".md", ".csv", ".log", ".json", ".xml", ".yaml", ".yml", ".html"];
    /// <summary>True for the supported text extensions.</summary>
    public bool CanRead(string path) => Exts.Contains(Path.GetExtension(path).ToLowerInvariant());
    /// <summary>Reads the whole file as text.</summary>
    public async Task<ExtractedDocument> ReadAsync(string path, CancellationToken ct) =>
        new(path, "text", await File.ReadAllTextAsync(path, ct));
}

/// <summary>Writes redacted plain-text documents.</summary>
public class TextDocumentWriter : IDocumentWriter
{
    /// <summary>True for documents read by the text reader.</summary>
    public bool CanWrite(ExtractedDocument source) => source.Format == "text";
    /// <summary>Creates the output folder if needed and writes the redacted text.</summary>
    public async Task WriteAsync(ExtractedDocument source, RedactionResult result, string outputPath, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath))!);
        await File.WriteAllTextAsync(outputPath, result.RedactedText, ct);
    }
}
