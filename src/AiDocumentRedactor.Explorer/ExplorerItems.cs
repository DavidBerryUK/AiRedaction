namespace AiDocumentRedactor.Explorer;

public partial class ExplorerService
{
    const int PlacesShown = 12;

    /// <summary>Which setups found an item and which missed it. For <paramref name="kind"/> "missed", the item is a sensitive text on the answer key: each setup's caught and missed
    /// occurrences of it. For "over_redaction" the item is a text some setups redacted that the key says is not sensitive: each setup's redacted and not-redacted occurrences of it.
    /// Setups and documents follow the filter.</summary>
    public async Task<ItemDetail> ItemDetailAsync(Filter f, string kind, string type, string text)
    {
        var withSource = f with { Source = f.Source ?? "batch" };
        return kind == "missed" ? await MissedDetailAsync(withSource, type, text) : await OverRedactedDetailAsync(withSource, type, text);
    }

    async Task<ItemDetail> MissedDetailAsync(Filter f, string type, string text)
    {
        var rows = await QueryAsync(cmd =>
        {
            cmd.Parameters.AddWithValue("@type", type);
            cmd.Parameters.AddWithValue("@text", text);
            return "SELECT r.config, r.model, r.variant, o.doc_id, o.entity_id, o.start, o.length, o.kind FROM outcomes o JOIN results r ON r.result_id = o.result_id JOIN documents d ON d.doc_id = r.doc_id " +
                $"WHERE o.kind IN ('caught', 'missed') AND o.type = @type AND o.text = @text AND r.status = 'ok' AND {Where(f, cmd)}";
        }, r => (Config: Str(r, 0), Model: Str(r, 1), Variant: Str(r, 2), Doc: Str(r, 3), Entity: Str(r, 4), Start: Int(r, 5), Length: Int(r, 6), Caught: Str(r, 7) == "caught"));
        var setups = rows.GroupBy(r => (r.Config, r.Model, r.Variant)).Select(g => new SetupVerdict(g.Key.Config, g.Key.Model, g.Key.Variant, g.Count(x => x.Caught), g.Count(x => !x.Caught))).ToList();
        var places = rows.Where(r => !r.Caught).Select(r => (r.Doc, r.Start, r.Length)).Distinct().ToList();
        var occurrences = rows.Select(r => (r.Doc, r.Entity, r.Start, r.Length)).Distinct().Count();
        return new ItemDetail("missed", type, text, occurrences, rows.Select(r => r.Doc).Distinct().Count(), Order(setups, missedFirst: true), await PlacesAsync(type, text, "missed", places));
    }

    async Task<ItemDetail> OverRedactedDetailAsync(Filter f, string type, string text)
    {
        // Where it was redacted: any setup's redaction of this text at a place (a document and position).
        var redactions = await QueryAsync(cmd =>
        {
            cmd.Parameters.AddWithValue("@text", text);
            return "SELECT r.config, r.model, r.variant, o.doc_id, o.start, o.length, o.kind FROM outcomes o JOIN results r ON r.result_id = o.result_id JOIN documents d ON d.doc_id = r.doc_id " +
                $"WHERE o.kind IN ('over_redaction', 'unjudged') AND o.text = @text AND o.start IS NOT NULL AND r.status = 'ok' AND {Where(f, cmd)}";
        }, r => (Config: Str(r, 0), Model: Str(r, 1), Variant: Str(r, 2), Doc: Str(r, 3), Start: Int(r, 4), Length: Int(r, 5), Kind: Str(r, 6)));
        // The places that count are those where it was wrongly redacted (an over-redaction) in the category named, by at least one setup.
        var placeKinds = await QueryAsync(cmd =>
        {
            cmd.Parameters.AddWithValue("@type", type);
            cmd.Parameters.AddWithValue("@text", text);
            return "SELECT DISTINCT o.doc_id, o.start, o.length FROM outcomes o JOIN results r ON r.result_id = o.result_id JOIN documents d ON d.doc_id = r.doc_id " +
                $"WHERE o.kind = 'over_redaction' AND o.type = @type AND o.text = @text AND o.start IS NOT NULL AND r.status = 'ok' AND {Where(f, cmd)}";
        }, r => (Doc: Str(r, 0), Start: Int(r, 1), Length: Int(r, 2)));
        var places = placeKinds.ToHashSet();
        var docs = places.Select(p => p.Doc).Distinct().ToList();
        if (docs.Count == 0)
        {
            return new ItemDetail("over_redaction", type, text, 0, 0, [], []);
        }

        // Every setup that finished those documents either redacted the place or left it alone.
        var finished = await QueryAsync(cmd =>
        {
            var docNames = ListParameters(cmd, "dd", docs);
            return $"SELECT r.config, r.model, r.variant, r.doc_id FROM results r JOIN documents d ON d.doc_id = r.doc_id WHERE r.status = 'ok' AND r.doc_id IN ({docNames}) AND {Where(f, cmd)}";
        }, r => (Config: Str(r, 0), Model: Str(r, 1), Variant: Str(r, 2), Doc: Str(r, 3)));
        var redactedAt = redactions.Where(r => places.Contains((r.Doc, r.Start, r.Length))).GroupBy(r => r.Config).ToDictionary(g => g.Key, g => g.Select(x => (x.Doc, x.Start, x.Length)).ToHashSet());
        var setups = new List<SetupVerdict>();
        foreach (var g in finished.GroupBy(x => (x.Config, x.Model, x.Variant)))
        {
            var docSet = g.Select(x => x.Doc).ToHashSet();
            var eligible = places.Count(p => docSet.Contains(p.Doc));
            var done = redactedAt.TryGetValue(g.Key.Config, out var at) ? at.Count(p => docSet.Contains(p.Doc)) : 0;
            setups.Add(new SetupVerdict(g.Key.Config, g.Key.Model, g.Key.Variant, done, Math.Max(0, eligible - done)));
        }

        return new ItemDetail("over_redaction", type, text, places.Count, docs.Count, Order(setups, missedFirst: false), await PlacesAsync(type, text, "over_redaction", places.ToList()));
    }

    /// <summary>Setups with the worst record on the item first: the most misses for a missed item, the most wrong redactions for an over-redacted one. Baselines last.</summary>
    static List<SetupVerdict> Order(List<SetupVerdict> setups, bool missedFirst) => setups
        .OrderBy(s => s.Model.Length == 0 ? 1 : 0)
        .ThenByDescending(s => missedFirst ? s.NotFound : s.Found)
        .ThenBy(s => s.Config, StringComparer.OrdinalIgnoreCase).ToList();

    async Task<IReadOnlyList<OutcomeItem>> PlacesAsync(string type, string text, string kind, List<(string Doc, int Start, int Length)> places)
    {
        var shown = places.OrderBy(p => p.Doc, StringComparer.Ordinal).ThenBy(p => p.Start).Take(PlacesShown).ToList();
        var texts = await TextsAsync(shown.Select(p => p.Doc).Distinct().ToList());
        return shown.Select(p =>
        {
            var (before, after) = Context(texts.GetValueOrDefault(p.Doc), p.Start, p.Length);
            return new OutcomeItem(p.Doc, string.Empty, kind, type, text, before, after);
        }).ToList();
    }
}
