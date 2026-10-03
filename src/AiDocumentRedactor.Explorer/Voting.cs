using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.Explorer;

/// <summary>Combining the spans of several detectors by voting. Shared by the evaluation's combination report and the explorer's combination tab, so both give the same answer.</summary>
public static class Voting
{
    /// <summary>Spans from several detectors, grouped where they overlap. A group is redacted when at least <paramref name="needed"/> different detectors found it;
    /// otherwise it is flagged (left in the text) when <paramref name="flagRest"/> is set, or dropped. Flagged spans of a single detector pass through unchanged.</summary>
    public static List<DetectedEntity> Vote(IReadOnlyList<IReadOnlyList<DetectedEntity>> voters, int needed, bool flagRest)
    {
        var all = voters.SelectMany((spans, i) => spans.Where(s => !s.Flag).Select(s => (Span: s, Voter: i))).OrderBy(x => x.Span.Start).ThenBy(x => x.Voter).ToList();
        var result = voters.SelectMany(v => v.Where(s => s.Flag)).ToList();
        var group = new List<(DetectedEntity Span, int Voter)>();
        var end = -1;
        void Close()
        {
            if (group.Count == 0)
            {
                return;
            }

            var votes = group.Select(g => g.Voter).Distinct().Count();
            var best = group.OrderBy(g => g.Voter).ThenByDescending(g => g.Span.Length).First().Span;
            if (votes >= needed)
            {
                result.Add(best with { Source = best.Source + (votes > 1 ? $"+{votes}" : string.Empty) });
            }
            else if (flagRest)
            {
                result.Add(best with { Flag = true, Source = "single-detector" });
            }

            group.Clear();
        }

        foreach (var x in all)
        {
            if (group.Count > 0 && x.Span.Start >= end)
            {
                Close();
            }

            group.Add(x);
            end = Math.Max(end, x.Span.Start + x.Span.Length);
            if (group.Count == 1)
            {
                end = x.Span.Start + x.Span.Length;
            }
        }

        Close();
        return result;
    }
}
