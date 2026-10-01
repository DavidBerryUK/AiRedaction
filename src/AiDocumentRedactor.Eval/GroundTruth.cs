using System.Text.Json;

namespace AiDocumentRedactor.Eval;

/// <summary>One thing that must be redacted: its category, its exact text, and how many times it appears in the source document.</summary>
public record GtEntity(string Where, string Type, string Text, int Occurrences);

/// <summary>The answer key for one source document: what must be redacted and what must survive.</summary>
public record GroundTruth(string Id, string Title, List<GtEntity> Entities, List<string>? MustPreserve);

/// <summary>Reads the ground-truth files of the test corpus.</summary>
public static class GroundTruthStore
{
    static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Loads every <c>ground-truth/*.json</c> under the corpus folder.</summary>
    public static List<GroundTruth> Load(string corpusDir) =>
        Directory.GetFiles(Path.Combine(corpusDir, "ground-truth"), "*.json").Order()
            .Select(f => JsonSerializer.Deserialize<GroundTruth>(File.ReadAllText(f), Json) ?? throw new InvalidDataException(f)).ToList();

    /// <summary>Finds the answer key for a corpus file by its name (the file name starts with the document id), or null if there is none.</summary>
    public static GroundTruth? For(string fileName, IEnumerable<GroundTruth> all) =>
        all.Where(g => fileName.StartsWith(g.Id, StringComparison.OrdinalIgnoreCase)).OrderByDescending(g => g.Id.Length).FirstOrDefault();

    /// <summary>A reader-friendly format group for a corpus file: plain text, Word, PDF with text, or one of the three kinds of scan.</summary>
    public static string FormatGroup(string path)
    {
        var name = Path.GetFileName(path).ToLowerInvariant(); var ext = Path.GetExtension(name);
        if (name.Contains("-scan-clean")) return "Scan: clean image";
        if (name.Contains("-scan-degraded")) return "Scan: degraded image";
        if (name.Contains("-scan") && ext == ".pdf") return "Scan: image-only PDF";
        return ext switch { ".docx" => "Word", ".pdf" => "PDF (text layer)", _ => "Plain text" };
    }
}
