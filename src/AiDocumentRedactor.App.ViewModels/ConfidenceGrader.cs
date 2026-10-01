using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.App.ViewModels;

/// <summary>Grades each edit of the active result by agreement with the other models' results (spec §7.3b).
/// High = every model found it; Low = fewer than half did; otherwise Medium. A single model run cannot be
/// graded, so everything is Medium and the reason says how to get a real grade.</summary>
public static class ConfidenceGrader
{
    /// <summary>True if two edits cover any of the same characters of the original text.</summary>
    static bool Overlaps(RedactionEdit a, RedactionEdit b) =>
        a.OriginalStart < b.OriginalStart + b.OriginalLength && b.OriginalStart < a.OriginalStart + a.OriginalLength;

    /// <summary>Returns a confidence level and reason for each edit of the active result, keyed by edit id.</summary>
    public static Dictionary<int, EditConfidence> Grade(ModelResult active, IReadOnlyList<ModelResult> voters)
    {
        var others = voters.Where(v => v.Id != active.Id && v.Model != active.Model).ToList();
        var total = others.Count + 1;
        var map = new Dictionary<int, EditConfidence>();
        foreach (var e in active.Result.Edits.Where(e => e.Status == EditStatus.Active))
        {
            var missedBy = others.Where(o => !o.Result.Edits.Any(x => x.Status == EditStatus.Active && Overlaps(e, x))).Select(o => o.Model).ToList();
            var votes = total - missedBy.Count;
            ConfidenceLevel level; string reason;
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
            map[e.Id] = new EditConfidence(level, votes, total, reason);
        }
        return map;
    }
}
