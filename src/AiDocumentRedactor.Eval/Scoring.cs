using System.Text.RegularExpressions;
using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.Eval;

/// <summary>A sensitive item the model did not remove.</summary>
public record Leak(string Type, string Text, int Count);
/// <summary>A redaction that covers nothing on the answer key.</summary>
public record FalsePositive(string Type, string Text);
/// <summary>A span a detector produced, kept as positions only (no document text), so combinations of detectors can be scored later without running the models again.</summary>
public record SavedSpan(string Type, int Start, int Length, double Confidence, string Source, bool Flag);

/// <summary>One judged fact about a redaction, kept so a result can be examined item by item and not only as totals. <c>Kind</c> is caught, missed, lost_to_extraction,
/// over_redaction, unjudged, preserve_broken or key_extra (a further occurrence of a key text beyond the count the key gives: not scored for recall, but a redaction of it
/// still matches the key). <c>EntityIndex</c> is the answer-key item (0-based, -1 when none) and <c>Start</c> is -1 when there is no position.</summary>
public record Fact(string Kind, int EntityIndex, string Type, string Text, int Start, int Length);

/// <summary>The scores of one model on one document.</summary>
public class DocScore
{
    /// <summary>The judged facts behind the totals. Built by <see cref="Scoring.Score"/>; not saved with the run.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public List<Fact> Facts = [];
    public string File = string.Empty, Group = string.Empty, Model = string.Empty;
    /// <summary>The kind of document (from the answer key's title), for the by-type table.</summary>
    public string DocType = string.Empty;
    /// <summary>Seconds GLiNER took, when it was used.</summary>
    public double GlinerSeconds;
    /// <summary>What this detector found, as positions only, for combining detectors later.</summary>
    public List<SavedSpan> Spans = [];
    /// <summary>Per category: occurrences that were in the text, and how many of them the model removed.</summary>
    public Dictionary<string, (int Present, int Caught)> ByCategory = new();
    public int Present, Caught, EntitiesPresent, EntitiesFullyCaught;
    /// <summary>Answer-key items (in the body) that never appeared in the extracted text, so the model could not have seen them (OCR misreads).</summary>
    public int LostToExtraction;
    public List<Leak> Leaks = [];
    /// <summary>Redactions made, how many overlap something on the answer key, and how many do not.</summary>
    public int Edits, TruePositives, TypeCorrect;
    public List<FalsePositive> FalsePositives = [];
    /// <summary>Per edit category: edits made and how many were correct.</summary>
    public Dictionary<string, (int Edits, int Correct)> EditsByType = new();
    public int MustPreserve; public List<string> PreserveBroken = [];
    /// <summary>Seconds the model took to find the sensitive items, and seconds to write and verify the redacted file (0 when files were not written).</summary>
    public double DetectSeconds, WriteSeconds;
    public long PromptTokens, OutputTokens; public int Discarded;
    /// <summary>With GLiNER on: things only GLiNER found that were left in the text for a person to review, and how many of them really were sensitive.</summary>
    public int FlagsRaised, FlagsCorrect;
    /// <summary>Redactions that match nothing on the key but could not be judged (a category the key does not label, or text the key was unable to decide), so they count as neither right nor wrong.</summary>
    public int Unjudged;
    /// <summary>Whether the redacted file was written and passed its own safety checks (null = not tried).</summary>
    public bool? OutputOk; public string? OutputError;
}

/// <summary>Compares a redaction with the answer key. Pure: no model, no files.</summary>
public static class Scoring
{
    /// <summary>A pattern for the text that matches whole words only and tolerates different spacing or line breaks inside it.</summary>
    static Regex Pattern(string text)
    {
        var parts = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Select(Regex.Escape);
        var body = string.Join(@"\s+", parts);
        body = body.Replace("@", @"\s?@\s?");   // OCR often reads "name@site" as "name @site"
        var lead = text.Length > 0 && char.IsLetterOrDigit(text[0]) ? @"(?<![\p{L}\p{N}])" : "";
        var trail = text.Length > 0 && char.IsLetterOrDigit(text[^1]) ? @"(?![\p{L}\p{N}])" : "";
        return new Regex(lead + body + trail, RegexOptions.CultureInvariant);
    }

    /// <summary>Where a piece of text occurs in a document (whole words, spacing-tolerant).</summary>
    public static List<(int Start, int Length)> Find(string document, string text) =>
        string.IsNullOrWhiteSpace(text) ? [] : Pattern(text).Matches(document).Select(m => (m.Index, m.Length)).ToList();

    /// <summary>Where each answer-key item sits in the text, with its category (every occurrence).</summary>
    public static List<(int Start, int Length, string Type)> KeySpans(string original, GroundTruth gt) =>
        gt.Entities.SelectMany(e => Find(original, e.Text).Select(p => (p.Start, p.Length, e.Type))).ToList();

    /// <summary>True if a span overlaps something on the answer key.</summary>
    public static bool OverlapsKey(IEnumerable<(int Start, int Length, string Type)> key, int start, int length) =>
        key.Any(k => k.Start < start + length && start < k.Start + k.Length);

    /// <summary>Scores one redaction. <paramref name="original"/> is the text the model saw, <paramref name="result"/> what came out.</summary>
    public static DocScore Score(string original, RedactionResult result, GroundTruth gt, string group)
    {
        var s = new DocScore { Group = group, DocType = gt.Title };
        // Recall: for every answer-key item, how many of its occurrences are gone from the redacted text.
        var active = result.Edits.Where(e => e.Status == EditStatus.Active).ToList();
        for (var index = 0; index < gt.Entities.Count; index++)
        {
            var e = gt.Entities[index];
            var places = Find(original, e.Text);
            var before = places.Count;
            var after = Find(result.RedactedText, e.Text).Count;
            var present = Math.Min(before, e.Occurrences);
            if (present == 0)
            {
                if (e.Where == "body")
                {
                    s.LostToExtraction++;
                    s.Facts.Add(new Fact("lost_to_extraction", index, e.Type, e.Text, -1, 0));
                }

                continue;
            }
            var caught = Math.Min(before - after, present);
            if (caught < 0)
            {
                caught = 0;
            }

            // Which occurrences were removed: the totals above decide how many, and the occurrences most covered by redactions are the ones counted as caught.
            var ranked = places.Take(present)
                .Select(p => (p.Start, p.Length, Covered: active.Sum(a => Math.Max(0, Math.Min(a.OriginalStart + a.OriginalLength, p.Start + p.Length) - Math.Max(a.OriginalStart, p.Start)))))
                .OrderByDescending(p => p.Covered).ThenBy(p => p.Start).ToList();
            for (var n = 0; n < ranked.Count; n++)
            {
                s.Facts.Add(new Fact(n < caught ? "caught" : "missed", index, e.Type, e.Text, ranked[n].Start, ranked[n].Length));
            }

            foreach (var extra in places.Skip(present))
            {
                s.Facts.Add(new Fact("key_extra", index, e.Type, e.Text, extra.Start, extra.Length));
            }

            s.Present += present;
            s.Caught += caught;
            s.EntitiesPresent++;
            if (caught == present)
            {
                s.EntitiesFullyCaught++;
            }
            else
            {
                s.Leaks.Add(new Leak(e.Type, e.Text, present - caught));
            }

            var c = s.ByCategory.GetValueOrDefault(e.Type);
            s.ByCategory[e.Type] = (c.Present + present, c.Caught + caught);
        }
        // Precision: every redaction should cover something on the answer key.
        var keySpans = gt.Entities.SelectMany(e => Find(original, e.Text).Select(p => (p.Start, p.Length, e.Type))).ToList();
        var ignored = (gt.Ignore ?? []).SelectMany(t => Find(original, t)).ToList();
        foreach (var edit in active)
        {
            var hits = keySpans.Where(k => k.Start < edit.OriginalStart + edit.OriginalLength && edit.OriginalStart < k.Start + k.Length).ToList();
            var ok = hits.Count > 0;
            if (!ok && (!gt.Judges(edit.Type) || ignored.Any(g => g.Start < edit.OriginalStart + edit.OriginalLength && edit.OriginalStart < g.Start + g.Length)))
            {
                s.Unjudged++;
                s.Facts.Add(new Fact("unjudged", -1, edit.Type, edit.OriginalText ?? string.Empty, edit.OriginalStart, edit.OriginalLength));
                continue;
            }

            s.Edits++;
            if (ok)
            {
                s.TruePositives++;
                if (hits.Any(h => h.Type == edit.Type))
                {
                    s.TypeCorrect++;
                }
            }
            else
            {
                s.FalsePositives.Add(new FalsePositive(edit.Type, edit.OriginalText ?? string.Empty));
                s.Facts.Add(new Fact("over_redaction", -1, edit.Type, edit.OriginalText ?? string.Empty, edit.OriginalStart, edit.OriginalLength));
            }

            var t = s.EditsByType.GetValueOrDefault(edit.Type);
            s.EditsByType[edit.Type] = (t.Edits + 1, t.Correct + (ok ? 1 : 0));
        }
        // Over-redaction of things that must survive (product names, public bodies, places, ordinary numbers).
        foreach (var keep in gt.MustPreserve ?? [])
        {
            var found = Find(original, keep);
            var before = found.Count;
            if (before == 0)
            {
                continue;
            }

            s.MustPreserve++;
            if (Find(result.RedactedText, keep).Count < before)
            {
                s.PreserveBroken.Add(keep);
                s.Facts.Add(new Fact("preserve_broken", -1, string.Empty, keep, found[0].Start, found[0].Length));
            }
        }
        return s;
    }
}
