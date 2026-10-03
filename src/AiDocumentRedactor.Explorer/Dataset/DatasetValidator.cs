using System.Globalization;
using System.Text.Json;

namespace AiDocumentRedactor.Explorer.Dataset;

/// <summary>Checks a dataset folder against the format: files and columns present, values of the right type, every id pointing at something, spans inside the text,
/// text hashes matching, and a result for every document under every detector setup of its corpus. Returns the problems found (none means valid).</summary>
public static class DatasetValidator
{
    const int MaxProblems = 50;

    /// <summary>Validates the dataset in <paramref name="dir"/>.</summary>
    public static async Task<List<string>> ValidateAsync(string dir)
    {
        var problems = new List<string>();
        void Problem(string message)
        {
            if (problems.Count < MaxProblems)
            {
                problems.Add(message);
            }
        }

        var tables = new Dictionary<string, List<Dictionary<string, string>>>();
        foreach (var (file, columns) in Schema.Files)
        {
            var path = Path.Combine(dir, file);
            if (!File.Exists(path))
            {
                Problem($"{file} is missing.");
                continue;
            }

            try
            {
                var (found, rows) = await Csv.ReadAsync(path);
                foreach (var missing in columns.Where(c => c.Required && !found.Contains(c.Name)))
                {
                    Problem($"{file}: required column '{missing.Name}' is missing.");
                }

                CheckCells(file, columns, rows, Problem);
                tables[file] = rows;
            }
            catch (InvalidDataException ex)
            {
                Problem($"{file}: {ex.Message}");
            }
        }

        JsonElement run = default;
        var runPath = Path.Combine(dir, Schema.RunFile);
        if (!File.Exists(runPath))
        {
            Problem($"{Schema.RunFile} is missing.");
        }
        else
        {
            try
            {
                run = JsonDocument.Parse(await File.ReadAllTextAsync(runPath)).RootElement.Clone();
                foreach (var field in new[] { "formatVersion", "datasetId", "status", "createdAt", "models", "corpora" })
                {
                    if (!run.TryGetProperty(field, out _))
                    {
                        Problem($"{Schema.RunFile}: required field '{field}' is missing.");
                    }
                }

                if (run.TryGetProperty("status", out var status) && status.GetString() == "final"
                    && (!run.TryGetProperty("gitDirty", out var dirty) || dirty.ValueKind != JsonValueKind.False))
                {
                    Problem("A final dataset must record gitDirty: false.");
                }
            }
            catch (JsonException ex)
            {
                Problem($"{Schema.RunFile}: {ex.Message}");
            }
        }

        var lengths = new Dictionary<string, int>();
        var hashes = new Dictionary<string, string>();
        var textPath = Path.Combine(dir, Schema.TextFile);
        if (!File.Exists(textPath))
        {
            Problem($"{Schema.TextFile} is missing.");
        }
        else
        {
            var lineNumber = 0;
            foreach (var line in await File.ReadAllLinesAsync(textPath))
            {
                lineNumber++;
                if (line.Length == 0)
                {
                    continue;
                }

                try
                {
                    var o = JsonDocument.Parse(line).RootElement;
                    var id = o.GetProperty("doc_id").GetString() ?? string.Empty;
                    var text = o.GetProperty("text").GetString() ?? string.Empty;
                    lengths[id] = text.Length;
                    hashes[id] = o.GetProperty("text_hash").GetString() ?? string.Empty;
                    if (DatasetWriter.HashText(text) != hashes[id])
                    {
                        Problem($"{Schema.TextFile} line {lineNumber}: the text of '{id}' does not match its text_hash.");
                    }
                }
                catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException)
                {
                    Problem($"{Schema.TextFile} line {lineNumber}: {ex.Message}");
                }
            }
        }

        if (tables.Count == Schema.Files.Count)
        {
            CheckReferences(tables, lengths, hashes, Problem);
        }

        return problems;
    }

    static void CheckCells(string file, Column[] columns, List<Dictionary<string, string>> rows, Action<string> problem)
    {
        var line = 1;
        foreach (var row in rows)
        {
            line++;
            foreach (var c in columns)
            {
                if (!row.TryGetValue(c.Name, out var v))
                {
                    continue;
                }

                if (v.Length == 0)
                {
                    if (c.Required)
                    {
                        problem($"{file} row {line}: required column '{c.Name}' is empty.");
                    }

                    continue;
                }

                var ok = c.Kind switch
                {
                    ColumnKind.Int => long.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out _),
                    ColumnKind.Number => double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out _),
                    ColumnKind.Bool => v is "true" or "false",
                    _ => true,
                };
                if (!ok)
                {
                    problem($"{file} row {line}: '{v}' in '{c.Name}' is not a valid {c.Kind.ToString().ToLowerInvariant()}.");
                }
            }
        }
    }

    static void CheckReferences(Dictionary<string, List<Dictionary<string, string>>> t, Dictionary<string, int> lengths, Dictionary<string, string> hashes, Action<string> problem)
    {
        var docs = t[Schema.DocumentsFile];
        var docIds = docs.Select(d => d["doc_id"]).ToHashSet();
        if (docIds.Count != docs.Count)
        {
            problem($"{Schema.DocumentsFile}: doc_id values are not unique.");
        }

        foreach (var d in docs)
        {
            if (!hashes.TryGetValue(d["doc_id"], out var h))
            {
                problem($"'{d["doc_id"]}' has no line in {Schema.TextFile}.");
            }
            else if (h != d["text_hash"])
            {
                problem($"'{d["doc_id"]}': text_hash in {Schema.DocumentsFile} differs from {Schema.TextFile}.");
            }
        }

        var keyIds = t[Schema.EntitiesFile].Select(e => e["key_id"]).ToHashSet();
        foreach (var d in docs.Where(d => !keyIds.Contains(d["key_id"]) && d["entity_count"] != "0"))
        {
            problem($"'{d["doc_id"]}' names answer key '{d["key_id"]}', which has no rows in {Schema.EntitiesFile} (and its entity_count is not 0).");
        }

        var entityIds = t[Schema.EntitiesFile].Select(e => e["entity_id"]).ToHashSet();
        foreach (var e in t[Schema.EntitiesFile].Where(e => !Schema.Roles.Contains(e["role"])))
        {
            problem($"{Schema.EntitiesFile}: '{e["role"]}' is not a role.");
        }

        var results = t[Schema.ResultsFile];
        var resultDoc = new Dictionary<string, string>();
        foreach (var r in results)
        {
            if (!resultDoc.TryAdd(r["result_id"], r["doc_id"]))
            {
                problem($"{Schema.ResultsFile}: result_id '{r["result_id"]}' is repeated.");
            }

            if (!docIds.Contains(r["doc_id"]))
            {
                problem($"{Schema.ResultsFile}: '{r["result_id"]}' names unknown document '{r["doc_id"]}'.");
            }

            if (!Schema.Variants.Contains(r["variant"]))
            {
                problem($"{Schema.ResultsFile}: '{r["variant"]}' is not a variant.");
            }

            if (!Schema.Statuses.Contains(r["status"]))
            {
                problem($"{Schema.ResultsFile}: '{r["status"]}' is not a status.");
            }

            if (int.TryParse(r["caught"], out var caught) && int.TryParse(r["present"], out var present) && caught > present)
            {
                problem($"{Schema.ResultsFile}: '{r["result_id"]}' caught more than was present.");
            }
        }

        // Every detector setup of a corpus must have a result (or a failed row saying why) for every document of that corpus.
        var corpusOf = docs.ToDictionary(d => d["doc_id"], d => d["corpus"]);
        var docsPerCorpus = docs.GroupBy(d => d["corpus"]).ToDictionary(g => g.Key, g => g.Select(d => d["doc_id"]).ToHashSet());
        foreach (var group in results.Where(r => corpusOf.ContainsKey(r["doc_id"])).GroupBy(r => (Corpus: corpusOf[r["doc_id"]], Config: r["config"], Repeat: r["repeat"])))
        {
            var have = group.Select(r => r["doc_id"]).ToHashSet();
            var missing = docsPerCorpus[group.Key.Corpus].Count(d => !have.Contains(d));
            if (missing > 0)
            {
                problem($"'{group.Key.Config}' is missing {missing} of {docsPerCorpus[group.Key.Corpus].Count} documents of corpus '{group.Key.Corpus}'.");
            }
        }

        var spanLength = new Dictionary<string, int>();
        foreach (var s in t[Schema.SpansFile])
        {
            if (!resultDoc.TryGetValue(s["result_id"], out var doc))
            {
                problem($"{Schema.SpansFile}: '{s["span_id"]}' names unknown result '{s["result_id"]}'.");
                continue;
            }

            if (!spanLength.TryAdd(s["span_id"], 0))
            {
                problem($"{Schema.SpansFile}: span_id '{s["span_id"]}' is repeated.");
            }

            if (lengths.TryGetValue(doc, out var chars) && int.TryParse(s["start"], out var start) && int.TryParse(s["length"], out var len) && (start < 0 || len <= 0 || start + len > chars))
            {
                problem($"{Schema.SpansFile}: '{s["span_id"]}' lies outside the text of '{doc}'.");
            }
        }

        foreach (var o in t[Schema.OutcomesFile])
        {
            if (!resultDoc.TryGetValue(o["result_id"], out var doc))
            {
                problem($"{Schema.OutcomesFile}: '{o["outcome_id"]}' names unknown result '{o["result_id"]}'.");
                continue;
            }

            if (doc != o["doc_id"])
            {
                problem($"{Schema.OutcomesFile}: '{o["outcome_id"]}' names document '{o["doc_id"]}' but its result belongs to '{doc}'.");
            }

            if (!Schema.OutcomeKinds.Contains(o["kind"]))
            {
                problem($"{Schema.OutcomesFile}: '{o["kind"]}' is not an outcome kind.");
            }

            if (o["entity_id"].Length > 0 && !entityIds.Contains(o["entity_id"]))
            {
                problem($"{Schema.OutcomesFile}: '{o["outcome_id"]}' names unknown entity '{o["entity_id"]}'.");
            }

            if (o["span_id"].Length > 0 && !spanLength.ContainsKey(o["span_id"]))
            {
                problem($"{Schema.OutcomesFile}: '{o["outcome_id"]}' names unknown span '{o["span_id"]}'.");
            }
        }
    }
}
