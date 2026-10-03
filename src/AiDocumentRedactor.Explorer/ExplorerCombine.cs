using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.Explorer;

/// <summary>How to combine detectors: which models, whether GLiNER votes too, how many must agree, what happens to what only some found, and whether to assume a perfect reviewer.</summary>
public record CombineChoice(IReadOnlyList<string> Models, bool IncludeGliner, int Needed, bool FlagRest, bool IdealReviewer);

/// <summary>The totals for one row of the combination table.</summary>
public record CombineRow(string Name, int Documents, int Present, int Caught, int Edits, int TruePositives, int Unjudged, int FlagsRaised, int FlagsCorrect)
{
    public int Missed => Present - Caught;
    public int OverRedactions => Edits - TruePositives;
    public double Recall => Present == 0 ? 0 : (double)Caught / Present;
    public double Precision => Edits == 0 ? 0 : (double)TruePositives / Edits;
    public double F1 => Stats.F1(Recall, Precision);
    public (double Low, double High) RecallRange => Stats.Wilson(Caught, Present);
    public (double Low, double High) PrecisionRange => Stats.Wilson(TruePositives, Edits);
}

public partial class ExplorerService
{
    const string GlinerSetup = "GLiNER only";

    /// <summary>Scores combinations of the saved detectors without running any model: each chosen model on its own, and the combination. Worked out from the stored spans and the
    /// key occurrences, over the documents every chosen detector succeeded on. It matches the evaluation's own scoring except where a key text appears more often than the key counts.</summary>
    public async Task<List<CombineRow>> CombineAsync(Filter f, CombineChoice choice)
    {
        var detectors = choice.Models.Where(m => m.Length > 0).Distinct().ToList();
        if (detectors.Count == 0)
        {
            return [];
        }

        var wanted = choice.IncludeGliner ? [.. detectors, GlinerSetup] : detectors;
        var docFilter = f with { Variants = null, Configs = null, Status = null, Source = null };
        var docs = await QueryAsync(cmd =>
        {
            var where = Where(docFilter, cmd, includeResults: false);
            var names = ListParameters(cmd, "w", wanted);
            return $"SELECT d.doc_id, d.chars FROM documents d WHERE {where} AND (SELECT COUNT(DISTINCT r.config) FROM results r WHERE r.doc_id = d.doc_id AND r.status = 'ok' AND r.source = 'batch' AND r.config IN ({names})) = {wanted.Count}";
        }, r => (Id: Str(r, 0), Chars: Int(r, 1)));
        if (docs.Count == 0)
        {
            return [];
        }

        var ids = docs.Select(d => d.Id).ToHashSet();
        var spans = (await QueryAsync(cmd =>
        {
            var where = Where(docFilter, cmd, includeResults: false);
            return "SELECT r.doc_id, r.config, s.type, s.start, s.length, s.confidence, s.source, s.flag FROM spans s JOIN results r ON r.result_id = s.result_id JOIN documents d ON d.doc_id = r.doc_id " +
                $"WHERE r.status = 'ok' AND r.source = 'batch' AND r.config IN ({ListParameters(cmd, "w", wanted)}) AND {where}";
        }, r => (Doc: Str(r, 0), Config: Str(r, 1), Span: new DetectedEntity(Str(r, 2), Int(r, 3), Int(r, 4), NDouble(r, 5) ?? 1, Str(r, 6), r.GetInt64(7) == 1))))
            .Where(x => ids.Contains(x.Doc)).GroupBy(x => x.Doc).ToDictionary(g => g.Key, g => g.GroupBy(x => x.Config).ToDictionary(c => c.Key, c => c.Select(x => x.Span).ToList()));

        // Every key occurrence is a caught or missed fact of any one setup's result, so one setup's facts give them all.
        var keyOccurrences = (await QueryAsync(cmd =>
        {
            cmd.Parameters.AddWithValue("@first", detectors[0]);
            var where = Where(docFilter, cmd, includeResults: false);
            return "SELECT o.doc_id, o.start, o.length, o.kind FROM outcomes o JOIN results r ON r.result_id = o.result_id JOIN documents d ON d.doc_id = r.doc_id " +
                $"WHERE o.kind IN ('caught', 'missed', 'key_extra') AND o.start IS NOT NULL AND r.config = @first AND r.status = 'ok' AND r.source = 'batch' AND {where}";
        }, r => (Doc: Str(r, 0), Start: Int(r, 1), Length: Int(r, 2), Counted: Str(r, 3) != "key_extra"))).Where(x => ids.Contains(x.Doc)).GroupBy(x => x.Doc).ToDictionary(g => g.Key, g => g.Select(x => (x.Start, x.Length, x.Counted)).ToList());

        // Redactions the key cannot judge (a category it does not label) count as neither right nor wrong.
        var unjudged = (await QueryAsync(cmd =>
        {
            var where = Where(docFilter, cmd, includeResults: false);
            return "SELECT o.doc_id, o.type, o.start, o.length FROM outcomes o JOIN results r ON r.result_id = o.result_id JOIN documents d ON d.doc_id = r.doc_id " +
                $"WHERE o.kind = 'unjudged' AND r.status = 'ok' AND r.source = 'batch' AND r.config IN ({ListParameters(cmd, "w", wanted)}) AND {where}";
        }, r => (Doc: Str(r, 0), Type: Str(r, 1), Start: Int(r, 2), Length: Int(r, 3)))).Where(x => ids.Contains(x.Doc)).Select(x => (x.Doc, x.Type, x.Start, x.Length)).ToHashSet();

        List<DetectedEntity> SpansOf(string doc, string setup) => spans.TryGetValue(doc, out var byConfig) && byConfig.TryGetValue(setup, out var list) ? list : [];
        var rows = new List<CombineRow>();
        foreach (var model in detectors)
        {
            rows.Add(Score($"{model} alone", docs, (doc, _) => SpansOf(doc, model), keyOccurrences, unjudged, false));
        }

        if (detectors.Count > 1 || choice.IncludeGliner)
        {
            var voters = choice.IncludeGliner ? [.. detectors, GlinerSetup] : detectors;
            var needed = Math.Clamp(choice.Needed, 1, voters.Count);
            // "Any one finds it" is the plain union of every detector's spans; voting keeps one span per overlapping group, which can cover less.
            rows.Add(Score(Describe(choice, needed), docs, (doc, _) => needed <= 1
                ? [.. voters.SelectMany(v => SpansOf(doc, v))]
                : Voting.Vote([.. voters.Select(v => (IReadOnlyList<DetectedEntity>)SpansOf(doc, v))], needed, choice.FlagRest), keyOccurrences, unjudged, choice.IdealReviewer));
        }

        return rows;
    }

    /// <summary>The combination's name in the table.</summary>
    public static string Describe(CombineChoice c, int needed)
    {
        var voters = c.Models.Count + (c.IncludeGliner ? 1 : 0);
        var rule = needed <= 1 ? "union (any one finds it)" : needed >= voters ? $"all {voters} agree" : $"at least {needed} of {voters} agree";
        return $"{rule}{(c.FlagRest ? ", the rest flagged" : string.Empty)}{(c.FlagRest && c.IdealReviewer ? " (ideal reviewer)" : string.Empty)}";
    }

    static CombineRow Score(string name, List<(string Id, int Chars)> docs, Func<string, int, IReadOnlyList<DetectedEntity>> spansOf,
        Dictionary<string, List<(int Start, int Length, bool Counted)>> keys, HashSet<(string Doc, string Type, int Start, int Length)> unjudged, bool idealReviewer)
    {
        int present = 0, caught = 0, edits = 0, tp = 0, unj = 0, flagsRaised = 0, flagsCorrect = 0;
        foreach (var (id, chars) in docs)
        {
            var key = keys.GetValueOrDefault(id) ?? [];
            bool OverlapsKey(DetectedEntity s) => key.Any(k => k.Start < s.Start + s.Length && s.Start < k.Start + k.Length);
            var all = spansOf(id, chars);
            var auto = Redactor.Merge(all, chars);
            // A flag on something the key cannot judge is left out of the flag counts, as in the evaluation.
            var raised = auto.Where(s => s.Flag && s.Source is "gliner-only" or "single-detector" && !unjudged.Contains((id, s.Type, s.Start, s.Length))).ToList();
            flagsRaised += raised.Count;
            flagsCorrect += raised.Count(OverlapsKey);
            // An ideal reviewer accepts exactly the flags that really were sensitive, so those become redactions.
            var final = idealReviewer ? Redactor.Merge(all.Select(s => s.Flag && s.Source is "gliner-only" or "single-detector" && OverlapsKey(s) ? s with { Flag = false } : s), chars) : auto;
            var redactions = final.Where(s => !s.Flag).ToList();
            present += key.Count(k => k.Counted);
            caught += key.Where(k => k.Counted).Count(k => redactions.Any(s => s.Start < k.Start + k.Length && k.Start < s.Start + s.Length));
            foreach (var s in redactions)
            {
                if (OverlapsKey(s))
                {
                    edits++;
                    tp++;
                }
                else if (unjudged.Contains((id, s.Type, s.Start, s.Length)))
                {
                    unj++;
                }
                else
                {
                    edits++;
                }
            }
        }

        return new CombineRow(name, docs.Count, present, caught, edits, tp, unj, flagsRaised, flagsCorrect);
    }
}
