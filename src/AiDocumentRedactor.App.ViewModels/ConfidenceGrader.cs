using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.App.ViewModels;

/// <summary>Extra facts the grader uses beyond agreement between models: how sure OCR was about the words an edit covers, and per-category ceilings.</summary>
public record ConfidenceContext(Func<RedactionEdit, double?> OcrConfidenceOf, double MinOcrConfidence, IReadOnlyDictionary<string, string> CategoryCaps);

/// <summary>Grades each edit of the active result by agreement with the other models' results (spec §7.3b).
/// High = every model found it; Low = fewer than half did; otherwise Medium. A single model run cannot be
/// graded, so everything is Medium and the reason says how to get a real grade.</summary>
public static class ConfidenceGrader
{
    /// <summary>One step down: High becomes Medium, anything else becomes Low.</summary>
    static ConfidenceLevel Lower(ConfidenceLevel l) => l == ConfidenceLevel.High ? ConfidenceLevel.Medium : ConfidenceLevel.Low;

    /// <summary>True if two edits cover any of the same characters of the original text.</summary>
    static bool Overlaps(RedactionEdit a, RedactionEdit b) =>
        a.OriginalStart < b.OriginalStart + b.OriginalLength && b.OriginalStart < a.OriginalStart + a.OriginalLength;

    /// <summary>Returns a confidence level and reason for each edit of the active result, keyed by edit id.</summary>
    public static Dictionary<int, EditConfidence> Grade(ModelResult active, IReadOnlyList<ModelResult> voters, ConfidenceContext? ctx = null)
    {
        var others = voters.Where(v => v.Id != active.Id && v.Model != active.Model).ToList();
        var total = others.Count + 1;
        var map = new Dictionary<int, EditConfidence>();
        foreach (var e in active.Result.Edits.Where(e => e.Status == EditStatus.Active))
        {
            var missedBy = others.Where(o => !o.Result.Edits.Any(x => x.Status == EditStatus.Active && x.Source != "human" && Overlaps(e, x))).Select(o => o.Model).ToList();
            var votes = total - missedBy.Count;
            ConfidenceLevel level;
            string reason;
            if (total == 1)
            {
                level = ConfidenceLevel.Medium;
                reason = "Only one model has run, so agreement can't be measured. Tick more models under ⚙ Models and re-run to grade confidence.";
            }
            else
            {
                level = votes == total ? ConfidenceLevel.High : (double)votes / total < 0.5 ? ConfidenceLevel.Low : ConfidenceLevel.Medium;
                reason = $"Found by {votes} of {total} models" + (missedBy.Count > 0 ? $" · missed by {string.Join(", ", missedBy)}" : "");
            }
            // Further signals, each of which can only lower the grade (a person's own edit is the exception).
            var notes = new List<string>();
            if (e.Source == "human")
            {
                level = ConfidenceLevel.High;
                reason = "Added by a person.";
            }
            else
            {
                if (e.Source == "llm-variant")
                {
                    level = Lower(level);
                    notes.Add("only a shorter form of something longer the model found");
                }
                if (ctx?.OcrConfidenceOf(e) is { } oc && oc < ctx.MinOcrConfidence)
                {
                    level = ConfidenceLevel.Low;
                    notes.Add($"low OCR confidence ({oc:P0})");
                }
                if (ctx is not null && ctx.CategoryCaps.TryGetValue(e.Type, out var cap) && Enum.TryParse<ConfidenceLevel>(cap, true, out var ceiling) && level > ceiling)
                {
                    level = ceiling;
                    notes.Add($"{e.Type} is limited to {ceiling}");
                }
                if (notes.Count > 0)
                {
                    reason += " · " + string.Join(" · ", notes);
                }
            }
            map[e.Id] = new EditConfidence(level, votes, total, reason);
        }
        return map;
    }
}
