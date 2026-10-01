using System.Text.Json;

namespace AiDocumentRedactor.Core;

/// <summary>Where a document is in processing: not yet, running, done, needs a second look, failed or cancelled.</summary>
public enum DocumentStatus { NotProcessed, Processing, Processed, NeedsReview, Error, Cancelled }

/// <summary>Per-document outcome. Holds no document content: only status, counts and an error message.</summary>
public record DocumentStatusRecord(DocumentStatus Status, string? Error, int Edits, DateTime AtUtc, string? Model);

/// <summary>Persists outcomes to status.json in the report directory, shared by the CLI and the UI so that
/// documents processed by either show the right status. Keys are input-relative paths with '/' separators.</summary>
public class StatusStore(string reportDirectory)
{
    readonly string path = Path.Combine(reportDirectory, "status.json");
    readonly object gate = new();

    /// <summary>Normalises a relative path to forward slashes so keys match on Mac and Windows.</summary>
    public static string Key(string relativePath) => relativePath.Replace('\\', '/');

    /// <summary>Reads all saved statuses; a missing or corrupt file gives an empty set.</summary>
    public Dictionary<string, DocumentStatusRecord> Load()
    {
        lock (gate)
        {
            if (!File.Exists(path)) return new();
            try { return JsonSerializer.Deserialize<Dictionary<string, DocumentStatusRecord>>(File.ReadAllText(path), RedactorOptions.JsonOptions) ?? new(); }
            catch (JsonException) { return new(); }   // a corrupt status file must never break the app
        }
    }

    /// <summary>Saves one document's status, keeping the others.</summary>
    public void Set(string relativePath, DocumentStatusRecord record)
    {
        lock (gate)
        {
            var all = Load();
            all[Key(relativePath)] = record;
            Directory.CreateDirectory(reportDirectory);
            File.WriteAllText(path, JsonSerializer.Serialize(all, RedactorOptions.JsonOptions));
        }
    }
}
