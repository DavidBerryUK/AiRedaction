using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace AiDocumentRedactor.Explorer.Dataset;

/// <summary>Writes a dataset folder: run.json, the five CSV files and document-text.jsonl (see documentation/RESULTS_DATASET_FORMAT.md).</summary>
public static class DatasetWriter
{
    static readonly JsonSerializerOptions Pretty = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    static readonly JsonSerializerOptions Line = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    /// <summary>The SHA-256 of a text's UTF-8 bytes as lower-case hex: the hash recorded for every document's text.</summary>
    public static string HashText(string text) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

    /// <summary>Writes the dataset into <paramref name="dir"/> (created if needed). Existing files of the same names are replaced.</summary>
    public static async Task WriteAsync(string dir, DatasetData data)
    {
        Directory.CreateDirectory(dir);
        await File.WriteAllTextAsync(Path.Combine(dir, Schema.RunFile), JsonSerializer.Serialize(data.Run, Pretty) + "\n", new UTF8Encoding(false));
        await Csv.WriteAsync(Path.Combine(dir, Schema.DocumentsFile), Schema.Documents, data.Documents.Select(d => d.Cells()));
        await Csv.WriteAsync(Path.Combine(dir, Schema.EntitiesFile), Schema.Entities, data.Entities.Select(e => e.Cells()));
        await Csv.WriteAsync(Path.Combine(dir, Schema.ResultsFile), Schema.Results, data.Results.Select(r => r.Cells()));
        await Csv.WriteAsync(Path.Combine(dir, Schema.SpansFile), Schema.Spans, data.Spans.Select(s => s.Cells()));
        await Csv.WriteAsync(Path.Combine(dir, Schema.OutcomesFile), Schema.Outcomes, data.Outcomes.Select(o => o.Cells()));
        await using var text = new StreamWriter(Path.Combine(dir, Schema.TextFile), false, new UTF8Encoding(false)) { NewLine = "\n" };
        foreach (var t in data.Texts)
        {
            await text.WriteLineAsync(JsonSerializer.Serialize(t, Line));
        }
    }
}
