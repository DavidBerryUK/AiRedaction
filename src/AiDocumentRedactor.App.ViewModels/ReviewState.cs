using System.Security.Cryptography;
using System.Text.Json;
using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.App.ViewModels;

/// <summary>A span a person chose to redact (offsets into the original text, no text content).</summary>
public record ManualSpan(string Type, int Start, int Length);

/// <summary>An AI edit a person rejected, identified by where it sits in the original text.</summary>
public record SpanKey(int Start, int Length);

/// <summary>Everything a reviewer has changed for one document: spans they added and AI edits they rejected. Offsets only, never the
/// text itself (FR9). It applies on top of any model's result, so the same review carries across models.</summary>
public record ReviewState(IReadOnlyList<ManualSpan> Manual, IReadOnlyList<SpanKey> Rejected, IReadOnlyList<AreaBox> Areas)
{
    /// <summary>A review with no hand-drawn areas.</summary>
    public ReviewState(IReadOnlyList<ManualSpan> manual, IReadOnlyList<SpanKey> rejected) : this(manual, rejected, []) { }

    /// <summary>No changes.</summary>
    public static ReviewState Empty { get; } = new([], [], []);

    /// <summary>True when nothing has been changed.</summary>
    public bool IsEmpty => Manual.Count == 0 && Rejected.Count == 0 && Areas.Count == 0;

    /// <summary>True if both states hold the same changes (records of lists compare by reference, so this compares the contents).</summary>
    public bool SameAs(ReviewState o) => Manual.Count == o.Manual.Count && Rejected.Count == o.Rejected.Count && Areas.Count == o.Areas.Count
        && Areas.OrderBy(a => a.Page).ThenBy(a => a.X).ThenBy(a => a.Y).SequenceEqual(o.Areas.OrderBy(a => a.Page).ThenBy(a => a.X).ThenBy(a => a.Y))
        && Manual.OrderBy(m => m.Start).ThenBy(m => m.Length).ThenBy(m => m.Type).SequenceEqual(o.Manual.OrderBy(m => m.Start).ThenBy(m => m.Length).ThenBy(m => m.Type))
        && Rejected.OrderBy(r => r.Start).ThenBy(r => r.Length).SequenceEqual(o.Rejected.OrderBy(r => r.Start).ThenBy(r => r.Length));

    /// <summary>The model's result with this review applied: rejected edits are left in the text (and listed as Rejected so they can be
    /// restored), manual spans are redacted, and the redacted text and every position are rebuilt.</summary>
    public RedactionResult ApplyTo(string text, RedactionResult model, string template)
    {
        var rejected = Rejected.ToHashSet();
        var spans = Manual.Select(m => new DetectedEntity(m.Type, m.Start, m.Length, 1, "human"))
            .Concat(model.Edits.Where(e => e.Status != EditStatus.Rejected && !rejected.Contains(new SpanKey(e.OriginalStart, e.OriginalLength)))
                .Select(e => new DetectedEntity(e.Type, e.OriginalStart, e.OriginalLength, e.Confidence, e.Source, e.Status == EditStatus.Flagged)));
        var built = Redactor.Apply(text, spans, template);
        if (Areas.Count > 0)
        {
            built = built with {
                Areas = Areas
            };
        }

        if (rejected.Count == 0)
        {
            return built;
        }

        var edits = built.Edits.ToList();
        var id = edits.Count == 0 ? 1 : edits.Max(e => e.Id) + 1;
        foreach (var e in model.Edits.Where(e => e.Status != EditStatus.Rejected && rejected.Contains(new SpanKey(e.OriginalStart, e.OriginalLength))))
        {
            // Left in the text: skip it if a redaction now covers the same characters, otherwise find where it sits in the new text.
            if (built.Edits.Any(x => x.Status == EditStatus.Active && x.OriginalStart < e.OriginalStart + e.OriginalLength && e.OriginalStart < x.OriginalStart + x.OriginalLength))
            {
                continue;
            }

            var shift = built.Edits.Where(x => x.Status == EditStatus.Active && x.OriginalStart + x.OriginalLength <= e.OriginalStart).Sum(x => x.RedactedLength - x.OriginalLength);
            edits.Add(new RedactionEdit(id++, e.Type, e.OriginalStart, e.OriginalLength, e.OriginalStart + shift, e.OriginalLength, string.Empty, e.OriginalText, e.Confidence, e.Source, EditStatus.Rejected));
        }
        return built with {
            Edits = edits
        };
    }
}

/// <summary>Keeps each document's review state in a small JSON file named after the SHA-256 of the input file, so it only ever applies to
/// that exact file. The file holds offsets and categories only, no document text.</summary>
public class ReviewStore(string directory)
{
    record Stored(int Version, List<ManualSpan> Manual, List<SpanKey> Rejected, List<AreaBox>? Areas = null);

    /// <summary>The file this document's review is kept in.</summary>
    string FileFor(string inputPath) => Path.Combine(directory, Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(inputPath))) + ".json");

    /// <summary>Loads the saved review for this exact file, or an empty one if there is none or it cannot be read.</summary>
    public ReviewState Load(string inputPath)
    {
        try
        {
            var f = FileFor(inputPath);
            if (!File.Exists(f))
            {
                return ReviewState.Empty;
            }

            var s = JsonSerializer.Deserialize<Stored>(File.ReadAllText(f));
            return s is null ? ReviewState.Empty : new ReviewState(s.Manual, s.Rejected, s.Areas ?? []);
        }
        catch (Exception ex) when (ex is IOException or JsonException)
        {
            return ReviewState.Empty;
        }
    }

    /// <summary>Saves the review (or removes the file when there is nothing to keep).</summary>
    public void Save(string inputPath, ReviewState state)
    {
        var f = FileFor(inputPath);
        if (state.IsEmpty)
        {
            File.Delete(f);
            return;
        }
        Directory.CreateDirectory(directory);
        File.WriteAllText(f, JsonSerializer.Serialize(new Stored(2, [.. state.Manual], [.. state.Rejected], [.. state.Areas])));
    }
}
