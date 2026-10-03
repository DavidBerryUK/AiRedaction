using System.Data.Common;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace AiDocumentRedactor.Explorer;

/// <summary>Answers the explorer's questions from the results database: summaries per setup, the results grid, document difficulty, one document in detail, and where
/// categories are missed. Every query is fixed and parameterised; nothing a user types becomes SQL (sort columns come from a list).</summary>
public partial class ExplorerService(string databasePath)
{
    /// <summary>Characters shown either side of an item in the lists.</summary>
    const int ContextChars = 40;

    /// <summary>The path of the database this service reads.</summary>
    public string DatabasePath => databasePath;

    async Task<List<T>> QueryAsync<T>(Func<SqliteCommand, string> build, Func<DbDataReader, T> map)
    {
        await using var c = new SqliteConnection($"Data Source={databasePath};Mode=ReadOnly;Pooling=False");
        await c.OpenAsync();
        await using var cmd = c.CreateCommand();
        cmd.CommandText = build(cmd);
        await using var reader = await cmd.ExecuteReaderAsync();
        var list = new List<T>();
        while (await reader.ReadAsync())
        {
            list.Add(map(reader));
        }

        return list;
    }

    Task<List<T>> QueryAsync<T>(string sql, Func<DbDataReader, T> map) => QueryAsync(_ => sql, map);

    static string Str(DbDataReader r, int i) => r.IsDBNull(i) ? string.Empty : r.GetString(i);
    static int? NInt(DbDataReader r, int i) => r.IsDBNull(i) ? null : (int)r.GetInt64(i);
    static int Int(DbDataReader r, int i) => r.IsDBNull(i) ? 0 : (int)r.GetInt64(i);
    static double? NDouble(DbDataReader r, int i) => r.IsDBNull(i) ? null : r.GetDouble(i);

    /// <summary>Adds a list of values as numbered parameters and returns the SQL list (@name0, @name1, ...).</summary>
    static string ListParameters(SqliteCommand cmd, string name, IReadOnlyList<string> values)
    {
        var names = new List<string>();
        for (var i = 0; i < values.Count; i++)
        {
            cmd.Parameters.AddWithValue($"@{name}{i}", values[i]);
            names.Add($"@{name}{i}");
        }

        return string.Join(", ", names);
    }

    /// <summary>The conditions on documents (alias d) and, when asked, on results (alias r). The values go in as parameters.</summary>
    static string Where(Filter f, SqliteCommand cmd, bool includeResults = true)
    {
        var parts = new List<string> { "1 = 1" };
        void Eq(string column, string? value, string name)
        {
            if (!string.IsNullOrEmpty(value))
            {
                parts.Add($"{column} = @{name}");
                cmd.Parameters.AddWithValue("@" + name, value);
            }
        }

        Eq("d.corpus", f.Corpus, "corpus");
        Eq("d.doc_type", f.DocType, "doctype");
        Eq("d.format_group", f.FormatGroup, "format");
        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            parts.Add("d.doc_id LIKE @search ESCAPE '\\'");
            cmd.Parameters.AddWithValue("@search", "%" + f.Search.Trim().Replace("\\", "\\\\", StringComparison.Ordinal).Replace("%", "\\%", StringComparison.Ordinal).Replace("_", "\\_", StringComparison.Ordinal) + "%");
        }

        if (includeResults)
        {
            if (f.Variants is { Count: > 0 })
            {
                parts.Add($"r.variant IN ({ListParameters(cmd, "variant", f.Variants)})");
            }

            if (f.Configs is { Count: > 0 })
            {
                parts.Add($"r.config IN ({ListParameters(cmd, "config", f.Configs)})");
            }

            if (!f.IncludeBaselines)
            {
                parts.Add("r.model <> ''");
            }

            Eq("r.status", f.Status, "status");
            Eq("r.source", f.Source, "source");
        }

        return string.Join(" AND ", parts);
    }

    /// <summary>The setups that count as "the language models" for ratings and repeated misses: those named in the filter, or else every model on its own (no GLiNER, no baselines).
    /// Only results that succeeded count.</summary>
    static string Scope(Filter f, SqliteCommand cmd) => f.Configs is { Count: > 0 }
        ? $"r.status = 'ok' AND r.config IN ({ListParameters(cmd, "scope", f.Configs)})"
        : "r.status = 'ok' AND r.model <> '' AND r.variant = 'plain'";

    /// <summary>What the dataset says about itself.</summary>
    public async Task<DatasetInfo> InfoAsync()
    {
        var json = (await QueryAsync("SELECT value FROM meta WHERE key = 'run'", r => r.GetString(0))).Single();
        var run = JsonDocument.Parse(json).RootElement;
        string? Text(string name) => run.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
        var keys = run.TryGetProperty("corpora", out var corpora) ? string.Join(", ", corpora.EnumerateArray().Select(c => $"{c.GetProperty("corpus").GetString()}: {c.GetProperty("keyVersion").GetString()}")) : null;
        var counts = (await QueryAsync("SELECT (SELECT COUNT(*) FROM documents), (SELECT COUNT(*) FROM results)", r => (Int(r, 0), Int(r, 1)))).Single();
        return new DatasetInfo(Text("datasetId") ?? "?", Text("status") ?? "?", Text("createdAt") ?? string.Empty, Text("gitCommit"), keys, Text("ollamaVersion"), Text("machine"), Text("notes"), counts.Item1, counts.Item2);
    }

    /// <summary>The distinct values of a column, for the filter lists. Only a fixed set of columns is allowed.</summary>
    public Task<List<string>> ValuesAsync(string column)
    {
        var sql = column switch
        {
            "corpus" => "SELECT DISTINCT corpus FROM documents ORDER BY 1",
            "doc_type" => "SELECT DISTINCT doc_type FROM documents WHERE doc_type <> '' ORDER BY 1",
            "format_group" => "SELECT DISTINCT format_group FROM documents ORDER BY 1",
            "config" => "SELECT DISTINCT config FROM results ORDER BY model <> '', model, variant, config",
            "model" => "SELECT DISTINCT model FROM results WHERE model <> '' ORDER BY 1",
            "type" => "SELECT DISTINCT type FROM entities WHERE type <> '' ORDER BY 1",
            _ => throw new ArgumentException($"'{column}' is not a column the explorer lists."),
        };
        return QueryAsync(sql, r => Str(r, 0));
    }

    /// <summary>The totals for each detector setup over the documents that match the filter.</summary>
    public Task<List<ConfigSummary>> SummaryAsync(Filter f) => QueryAsync(
        cmd => "SELECT r.config, r.model, r.variant, SUM(r.status = 'ok'), SUM(r.status <> 'ok'), SUM(r.present), SUM(r.caught), SUM(r.edits), SUM(r.true_positives), SUM(r.unjudged), " +
            "SUM(r.preserve_total), SUM(r.preserve_broken), SUM(r.flags_raised), SUM(r.flags_correct), AVG(r.detect_seconds), AVG(r.gliner_seconds) " +
            $"FROM results r JOIN documents d ON d.doc_id = r.doc_id WHERE {Where(f, cmd)} GROUP BY r.config, r.model, r.variant ORDER BY r.model = '' DESC, r.model, r.variant",
        r => new ConfigSummary(Str(r, 0), Str(r, 1), Str(r, 2), Int(r, 3), Int(r, 4), Int(r, 5), Int(r, 6), Int(r, 7), Int(r, 8), Int(r, 9), Int(r, 10), Int(r, 11), Int(r, 12), Int(r, 13), NDouble(r, 14), NDouble(r, 15)));

    /// <summary>The columns the grid can be sorted by.</summary>
    public static readonly IReadOnlyDictionary<string, string> SortColumns = new Dictionary<string, string>
    {
        ["document"] = "d.doc_id", ["corpus"] = "d.corpus", ["format"] = "d.format_group", ["setup"] = "r.config", ["status"] = "r.status",
        ["recall"] = "(CASE WHEN r.present > 0 THEN 1.0 * r.caught / r.present END)", ["precision"] = "(CASE WHEN r.edits > 0 THEN 1.0 * r.true_positives / r.edits END)",
        ["missed"] = "(r.present - r.caught)", ["over"] = "(r.edits - r.true_positives)", ["flags"] = "r.flags_raised", ["time"] = "r.detect_seconds",
    };

    /// <summary>The results grid: one row per setup per document, sorted and paged.</summary>
    public async Task<Page<GridRow>> GridAsync(Filter f, string sort, bool descending, int page, int pageSize)
    {
        var order = SortColumns.TryGetValue(sort, out var column) ? column : SortColumns["document"];
        var total = (await QueryAsync(cmd => $"SELECT COUNT(*) FROM results r JOIN documents d ON d.doc_id = r.doc_id WHERE {Where(f, cmd)}", r => Int(r, 0))).Single();
        var rows = await QueryAsync(
            cmd => GridSelect + $"WHERE {Where(f, cmd)} ORDER BY {order} {(descending ? "DESC" : "ASC")}, r.result_id LIMIT {Math.Max(1, pageSize)} OFFSET {Math.Max(0, page) * Math.Max(1, pageSize)}",
            ReadGridRow);
        return new Page<GridRow>(rows, total);
    }

    /// <summary>Every grid row that matches the filter, in the grid's order, for exporting to CSV.</summary>
    public Task<List<GridRow>> GridAllAsync(Filter f, string sort, bool descending) => QueryAsync(
        cmd => GridSelect + $"WHERE {Where(f, cmd)} ORDER BY {(SortColumns.TryGetValue(sort, out var column) ? column : SortColumns["document"])} {(descending ? "DESC" : "ASC")}, r.result_id", ReadGridRow);

    const string GridSelect = "SELECT r.doc_id, d.corpus, d.format_group, d.doc_type, r.config, r.variant, r.status, r.error, r.present, r.caught, r.edits, r.true_positives, r.unjudged, " +
        "r.flags_raised, r.flags_correct, r.preserve_broken, r.detect_seconds, r.source, r.model FROM results r JOIN documents d ON d.doc_id = r.doc_id ";

    static GridRow ReadGridRow(DbDataReader r) => new(Str(r, 0), Str(r, 1), Str(r, 2), Str(r, 3), Str(r, 4), Str(r, 5), Str(r, 6), r.IsDBNull(7) ? null : Str(r, 7), NInt(r, 8), NInt(r, 9),
        NInt(r, 10), NInt(r, 11), NInt(r, 12), NInt(r, 13), NInt(r, 14), NInt(r, 15), NDouble(r, 16), Str(r, 17), Str(r, 18));

    /// <summary>Every document that matches the filter with its difficulty figures over the language models.</summary>
    public Task<List<DocumentSummary>> DocumentsAsync(Filter f) => QueryAsync(
        cmd =>
        {
            var scope = Scope(f, cmd);
            var where = Where(f with { Variants = null, Configs = null, Status = null, Source = null }, cmd, includeResults: false);
            return "SELECT d.doc_id, d.corpus, d.format_group, d.doc_type, d.chars, d.occurrence_count, COUNT(r.result_id), " +
                "AVG(CASE WHEN r.present > 0 THEN 1.0 * r.caught / r.present END), SUM(r.present > r.caught), AVG(r.present - r.caught), AVG(r.edits - r.true_positives), SUM(r.preserve_broken), " +
                "COALESCE(m.missed_by_all, 0) FROM documents d LEFT JOIN results r ON r.doc_id = d.doc_id AND " + scope + " " +
                "LEFT JOIN (SELECT x.doc_id AS doc_id, COUNT(*) AS missed_by_all FROM (SELECT o.doc_id AS doc_id, o.entity_id, o.start, COUNT(DISTINCT o.result_id) AS n " +
                "FROM outcomes o JOIN results r ON r.result_id = o.result_id WHERE o.kind = 'missed' AND " + scope + " GROUP BY o.doc_id, o.entity_id, o.start) x " +
                "JOIN (SELECT r.doc_id AS doc_id, COUNT(*) AS models FROM results r WHERE " + scope + " GROUP BY r.doc_id) t ON t.doc_id = x.doc_id WHERE x.n = t.models GROUP BY x.doc_id) m ON m.doc_id = d.doc_id " +
                $"WHERE {where} GROUP BY d.doc_id ORDER BY d.doc_id";
        },
        r => new DocumentSummary(Str(r, 0), Str(r, 1), Str(r, 2), Str(r, 3), Int(r, 4), Int(r, 5), Int(r, 6), NDouble(r, 7), Int(r, 8), NDouble(r, 9) ?? 0, NDouble(r, 10) ?? 0, Int(r, 11), Int(r, 12)));

    /// <summary>One document: its text, how every setup did, and the answer key. Null if there is no such document.</summary>
    public async Task<DocumentDetail?> DocumentAsync(string docId)
    {
        var doc = (await QueryAsync(cmd =>
        {
            cmd.Parameters.AddWithValue("@d", docId);
            return "SELECT d.key_id, d.corpus, d.format_group, d.doc_type, t.text FROM documents d JOIN texts t ON t.doc_id = d.doc_id WHERE d.doc_id = @d";
        }, r => (Key: Str(r, 0), Corpus: Str(r, 1), Format: Str(r, 2), Type: Str(r, 3), Text: Str(r, 4)))).FirstOrDefault();
        if (doc.Key is null)
        {
            return null;
        }

        var results = await QueryAsync(cmd =>
        {
            cmd.Parameters.AddWithValue("@d", docId);
            return GridSelect + "WHERE r.doc_id = @d ORDER BY r.model <> '', r.model, r.variant, r.config";
        }, ReadGridRow);
        var key = await QueryAsync(cmd =>
        {
            cmd.Parameters.AddWithValue("@k", doc.Key);
            return "SELECT role, type, text, occurrences, origin FROM entities WHERE key_id = @k ORDER BY role, type, text";
        }, r => new KeyItem(Str(r, 0), Str(r, 1), Str(r, 2), NInt(r, 3), Str(r, 4)));
        return new DocumentDetail(docId, doc.Key, doc.Corpus, doc.Format, doc.Type, doc.Text, results, key);
    }

    /// <summary>What one setup did on one document: its redactions and flags and the judged facts, as positions in the document's text.</summary>
    public async Task<ConfigView> ConfigViewAsync(string docId, string config)
    {
        var facts = await QueryAsync(cmd =>
        {
            cmd.Parameters.AddWithValue("@d", docId);
            cmd.Parameters.AddWithValue("@c", config);
            return "SELECT o.kind, o.type, o.text, o.start, o.length FROM outcomes o JOIN results r ON r.result_id = o.result_id WHERE r.doc_id = @d AND r.config = @c AND o.start IS NOT NULL AND o.kind <> 'key_extra' ORDER BY o.start";
        }, r => new Marker(Str(r, 0), Str(r, 1), Str(r, 2), Int(r, 3), Int(r, 4), null, null));
        var spans = await QueryAsync(cmd =>
        {
            cmd.Parameters.AddWithValue("@d", docId);
            cmd.Parameters.AddWithValue("@c", config);
            return "SELECT s.flag, s.type, s.start, s.length, s.source, s.confidence FROM spans s JOIN results r ON r.result_id = s.result_id WHERE r.doc_id = @d AND r.config = @c ORDER BY s.start";
        }, r => new Marker(r.GetInt64(0) == 1 ? "flagged" : "redacted", Str(r, 1), string.Empty, Int(r, 2), Int(r, 3), Str(r, 4), NDouble(r, 5)));
        return new ConfigView(config, [.. facts, .. spans]);
    }

    /// <summary>Recall and over-redaction for each setup and category, over the documents that match the filter.</summary>
    public Task<List<CategoryCell>> CategoriesAsync(Filter f) => QueryAsync(
        cmd => "SELECT r.config, o.type, SUM(o.kind = 'caught'), SUM(o.kind = 'missed'), SUM(o.kind = 'over_redaction') FROM outcomes o JOIN results r ON r.result_id = o.result_id " +
            $"JOIN documents d ON d.doc_id = r.doc_id WHERE o.type <> '' AND o.kind IN ('caught', 'missed', 'over_redaction') AND r.status = 'ok' AND {Where(f, cmd)} GROUP BY r.config, o.type",
        r => new CategoryCell(Str(r, 0), Str(r, 1), Int(r, 2), Int(r, 3), Int(r, 4)));

    /// <summary>The places behind a number: the key items missed (or the redactions that were wrong) for one setup and category, with the words around them.</summary>
    public async Task<Page<OutcomeItem>> OutcomeItemsAsync(Filter f, string config, string type, string kind, int limit, int offset)
    {
        var scope = f with { Configs = [config], Variants = null, Status = "ok" };
        var total = (await QueryAsync(cmd =>
        {
            cmd.Parameters.AddWithValue("@kind", kind);
            cmd.Parameters.AddWithValue("@type", type);
            return $"SELECT COUNT(*) FROM outcomes o JOIN results r ON r.result_id = o.result_id JOIN documents d ON d.doc_id = r.doc_id WHERE o.kind = @kind AND o.type = @type AND {Where(scope, cmd)}";
        }, r => Int(r, 0))).Single();
        var rows = await QueryAsync(cmd =>
        {
            cmd.Parameters.AddWithValue("@kind", kind);
            cmd.Parameters.AddWithValue("@type", type);
            return "SELECT o.doc_id, r.config, o.kind, o.type, o.text, o.start, o.length FROM outcomes o JOIN results r ON r.result_id = o.result_id JOIN documents d ON d.doc_id = r.doc_id " +
                $"WHERE o.kind = @kind AND o.type = @type AND {Where(scope, cmd)} ORDER BY o.doc_id, o.start LIMIT {Math.Max(1, limit)} OFFSET {Math.Max(0, offset)}";
        }, r => (DocId: Str(r, 0), Config: Str(r, 1), Kind: Str(r, 2), Type: Str(r, 3), Text: Str(r, 4), Start: NInt(r, 5), Length: NInt(r, 6)));
        var texts = await TextsAsync(rows.Select(x => x.DocId).Distinct().ToList());
        var items = rows.Select(x =>
        {
            var (before, after) = Context(texts.GetValueOrDefault(x.DocId), x.Start, x.Length);
            return new OutcomeItem(x.DocId, x.Config, x.Kind, x.Type, x.Text, before, after);
        }).ToList();
        return new Page<OutcomeItem>(items, total);
    }

    /// <summary>The words either side of a place in a text (blank when the place is not known).</summary>
    public static (string Before, string After) Context(string? text, int? start, int? length)
    {
        if (text is null || start is null || length is null || start < 0 || start + length > text.Length)
        {
            return (string.Empty, string.Empty);
        }

        var from = Math.Max(0, start.Value - ContextChars);
        var to = Math.Min(text.Length, start.Value + length.Value + ContextChars);
        return (text[from..start.Value].ReplaceLineEndings(" "), text[(start.Value + length.Value)..to].ReplaceLineEndings(" "));
    }

    async Task<Dictionary<string, string>> TextsAsync(IReadOnlyList<string> docIds)
    {
        if (docIds.Count == 0)
        {
            return [];
        }

        var list = await QueryAsync(cmd => $"SELECT doc_id, text FROM texts WHERE doc_id IN ({ListParameters(cmd, "t", docIds)})", r => (Str(r, 0), Str(r, 1)));
        return list.ToDictionary(x => x.Item1, x => x.Item2);
    }

    /// <summary>The same item missed (or wrongly redacted) again and again: ordered by how many setups it affected. <paramref name="kind"/> is missed or over_redaction.</summary>
    public Task<List<RepeatedItem>> RepeatedAsync(Filter f, string kind, int limit) => QueryAsync(
        cmd =>
        {
            cmd.Parameters.AddWithValue("@kind", kind);
            var scope = Scope(f, cmd);
            var where = Where(f with { Variants = null, Configs = null, Status = null, Source = null }, cmd, includeResults: false);
            return "SELECT o.type, o.text, COUNT(DISTINCT r.config), COUNT(DISTINCT o.doc_id), COUNT(*) FROM outcomes o JOIN results r ON r.result_id = o.result_id JOIN documents d ON d.doc_id = o.doc_id " +
                $"WHERE o.kind = @kind AND {scope} AND {where} GROUP BY o.type, o.text ORDER BY 3 DESC, 5 DESC, o.text LIMIT {Math.Max(1, limit)}";
        },
        r => new RepeatedItem(Str(r, 0), Str(r, 1), Int(r, 2), Int(r, 3), Int(r, 4)));
}
