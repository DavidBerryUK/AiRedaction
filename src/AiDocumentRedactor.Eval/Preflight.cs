using AiDocumentRedactor.Core;
using AiDocumentRedactor.Explorer;
using AiDocumentRedactor.Explorer.Dataset;

namespace AiDocumentRedactor.Eval;

/// <summary>Checks the final evaluation end to end on a handful of documents before the long run: one of each file type, a held-out document and one whose key is empty, with the
/// fastest model. It checks that everything is saved and complete, that the explorer can read it, that stopping part-way and resuming gives the same result, and that the scores
/// agree with the earlier saved run of the same model. Nothing here is part of the real dataset.</summary>
public static class Preflight
{
    sealed record Check(string Name, bool Passed, string Detail);

    /// <summary>Runs the checks and prints a pass or fail for each. Returns 0 only if every check passed.</summary>
    public static async Task<int> RunAsync(RedactorOptions options, string inputRoot, string corpusRoot, string datasetsDir, string[]? models, int timeoutSeconds, bool allowDirty)
    {
        var checks = new List<Check>();
        var model = models?.FirstOrDefault() ?? (options.Evaluation.Models.Any(m => m.Include && m.Name == "gemma4:e4b") ? "gemma4:e4b" : options.Evaluation.Models.First(m => m.Include).Name);
        var docs = ChooseDocuments(corpusRoot, inputRoot);
        Console.WriteLine($"Preflight: {docs.Count} documents ({string.Join(", ", docs.Select(d => d.Label))}) with {model}.\n");
        var dir = Path.Combine(datasetsDir, ".preflight");
        if (Directory.Exists(dir))
        {
            Directory.Delete(dir, true);   // the previous preflight's own folder
        }

        var only = docs.Select(d => d.DocId).ToList();
        FinalOptions Options() => new(options, inputRoot, corpusRoot, dir, "preflight", [model], only, allowDirty, true, timeoutSeconds);

        // 1. The run itself, from nothing.
        var code = await FinalEvaluator.RunAsync(Options());
        checks.Add(new Check("The evaluation runs and its own completeness check passes", code == 0, code == 0 ? "exit 0" : $"exit {code} (see above)"));
        if (code != 0)
        {
            return Report(checks, keep: dir);
        }

        // 2. What was saved.
        var runPath = Path.Combine(dir, Schema.RunFile);
        var run = System.Text.Json.JsonDocument.Parse(await File.ReadAllTextAsync(runPath)).RootElement;
        var (_, results) = await Csv.ReadAsync(Path.Combine(dir, Schema.ResultsFile));
        var (_, documents) = await Csv.ReadAsync(Path.Combine(dir, Schema.DocumentsFile));
        var spans = (await Csv.ReadAsync(Path.Combine(dir, Schema.SpansFile))).Rows;
        var outcomes = (await Csv.ReadAsync(Path.Combine(dir, Schema.OutcomesFile))).Rows;
        var expected = docs.Count * (InterimConverter.Baselines.Length + FinalEvaluator.ConfigsFor(model).Length);
        checks.Add(new Check("Every document has a result for every setup", results.Count == expected, $"{results.Count} results, expected {expected} ({docs.Count} documents × {InterimConverter.Baselines.Length + 4} setups)"));
        checks.Add(new Check("Every result succeeded", results.All(r => r["status"] == "ok"), $"{results.Count(r => r["status"] != "ok")} failed"));
        checks.Add(new Check("Every document, its text and its answer key are in the dataset", documents.Count == docs.Count && File.ReadAllLines(Path.Combine(dir, Schema.TextFile)).Length == docs.Count, $"{documents.Count} documents, {File.ReadAllLines(Path.Combine(dir, Schema.TextFile)).Length} texts"));
        checks.Add(new Check("Spans and item-by-item outcomes were saved", spans.Count > 0 && outcomes.Count > 0, $"{spans.Count} spans, {outcomes.Count} outcomes"));
        var modelRows = results.Where(r => r["model"] == model && r["variant"] == "plain").ToList();
        checks.Add(new Check("Each redacted file was written and passed its safety check", modelRows.Count == docs.Count && modelRows.All(r => r["output_ok"] == "true"),
            $"{modelRows.Count(r => r["output_ok"] == "true")} of {modelRows.Count} passed; {string.Join(", ", modelRows.Where(r => r["output_ok"] != "true").Select(r => r["doc_id"]))}"));
        checks.Add(new Check("Timings and token counts were saved", modelRows.All(r => r["detect_seconds"].Length > 0 && r["prompt_tokens"].Length > 0 && r["output_tokens"].Length > 0) && results.Where(r => r["variant"] != "plain").All(r => r["gliner_seconds"].Length > 0),
            $"model rows: {modelRows.Count(r => r["detect_seconds"].Length > 0)} timed"));
        checks.Add(new Check("The run is tied to the code and the environment", run.GetProperty("status").GetString() == "final" && run.GetProperty("gitCommit").GetString()!.Length > 0 && run.GetProperty("gitDirty").ValueKind == System.Text.Json.JsonValueKind.False
            && run.TryGetProperty("ollamaVersion", out var ov) && ov.ValueKind == System.Text.Json.JsonValueKind.String && run.TryGetProperty("machineBusy", out _),
            $"status {run.GetProperty("status")}, code {run.GetProperty("gitCommit")}, dirty {run.GetProperty("gitDirty")}, busy {(run.TryGetProperty("machineBusy", out var mb) ? mb : default)}"));
        checks.Add(new Check("The answer-key version and the timeout are recorded", run.GetProperty("corpora").EnumerateArray().All(c => c.TryGetProperty("keyChecksum", out var k) && k.GetString()!.Length == 64)
            && run.GetProperty("sources")[0].GetProperty("settings").GetProperty("llm").GetProperty("timeoutSeconds").GetInt32() >= timeoutSeconds, $"timeout {run.GetProperty("sources")[0].GetProperty("settings").GetProperty("llm").GetProperty("timeoutSeconds")} s"));

        // 3. The explorer can read it.
        try
        {
            var service = new ExplorerService(Path.Combine(dir, ResultsDatabase.DatabaseFile));
            var summary = await service.SummaryAsync(new Filter());
            var first = await service.DocumentAsync(docs[0].DocId);
            var view = await service.ConfigViewAsync(docs[0].DocId, model);
            var combine = await service.CombineAsync(new Filter(), new CombineChoice([model], true, 1, false, false));
            checks.Add(new Check("The results explorer can open it", summary.Count == InterimConverter.Baselines.Length + 4 && first is not null && first.Text.Length > 0 && combine.Count == 2,
                $"{summary.Count} setups, document text {first?.Text.Length} characters, {view.Markers.Count} markers"));
        }
        catch (Exception ex)
        {
            checks.Add(new Check("The results explorer can open it", false, ex.Message));
        }

        // 4. Running again changes nothing.
        var before = (results.Count, spans.Count, outcomes.Count);
        var again = await FinalEvaluator.RunAsync(Options());
        var after = await CountsAsync(dir);
        checks.Add(new Check("Running it again finds nothing left to do and changes nothing", again == 0 && before == after, $"{before} then {after}"));

        // 5. Stopping part-way: drop the last group's result rows (leaving its spans and outcomes behind, as a crash would), then resume.
        var resultsPath = Path.Combine(dir, Schema.ResultsFile);
        var lines = (await File.ReadAllLinesAsync(resultsPath)).ToList();
        await File.WriteAllLinesAsync(resultsPath, lines.Take(lines.Count - 4));
        var resumed = await FinalEvaluator.RunAsync(Options());
        var final = await CountsAsync(dir);
        checks.Add(new Check("A run that stopped part-way resumes and ends up the same", resumed == 0 && before == final, $"{before} originally, {final} after the simulated stop and resume"));

        // 6. The same scores as the earlier saved run of this model on the same documents.
        var earlier = Directory.GetDirectories(datasetsDir, "interim-*").OrderDescending().FirstOrDefault();
        if (earlier is not null)
        {
            var (_, old) = await Csv.ReadAsync(Path.Combine(earlier, Schema.ResultsFile));
            var oldById = old.Where(r => r["status"] == "ok" && r["source"] == "batch").ToDictionary(r => r["result_id"]);
            var (_, now) = await Csv.ReadAsync(resultsPath);
            var compared = 0;
            var different = new List<string>();
            foreach (var r in now)
            {
                if (!oldById.TryGetValue(r["result_id"], out var o))
                {
                    continue;
                }

                compared++;
                if (r["present"] != o["present"] || r["caught"] != o["caught"] || r["edits"] != o["edits"] || r["true_positives"] != o["true_positives"])
                {
                    different.Add($"{r["result_id"]}: caught {r["caught"]}/{r["present"]} edits {r["edits"]} (was {o["caught"]}/{o["present"]} edits {o["edits"]})");
                }
            }

            checks.Add(new Check($"The scores agree with the earlier run ({Path.GetFileName(earlier)})", compared > 0 && different.Count * 5 <= compared,
                $"{compared - different.Count} of {compared} setups scored identically{(different.Count > 0 ? $"; differences (a model is not always exactly repeatable): {string.Join(" | ", different.Take(4))}" : string.Empty)}"));
        }

        return Report(checks, keep: checks.All(c => c.Passed) ? null : dir, cleanup: dir);
    }

    static async Task<(int, int, int)> CountsAsync(string dir) =>
        ((await Csv.ReadAsync(Path.Combine(dir, Schema.ResultsFile))).Rows.Count, (await Csv.ReadAsync(Path.Combine(dir, Schema.SpansFile))).Rows.Count, (await Csv.ReadAsync(Path.Combine(dir, Schema.OutcomesFile))).Rows.Count);

    static int Report(List<Check> checks, string? keep, string? cleanup = null)
    {
        Console.WriteLine("\n=== Preflight result ===");
        foreach (var c in checks)
        {
            Console.WriteLine($"  {(c.Passed ? "PASS" : "FAIL")}  {c.Name}\n        {c.Detail}");
        }

        var ok = checks.All(c => c.Passed);
        Console.WriteLine(ok ? "\nEverything passed. It is safe to start the final run." : $"\nSomething failed. The trial dataset is kept for inspection: {keep}");
        if (ok && cleanup is not null && Directory.Exists(cleanup))
        {
            Directory.Delete(cleanup, true);   // the trial's own folder
        }

        return ok ? 0 : 1;
    }

    sealed record Pick(string DocId, string Label);

    /// <summary>One document of each file type from the formats corpus, plus the first held-out document and the first held-out document whose answer key is empty.</summary>
    static List<Pick> ChooseDocuments(string corpusRoot, string inputRoot)
    {
        var picks = new List<Pick>();
        foreach (var dir in Directory.GetDirectories(corpusRoot).Where(d => Directory.Exists(Path.Combine(d, "ground-truth"))).Order(StringComparer.Ordinal))
        {
            var corpus = InterimConverter.CorpusName(dir);
            var truth = GroundTruthStore.Load(dir);
            var seenFormats = new HashSet<string>();
            var heldFirst = false;
            var heldEmpty = false;
            foreach (var file in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
            {
                var rel = Path.GetRelativePath(dir, file).Replace('\\', '/');
                if (rel.StartsWith("ground-truth", StringComparison.Ordinal) || rel is "README.md" or "AUDIT.md" or "manifest.csv")
                {
                    continue;
                }

                var key = GroundTruthStore.For(Path.GetFileName(file), truth);
                var docId = InterimConverter.DocId(corpus, rel);
                if (key is null || !File.Exists(Path.Combine(inputRoot, docId)))
                {
                    continue;
                }

                if (corpus == "heldout")
                {
                    if (!heldFirst)
                    {
                        heldFirst = true;
                        picks.Add(new Pick(docId, "held-out"));
                    }
                    else if (!heldEmpty && key.Entities.Count == 0)
                    {
                        heldEmpty = true;
                        picks.Add(new Pick(docId, "held-out, empty key"));
                    }
                }
                else if (seenFormats.Add(GroundTruthStore.FormatGroup(file)))
                {
                    picks.Add(new Pick(docId, GroundTruthStore.FormatGroup(file)));
                }
            }
        }

        return picks;
    }
}
