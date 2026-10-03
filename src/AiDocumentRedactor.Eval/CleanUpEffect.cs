using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Text.Json;
using AiDocumentRedactor.Core;
using AiDocumentRedactor.Detection;
using AiDocumentRedactor.Explorer.Dataset;

namespace AiDocumentRedactor.Eval;

/// <summary>Measures what the clean-up rules would do, without running any model: it takes each model's saved spans from a dataset, applies the rules, and scores the result
/// against the answer key again. Each rule is tried alone and then all together. It also lists the right redactions a rule would lose, which is the cost to check.
/// Read the caveat in the report: the rules were chosen after looking at these same documents, so the gain is an upper estimate until they are tried on documents they were not based on.</summary>
public static class CleanUpEffect
{
    sealed record Scenario(string Name, CleanUpOptions Options);

    sealed class Totals
    {
        public int Present, Caught, Edits, TruePositives, Documents;
        public double Recall => Present == 0 ? 0 : (double)Caught / Present;
        public double Precision => Edits == 0 ? 0 : (double)TruePositives / Edits;
        public double F1 => Recall + Precision == 0 ? 0 : 2 * Recall * Precision / (Recall + Precision);
    }

    sealed record Lost(string Rule, string Model, string DocId, string Type, string Text);

    static string P(double v) => (v * 100).ToString("0.0", CultureInfo.InvariantCulture) + "%";

    /// <summary>Runs the simulation on a dataset folder and writes a Markdown report. Returns the exit code.</summary>
    public static async Task<int> RunAsync(string datasetDir, string corpusRoot, string outPath)
    {
        var (_, docRows) = await Csv.ReadAsync(Path.Combine(datasetDir, Schema.DocumentsFile));
        var texts = new Dictionary<string, string>();
        foreach (var line in await File.ReadAllLinesAsync(Path.Combine(datasetDir, Schema.TextFile)))
        {
            if (line.Length > 0)
            {
                var o = JsonDocument.Parse(line).RootElement;
                texts[o.GetProperty("doc_id").GetString()!] = o.GetProperty("text").GetString()!;
            }
        }

        var (_, results) = await Csv.ReadAsync(Path.Combine(datasetDir, Schema.ResultsFile));
        var models = results.Where(r => r["status"] == "ok" && r["variant"] == "plain" && r["model"].Length > 0 && r["source"] == "batch").ToList();
        var spansBy = (await Csv.ReadAsync(Path.Combine(datasetDir, Schema.SpansFile))).Rows.GroupBy(r => r["result_id"])
            .ToDictionary(g => g.Key, g => g.Select(r => new DetectedEntity(r["type"], int.Parse(r["start"], CultureInfo.InvariantCulture), int.Parse(r["length"], CultureInfo.InvariantCulture),
                double.TryParse(r["confidence"], NumberStyles.Float, CultureInfo.InvariantCulture, out var c) ? c : 1, r["source"], r["flag"] == "true")).ToList());

        var keys = new Dictionary<string, GroundTruth>();
        foreach (var dir in Directory.GetDirectories(corpusRoot).Where(d => Directory.Exists(Path.Combine(d, "ground-truth"))))
        {
            foreach (var g in GroundTruthStore.Load(dir))
            {
                keys[g.Id] = g;
            }
        }

        var docInfo = docRows.ToDictionary(r => r["doc_id"], r => (Key: r["key_id"], Corpus: r["corpus"], Format: r["format_group"]));
        var scenarios = new List<Scenario>
        {
            new("no rules (as run)", new CleanUpOptions()),
            new("placeholders in brackets", new CleanUpOptions { BracketedPlaceholders = true }),
            new("masked values", new CleanUpOptions { MaskedValues = true }),
            new("generic terms", new CleanUpOptions { GenericTerms = true }),
            new("dates need birth wording", new CleanUpOptions { BirthDateContext = true }),
            new("IPv6 addresses (adds)", new CleanUpOptions { Ipv6 = true }),
            new("all rules together", new CleanUpOptions { BracketedPlaceholders = true, MaskedValues = true, GenericTerms = true, BirthDateContext = true, Ipv6 = true }),
        };
        var totals = new ConcurrentDictionary<(string Corpus, string Model, string Scenario), Totals>();
        var lost = new ConcurrentBag<Lost>();
        var suppressedCount = new ConcurrentDictionary<(string Rule, bool Right), int>();
        var stored = new ConcurrentDictionary<(string Corpus, string Model), (int Present, int Caught, int Edits, int Tp)>();
        var template = new RedactorOptions().Redaction.PlaceholderTemplate;
        var done = 0;
        Parallel.ForEach(models, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, r =>
        {
            var docId = r["doc_id"];
            var info = docInfo[docId];
            var gt = keys[info.Key];
            var text = texts[docId];
            var saved = spansBy.GetValueOrDefault(r["result_id"]) ?? [];
            var keySpans = Scoring.KeySpans(text, gt);
            var ipv6 = RuleDetector.FindIpv6(text);
            foreach (var corpus in new[] { info.Corpus, "all" })
            {
                stored.AddOrUpdate((corpus, r["model"]), (int.Parse(r["present"], CultureInfo.InvariantCulture), int.Parse(r["caught"], CultureInfo.InvariantCulture), int.Parse(r["edits"], CultureInfo.InvariantCulture), int.Parse(r["true_positives"], CultureInfo.InvariantCulture)),
                    (_, t) => (t.Present + int.Parse(r["present"], CultureInfo.InvariantCulture), t.Caught + int.Parse(r["caught"], CultureInfo.InvariantCulture), t.Edits + int.Parse(r["edits"], CultureInfo.InvariantCulture), t.Tp + int.Parse(r["true_positives"], CultureInfo.InvariantCulture)));
            }

            foreach (var scenario in scenarios)
            {
                var (kept, suppressed) = CleanUpRules.Apply(text, saved, scenario.Options);
                var spans = scenario.Options.Ipv6 ? [.. kept, .. ipv6] : kept;
                var score = Scoring.Score(text, Redactor.Apply(text, spans, template), gt, info.Format);
                foreach (var corpus in new[] { info.Corpus, "all" })
                {
                    var t = totals.GetOrAdd((corpus, r["model"], scenario.Name), _ => new Totals());
                    lock (t)
                    {
                        t.Present += score.Present;
                        t.Caught += score.Caught;
                        t.Edits += score.Edits;
                        t.TruePositives += score.TruePositives;
                        t.Documents++;
                    }
                }

                if (scenario.Name == "all rules together" || scenario.Options.AnyOn && scenario.Name != "IPv6 addresses (adds)")
                {
                    foreach (var s in suppressed)
                    {
                        var right = Scoring.OverlapsKey(keySpans, s.Start, s.Length);
                        if (scenario.Name == "all rules together")
                        {
                            continue;
                        }

                        suppressedCount.AddOrUpdate((s.Rule, right), 1, (_, n) => n + 1);
                        if (right)
                        {
                            lost.Add(new Lost(s.Rule, r["model"], docId, s.Type, text.Substring(s.Start, s.Length)));
                        }
                    }
                }
            }

            if (Interlocked.Increment(ref done) % 500 == 0)
            {
                Console.WriteLine($"  {done} of {models.Count} results");
            }
        });

        var sb = new StringBuilder();
        sb.AppendLine("# What the clean-up rules would do").AppendLine();
        sb.AppendLine($"Worked out from the spans saved in `{Path.GetFileName(datasetDir)}` with no model run: each model's saved answers have each rule applied, and are scored against the answer keys again. Models: {string.Join(", ", models.Select(m => m["model"]).Distinct().Order())}.").AppendLine();
        sb.AppendLine("**Read this first.** The rules were chosen after looking at these same documents, so the gain shown is an upper estimate. A fair test needs documents the rules were not based on. The figure that matters most is the second table: how many *right* redactions a rule would take away, because that is the risk.").AppendLine();
        foreach (var corpus in new[] { "heldout", "formats", "all" })
        {
            sb.AppendLine($"## {(corpus == "all" ? "Both corpora" : corpus == "heldout" ? "Held-out (300 documents)" : "Formats (38 documents)")}").AppendLine();
            sb.AppendLine("| Model | Rules | Recall | Precision | F1 | Change in precision | Change in recall | Wrong redactions removed | Right redactions lost |");
            sb.AppendLine("|---|---|---:|---:|---:|---:|---:|---:|---:|");
            foreach (var model in models.Select(m => m["model"]).Distinct().Order())
            {
                if (!totals.TryGetValue((corpus, model, scenarios[0].Name), out var baseline))
                {
                    continue;
                }

                foreach (var sc in scenarios)
                {
                    var t = totals[(corpus, model, sc.Name)];
                    var wrongRemoved = (baseline.Edits - baseline.TruePositives) - (t.Edits - t.TruePositives);
                    var rightLost = baseline.TruePositives - t.TruePositives;
                    sb.AppendLine($"| {model} | {sc.Name} | {P(t.Recall)} | {P(t.Precision)} | {P(t.F1)} | {(sc == scenarios[0] ? "" : $"{(t.Precision - baseline.Precision) * 100:+0.0;-0.0;0.0} pts")} | {(sc == scenarios[0] ? "" : $"{(t.Recall - baseline.Recall) * 100:+0.0;-0.0;0.0} pts")} | {(sc == scenarios[0] ? "" : wrongRemoved.ToString(CultureInfo.InvariantCulture))} | {(sc == scenarios[0] ? "" : rightLost.ToString(CultureInfo.InvariantCulture))} |");
                }
            }

            sb.AppendLine();
        }

        sb.AppendLine("## Does the starting point match the saved results?").AppendLine();
        sb.AppendLine("The first row of each model (no rules) is rescored here and must equal the saved result. Differences below would mean the simulation is wrong.").AppendLine();
        var mismatches = 0;
        foreach (var ((corpus, model), s) in stored.OrderBy(k => k.Key))
        {
            var t = totals[(corpus, model, scenarios[0].Name)];
            var same = (t.Present, t.Caught, t.Edits, t.TruePositives) == s;
            if (!same)
            {
                mismatches++;
                sb.AppendLine($"- {corpus} / {model}: simulated present {t.Present}, caught {t.Caught}, edits {t.Edits}, right {t.TruePositives}; saved {s.Present}, {s.Caught}, {s.Edits}, {s.Tp}");
            }
        }

        sb.AppendLine(mismatches == 0 ? "All match." : $"{mismatches} do not match.").AppendLine();
        sb.AppendLine("## What each rule removes, over all five models").AppendLine();
        sb.AppendLine("| Rule | Redactions removed that were wrong | Redactions removed that were right (the cost) |");
        sb.AppendLine("|---|---:|---:|");
        foreach (var rule in new[] { CleanUpRules.BracketedRule, CleanUpRules.MaskedRule, CleanUpRules.GenericRule, CleanUpRules.BirthRule })
        {
            sb.AppendLine($"| {CleanUpRules.Label(rule)} | {suppressedCount.GetValueOrDefault((rule, false))} | {suppressedCount.GetValueOrDefault((rule, true))} |");
        }

        sb.AppendLine().AppendLine("## Right redactions a rule would lose (examples to check)").AppendLine();
        var grouped = lost.GroupBy(l => (l.Rule, l.Text, l.Type)).Select(g => (g.Key, Count: g.Count(), Models: g.Select(x => x.Model).Distinct().Count(), Doc: g.First().DocId)).OrderByDescending(x => x.Count).Take(25).ToList();
        if (grouped.Count == 0)
        {
            sb.AppendLine("None: no rule took away a redaction that matched the answer key.");
        }
        else
        {
            sb.AppendLine("| Rule | Category | Text | Times (all models) | Models | Example document |");
            sb.AppendLine("|---|---|---|---:|---:|---|");
            foreach (var g in grouped)
            {
                sb.AppendLine($"| {g.Key.Rule} | {g.Key.Type} | `{g.Key.Text.Replace("|", "\\|", StringComparison.Ordinal)}` | {g.Count} | {g.Models} | {g.Doc} |");
            }
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
        await File.WriteAllTextAsync(outPath, sb.ToString());
        Console.WriteLine($"Report: {outPath}{(mismatches == 0 ? string.Empty : $"  (WARNING: {mismatches} starting points differ from the saved results)")}");
        return mismatches == 0 ? 0 : 1;
    }
}
