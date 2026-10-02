using System.Globalization;
using System.Text;
using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.Eval;

/// <summary>What was run: the models, the settings and the corpus, for the top of the report.</summary>
public record RunInfo(DateTime Started, TimeSpan Elapsed, string Machine, string CorpusDir, int Documents, int GroundTruthEntities,
    RedactorOptions Options, List<(string Model, ModelInfo? Info)> Models, bool ShowText, bool WroteOutputs, List<string> Skipped, string? OllamaVersion = null);

/// <summary>Totals for one model over a set of documents.</summary>
public record Totals(int Present, int Caught, int Edits, int TruePositives, int TypeCorrect, int Leaked, int PreserveTotal, int PreserveBroken, int Lost, int Docs,
    double Seconds, long Tokens, long OutTokens)
{
    /// <summary>Share of sensitive occurrences removed.</summary>
    public double Recall => Present == 0 ? 1 : (double)Caught / Present;
    /// <summary>Share of redactions that covered something sensitive.</summary>
    public double Precision => Edits == 0 ? 1 : (double)TruePositives / Edits;
    /// <summary>Harmonic mean of recall and precision.</summary>
    public double F1 => Recall + Precision == 0 ? 0 : 2 * Recall * Precision / (Recall + Precision);
    /// <summary>Output tokens per second while the model was working.</summary>
    public double TokensPerSecond => Seconds > 0 ? OutTokens / Seconds : 0;
    /// <summary>Adds up scores.</summary>
    public static Totals Of(IEnumerable<DocScore> s)
    {
        var l = s.ToList();
        return new Totals(l.Sum(x => x.Present), l.Sum(x => x.Caught), l.Sum(x => x.Edits), l.Sum(x => x.TruePositives), l.Sum(x => x.TypeCorrect),
            l.Sum(x => x.Present - x.Caught), l.Sum(x => x.MustPreserve), l.Sum(x => x.PreserveBroken.Count), l.Sum(x => x.LostToExtraction), l.Count,
            l.Sum(x => x.DetectSeconds), l.Sum(x => x.PromptTokens), l.Sum(x => x.OutputTokens));
    }
}

/// <summary>Turns the scores into a Markdown report a person can read, share or paste into a document.</summary>
public static class MarkdownReport
{
    static string P(double v) => (v * 100).ToString("0.0", CultureInfo.InvariantCulture) + "%";
    static string Cell(string s) => s.Replace("|", "\\|").Replace("\n", " ");
    /// <summary>One table row from single cells and lists of cells, flattened.</summary>
    static string[] Row(params object[] parts) => parts.SelectMany(p => p is IEnumerable<string> l ? l : [p.ToString() ?? string.Empty]).ToArray();
    /// <summary>A Markdown table from a header row and body rows.</summary>
    static void Table(StringBuilder sb, string[] head, IEnumerable<string[]> rows)
    {
        sb.AppendLine("| " + string.Join(" | ", head) + " |");
        sb.AppendLine("|" + string.Join("|", head.Select((h, i) => i == 0 ? "---" : "---:")) + "|");
        foreach (var r in rows)
        {
            sb.AppendLine("| " + string.Join(" | ", r.Select(Cell)) + " |");
        }

        sb.AppendLine();
    }

    /// <summary>Builds the whole report.</summary>
    public static string Build(RunInfo run, List<DocScore> scores)
    {
        var models = run.Models.Select(m => m.Model).ToList();
        var by = models.ToDictionary(m => m, m => scores.Where(s => s.Model == m).ToList());
        var tot = models.ToDictionary(m => m, m => Totals.Of(by[m]));
        var sb = new StringBuilder();
        sb.AppendLine("# Redaction evaluation report").AppendLine();
        sb.AppendLine($"Run on {run.Started:yyyy-MM-dd HH:mm} ({run.Machine}); took {run.Elapsed:hh\\:mm\\:ss}. " +
                      $"{run.Documents} documents, {run.GroundTruthEntities} items on the answer key, {models.Count} model{(models.Count == 1 ? "" : "s")}. " +
                      "Everything ran on this machine through local models.").AppendLine();
        if (!run.ShowText)
        {
            sb.AppendLine("> This report contains **no document text**: only counts, categories and file names. Run with `--show-text` to list the missed and over-redacted strings (only sensible on synthetic data).").AppendLine();
        }

        sb.AppendLine("## Summary").AppendLine();
        Table(sb, ["Model", "Size", "Recall", "Precision", "F1", "Sensitive items missed", "Over-redactions", "Must-keep items damaged", "Time per document", "Output tokens/s"],
            models.Select(m =>
            {
                var t = tot[m];
                var info = run.Models.First(x => x.Model == m).Info;
                return new[] { m, info is null ? "?" : $"{info.ParameterSize} · {info.SizeBytes / 1_000_000_000.0:0.0} GB", P(t.Recall), P(t.Precision), P(t.F1),
                    $"{t.Leaked} of {t.Present}", $"{t.Edits - t.TruePositives} of {t.Edits}", t.PreserveTotal == 0 ? "–" : $"{t.PreserveBroken} of {t.PreserveTotal}",
                    TimeSpan.FromSeconds(t.Docs == 0 ? 0 : t.Seconds / t.Docs).ToString(@"m\:ss\.f"), t.TokensPerSecond.ToString("0", CultureInfo.InvariantCulture) };
            }));
        sb.AppendLine("### What the columns mean").AppendLine();
        sb.AppendLine("- **Recall** answers: *of everything that should have been hidden, how much did the model hide?* If a document has 100 sensitive items and the model hides 95, recall is 95%. The other 5 are leaks, so for a redaction tool this is the most important number. It is strict: hiding only the surname of \"Jane Smith\" leaves the first name visible and counts as a miss.");
        sb.AppendLine("- **Precision** answers: *of everything the model hid, how much really needed hiding?* If it hides 100 things and 90 were sensitive, precision is 90%. The other 10 are over-redactions: harmless, but they make the document harder to read.");
        sb.AppendLine("- **F1** is a single score that blends recall and precision. It is high only when both are high, so a model cannot score well by hiding everything (perfect recall, poor precision) or by hiding almost nothing (high precision, poor recall). Use it for a quick ranking, but look at recall first.");
        sb.AppendLine("- **Sensitive items missed** is the count behind recall (\"3 of 120\" means 3 sensitive items were left visible).");
        sb.AppendLine("- **Over-redactions** is the count behind precision (\"8 of 130\" means 8 of the 130 redactions covered text that did not need hiding).");
        sb.AppendLine("- **Must-keep items damaged** (also called *preserved*) checks the opposite risk. Each test document contains ordinary text that must survive, such as dates, job titles, amounts, product names and general places. This counts how many of those were wrongly removed. \"0 of 40\" is ideal, and each one damaged is information the reader needed that is now gone.");
        sb.AppendLine("- **Time per document** is the average wall-clock time to redact one document, and **Output tokens/s** is how fast the model writes its answer (a hardware and model-size measure).").AppendLine();

        sb.AppendLine("## Detail per model").AppendLine();
        sb.AppendLine("Where each model's time and effort went, and how many documents it could not process. *Items fully caught* counts whole items (a full name is one item) rather than every occurrence; *label accuracy* is the share of correct redactions given the right category; *lost to OCR* counts items the scan reader never produced, which no model could have found. For rows that include **GLiNER**, *flagged for review* counts things only GLiNER found, which are left in the text for a person to check, with how many of them really were sensitive. A row marked *(flags accepted)* shows the result if the reviewer accepted every flag.").AppendLine();
        Table(sb, ["Model", "Documents scored", "Documents failed", "Total model time", "Median document", "Slowest document", "Prompt tokens", "Output tokens", "Items fully caught", "Label accuracy", "Lost to OCR", "Flagged for review (really sensitive)"],
            models.Select(m =>
            {
                var d = by[m];
                var sorted = d.Select(x => x.DetectSeconds).Order().ToList();
                var slow = d.Count == 0 ? null : d.MaxBy(x => x.DetectSeconds);
                var t = tot[m];
                return new[] { m, d.Count.ToString(), run.Skipped.Count(x => x.Contains($"with {m}:")).ToString(), TimeSpan.FromSeconds(t.Seconds).ToString(@"h\:mm\:ss"),
                    sorted.Count == 0 ? "–" : $"{sorted[sorted.Count / 2]:0.0} s", slow is null ? "–" : $"{slow.DetectSeconds:0.0} s ({slow.File})",
                    t.Tokens.ToString("N0", CultureInfo.InvariantCulture), t.OutTokens.ToString("N0", CultureInfo.InvariantCulture),
                    $"{d.Sum(x => x.EntitiesFullyCaught)} of {d.Sum(x => x.EntitiesPresent)}", P(t.TruePositives == 0 ? 1 : (double)t.TypeCorrect / t.TruePositives), t.Lost.ToString(), d.Sum(x => x.FlagsRaised) == 0 ? "–" : $"{d.Sum(x => x.FlagsRaised)} ({d.Sum(x => x.FlagsCorrect)})" };
            }));

        Findings(sb, run, models, tot, by);

        sb.AppendLine("## Recall by category").AppendLine();
        var cats = scores.SelectMany(s => s.ByCategory.Keys).Distinct().Order().ToList();
        Table(sb, ["Category", "Items", .. models], cats.Select(c =>
        {
            var present = by[models[0]].Sum(s => s.ByCategory.GetValueOrDefault(c).Present);
            return Row(c, present.ToString(), models.Select(m => { var p = by[m].Sum(s => s.ByCategory.GetValueOrDefault(c).Present); var k = by[m].Sum(s => s.ByCategory.GetValueOrDefault(c).Caught); return p == 0 ? "–" : $"{P((double)k / p)} ({k}/{p})"; }));
        }));

        sb.AppendLine("## Recall by document format").AppendLine();
        var groups = scores.Select(s => s.Group).Distinct().Order().ToList();
        Table(sb, ["Format", "Documents", "Items", .. models], groups.Select(g =>
        {
            var docs = by[models[0]].Count(s => s.Group == g);
            var present = by[models[0]].Where(s => s.Group == g).Sum(s => s.Present);
            return Row(g, docs.ToString(), present.ToString(), models.Select(m => { var t = Totals.Of(by[m].Where(s => s.Group == g)); return t.Present == 0 ? "–" : $"{P(t.Recall)} ({t.Caught}/{t.Present})"; }));
        }));
        var lost = models.Select(m => by[m].Where(s => s.Group.StartsWith("Scan")).Sum(s => s.LostToExtraction)).FirstOrDefault();
        if (lost > 0)
        {
            sb.AppendLine($"For scans, {lost} answer-key item(s) were not readable by OCR at all, so no model could see them. They are **not** counted above but may still be visible in the output image; they are a limit of OCR, not of the model.").AppendLine();
        }

        sb.AppendLine("## Precision by redaction category").AppendLine();
        var etypes = scores.SelectMany(s => s.EditsByType.Keys).Distinct().Order().ToList();
        Table(sb, ["Category", .. models], etypes.Select(c => Row(c, models.Select(m =>
        {
            var e = by[m].Sum(s => s.EditsByType.GetValueOrDefault(c).Edits);
            var k = by[m].Sum(s => s.EditsByType.GetValueOrDefault(c).Correct);
            return e == 0 ? "–" : $"{P((double)k / e)} ({k}/{e})";
        }))));
        sb.AppendLine("Of the correct redactions, the share given the right category label: " + string.Join("; ", models.Select(m => $"{m} {P(tot[m].TruePositives == 0 ? 1 : (double)tot[m].TypeCorrect / tot[m].TruePositives)}")) + ".").AppendLine();

        sb.AppendLine("## Per document").AppendLine();
        var files = scores.Select(s => s.File).Distinct().Order().ToList();
        Table(sb, ["Document", "Format", "Items", .. models.Select(m => m + " (recall · missed · over)")], files.Select(f =>
        {
            var any = scores.First(s => s.File == f);
            return Row(f, any.Group, any.Present.ToString(), models.Select(m => scores.FirstOrDefault(s => s.File == f && s.Model == m) is { } s ? (s.Present == 0 ? "–" : P((double)s.Caught / s.Present)) + $" · {s.Present - s.Caught} · {s.FalsePositives.Count + s.PreserveBroken.Count}" : "–"));
        }));

        sb.AppendLine("## Timings").AppendLine();
        sb.AppendLine("Seconds for each document and model. The first figure is the model finding the sensitive items" + (run.WroteOutputs ? "; the second is writing and verifying the redacted file (PDF render, OCR re-read of scans, and so on)" : "") + ". Models are run one after another, every document with one model before the next model is loaded, so a model is loaded into memory once.").AppendLine();
        string Time(DocScore? s) => s is null ? "–" : s.DetectSeconds.ToString("0.0", CultureInfo.InvariantCulture) + (s.OutputOk is null ? "" : " + " + s.WriteSeconds.ToString("0.0", CultureInfo.InvariantCulture));
        var timeRows = files.Select(f => Row(f, models.Select(m => Time(scores.FirstOrDefault(s => s.File == f && s.Model == m))))).ToList();
        timeRows.Add(Row("**Total**", models.Select(m => $"**{by[m].Sum(s => s.DetectSeconds):0.0}" + (run.WroteOutputs ? $" + {by[m].Sum(s => s.WriteSeconds):0.0}" : "") + "**")));
        timeRows.Add(Row("Average per document", models.Select(m => by[m].Count == 0 ? "–" : $"{by[m].Average(s => s.DetectSeconds):0.0}")));
        Table(sb, ["Document", .. models], timeRows);

        if (run.ShowText)
        {
            sb.AppendLine("## What was missed and over-redacted").AppendLine("*Contains text from the documents.*").AppendLine();
            foreach (var m in models)
            {
                sb.AppendLine($"### {m}").AppendLine();
                var leaks = by[m].SelectMany(s => s.Leaks.Select(l => (s.File, l))).ToList();
                sb.AppendLine("**Missed**").AppendLine();
                if (leaks.Count == 0)
                {
                    sb.AppendLine("- nothing");
                }

                foreach (var (f, l) in leaks)
                {
                    sb.AppendLine($"- `{f}` {l.Type}: “{l.Text}” ({l.Count})");
                }

                sb.AppendLine().AppendLine("**Over-redacted**").AppendLine();
                var fps = by[m].SelectMany(s => s.FalsePositives.Select(x => (s.File, x))).ToList();
                var broken = by[m].SelectMany(s => s.PreserveBroken.Select(x => (s.File, x))).ToList();
                if (fps.Count + broken.Count == 0)
                {
                    sb.AppendLine("- nothing");
                }

                foreach (var (f, x) in fps)
                {
                    sb.AppendLine($"- `{f}` {x.Type}: “{x.Text}” is not on the answer key");
                }

                foreach (var (f, x) in broken)
                {
                    sb.AppendLine($"- `{f}` should have been kept: “{x}”");
                }

                sb.AppendLine();
            }
        }

        if (run.WroteOutputs)
        {
            sb.AppendLine("## Output safety").AppendLine();
            sb.AppendLine("Each redacted file was also written and passed through the tool's own checks (no text layer in PDFs, nothing recoverable in Word files, OCR re-read of scans). A file that fails is refused rather than written.").AppendLine();
            Table(sb, ["Model", "Files written and verified", "Refused or failed"], models.Select(m =>
            {
                var w = by[m].Where(s => s.OutputOk is not null).ToList();
                return new[] { m, $"{w.Count(s => s.OutputOk == true)} of {w.Count}", w.Count(s => s.OutputOk == false) == 0 ? "none" : string.Join("; ", w.Where(s => s.OutputOk == false).Select(s => $"{s.File}: {s.OutputError}")) };
            }));
        }

        if (run.Skipped.Count > 0)
        {
            sb.AppendLine("## Skipped").AppendLine();
            foreach (var x in run.Skipped)
            {
                sb.AppendLine("- " + x);
            }

            sb.AppendLine();
        }

        sb.AppendLine("## Settings used").AppendLine();
        var o = run.Options;
        sb.AppendLine($"- Temperature {o.Llm.Temperature}, seed {o.Llm.Seed}, context {o.Llm.NumCtx} tokens, chunks of about {o.Llm.ChunkChars} characters with {o.Llm.ChunkOverlapChars} overlap.");
        sb.AppendLine("- Reasoning (think): " + (o.Llm.Think switch { false => "off (gpt-oss cannot switch it off, so it runs at its lowest level, low)", true => "on", _ => "each model's own default" }) + $". Model kept loaded for {o.Llm.KeepAlive}; each model is unloaded when its turn ends.");
        sb.AppendLine($"- Run on {run.Machine}; Ollama {run.OllamaVersion ?? "version unknown"}; endpoint {o.Llm.Endpoint}; started {run.Started:yyyy-MM-dd HH:mm}.");
        sb.AppendLine("- Categories and their modes: " + string.Join(", ", o.Entities.Where(kv => kv.Value.Enabled).Select(kv => $"{kv.Key} ({(kv.Value.Mode == "flag" ? "flag only" : "redact")})")) + ".");
        sb.AppendLine("- Categories on: " + string.Join(", ", PromptBuilder.EnabledTypes(o)) + (o.Entities.TryGetValue("GENDER", out var g) && g.RedactPronouns ? " (pronouns included)" : "") + ".");
        var flag = o.Entities.Where(kv => kv.Value.Mode == "flag").Select(kv => kv.Key).ToList();
        if (flag.Count > 0)
        {
            sb.AppendLine("- Flag-only (reported, not redacted, so they count as missed here): " + string.Join(", ", flag) + ".");
        }

        sb.AppendLine("- Models: " + string.Join("; ", run.Models.Select(m => m.Info is null ? m.Model : $"{m.Model} ({m.Info.Summary}, digest {m.Info.ShortDigest})")) + ".").AppendLine();

        Combinations(sb, run, models, by);

        sb.AppendLine("## Beyond one model: combining models, and a model of our own").AppendLine();
        sb.AppendLine("A finished product would not have to rely on one model. The table above shows each model's strengths and gaps, and they are not the same gaps, so combining models is a real option. There are four common ways. **Union:** run two models and redact whatever either finds. Recall rises, because an item has to be missed by both to leak, but over-redaction and run time add up. This suits a tool where a leak costs far more than an extra black box. **Agreement:** with three or more models, redact only what at least two agree on, or send the disagreements to a person; this cuts over-redaction but gives up some recall, and the app's manual review screen is already the right place for the disagreements. **Cascade:** a small, fast model reads everything, and a larger one is used only on documents or passages where the small one is unsure or found something odd. **Specialists:** reliable patterns such as emails, phone numbers, postcodes and ID formats are better found by fixed rules, which are fast and never forget, leaving the model for names, companies and the contextual judgement calls where only a language model does well.").AppendLine();
        sb.AppendLine("The price of any combination is time and memory. On one machine the models run one after another, so the time is roughly the sum of the models used, and each must be loaded in turn. The estimate above is a ceiling on what a union could gain, worked out from this run's own misses; the real gain also depends on how many extra over-redactions the second model adds, which this report can only bound. Before choosing a combination, rerun the evaluation with the combination itself, since this harness scores any detector the same way.").AppendLine();
        sb.AppendLine("**Could we make our own model?** Yes, in three different senses, from least to most effort. *Tune what we have* (the instructions, category descriptions and examples in the configuration): cheap, already done once, and it moved results noticeably. *Fine-tune an existing open model* on examples of documents with the sensitive items marked: this is a well-trodden technique (a light-weight method called LoRA adjusts a small fraction of the weights) and a machine like this one can plausibly train a small or mid-sized model, which can then be loaded into Ollama like any other. The hard part is not the training but the **data**: it needs hundreds to thousands of carefully marked documents that look like the clients' real ones, and the synthetic corpus here, which is deliberately simple and invented, is a useful start but could teach a model the style of our test documents rather than real documents. *Train a dedicated, much smaller entity-recognition model* (the kind used for names and places in classic language-processing tools): fast, runs on an ordinary CPU, and very good at the plain categories, but weaker than a language model at the judgement calls such as contextual identifiers. Training a large language model from scratch is not realistic for this project.").AppendLine();
        sb.AppendLine("Our recommendation is to treat this as a staged question rather than a yes or no. The first step is the one under way: measure the off-the-shelf models and the combinations of them. If a gap remains in a specific category (for example contextual identifiers or place names), the next step is a small fine-tune aimed at that gap, using synthetic data plus a modest set of real, client-approved, hand-marked documents, and scored with this same harness on documents the model never saw in training. Costs to plan for are the marking effort, keeping any real client data local and out of the repository, checking that each base model's licence allows this use, and repeating the exercise whenever the base model changes. None of this needs to block the prototype: the evaluation harness is the part that makes every one of these options measurable.").AppendLine();

        sb.AppendLine("## How to read this, and its limits").AppendLine();
        sb.AppendLine("- **Synthetic corpus.** The documents are invented and every sensitive item is known exactly. The scores compare models fairly with each other; they are not a promise about a client's real documents, which are messier.");
        sb.AppendLine("- **Strict recall.** An item counts as caught only when its text is gone from the redacted text. A partial redaction (for example only the surname of a full name) leaves the rest visible and counts as a miss.");
        sb.AppendLine("- **Precision** counts a redaction as correct if it overlaps anything on the answer key, whatever its label; label accuracy is reported separately. Text the key does not list but which a person might also want removed counts against precision.");
        sb.AppendLine("- **Scans** are read by OCR first. Words OCR misreads can't be found by the model, so scan scores mix model and OCR quality.");
        sb.AppendLine("- Results with a local model can vary slightly between runs and machines; the temperature and seed are fixed to keep this small.");
        return sb.ToString();
    }


    /// <summary>Estimates what running two models together (a union of what they find) would do to recall, from the items each one missed. Needs the missed text, so only when the report shows text.</summary>
    static void Combinations(StringBuilder sb, RunInfo run, List<string> models, Dictionary<string, List<DocScore>> by)
    {
        if (!run.ShowText || models.Count < 2)
        {
            return;
        }

        sb.AppendLine("## What combining two models could achieve (estimate)").AppendLine();
        sb.AppendLine("If a document were redacted with *both* models of a pair and everything either found were removed, an item would leak only if both missed it. This estimate counts the items missed by both, from the missed lists in this run, so it is a ceiling on the recall of a union: it ignores that the second model's different wording may also redact the same text partly. The over-redaction figure is the most extra redactions that could add up (the two models' counts summed). Only documents scored by both models are counted. The top pairs by estimated recall are shown.").AppendLine();
        var results = new List<(string A, string B, double Single, double Union, int Over)>();
        for (var i = 0; i < models.Count; i++)
        {
            for (var j = i + 1; j < models.Count; j++)
            {
                var common = by[models[i]].Select(x => x.File).Intersect(by[models[j]].Select(x => x.File)).ToHashSet();
                var a = by[models[i]].Where(x => common.Contains(x.File)).ToList();
                var b = by[models[j]].Where(x => common.Contains(x.File)).ToList();
                var present = a.Sum(x => x.Present);
                if (present == 0)
                {
                    continue;
                }

                var bothMissed = 0;
                foreach (var da in a)
                {
                    var db = b.First(x => x.File == da.File);
                    foreach (var l in da.Leaks)
                    {
                        var other = db.Leaks.FirstOrDefault(x => x.Type == l.Type && x.Text == l.Text);
                        bothMissed += other is null ? 0 : Math.Min(l.Count, other.Count);
                    }
                }

                var best = Math.Max(a.Sum(x => x.Caught), b.Sum(x => x.Caught));
                results.Add((models[i], models[j], (double)best / present, 1 - (double)bothMissed / present,
                    a.Sum(x => x.Edits - x.TruePositives) + b.Sum(x => x.Edits - x.TruePositives)));
            }
        }

        Table(sb, ["Pair", "Better model alone (recall)", "Both together (estimated recall)", "Most extra over-redactions"],
            results.OrderByDescending(r => r.Union).ThenBy(r => r.Over).Take(8).Select(r => new[] { $"{r.A} + {r.B}", P(r.Single), P(r.Union), r.Over.ToString() }));
    }

    /// <summary>A few plain-English headline findings generated from the numbers.</summary>
    static void Findings(StringBuilder sb, RunInfo run, List<string> models, Dictionary<string, Totals> tot, Dictionary<string, List<DocScore>> by)
    {
        sb.AppendLine("## Headline findings").AppendLine();
        var bestRecall = models.OrderByDescending(m => tot[m].Recall).ThenByDescending(m => tot[m].Precision).First();
        sb.AppendLine($"- **Best at finding sensitive data:** {bestRecall}, removing {P(tot[bestRecall].Recall)} of items ({tot[bestRecall].Leaked} missed) with {P(tot[bestRecall].Precision)} precision.");
        if (models.Count > 1)
        {
            var fastest = models.OrderBy(m => tot[m].Docs == 0 ? 0 : tot[m].Seconds / tot[m].Docs).First();
            var bestF1 = models.OrderByDescending(m => tot[m].F1).First();
            sb.AppendLine($"- **Best balance (F1):** {bestF1} at {P(tot[bestF1].F1)}. **Fastest:** {fastest} at about {(tot[fastest].Seconds / Math.Max(1, tot[fastest].Docs)):0.0} s per document.");
        }
        var weakest = by[bestRecall].SelectMany(s => s.ByCategory.Select(kv => (Cat: kv.Key, kv.Value))).GroupBy(x => x.Cat)
            .Select(g => (Cat: g.Key, P: g.Sum(x => x.Value.Present), C: g.Sum(x => x.Value.Caught))).Where(x => x.P >= 3).OrderBy(x => (double)x.C / x.P).FirstOrDefault();
        if (weakest.Cat is not null && weakest.C < weakest.P)
        {
            sb.AppendLine($"- **Weakest category for {bestRecall}:** {weakest.Cat} ({P((double)weakest.C / weakest.P)}, {weakest.P - weakest.C} of {weakest.P} missed).");
        }

        var worstGroup = by[bestRecall].GroupBy(s => s.Group).Select(g => (G: g.Key, T: Totals.Of(g))).Where(x => x.T.Present > 0).OrderBy(x => x.T.Recall).FirstOrDefault();
        if (worstGroup.G is not null && worstGroup.T.Recall < 1)
        {
            sb.AppendLine($"- **Hardest format:** {worstGroup.G} ({P(worstGroup.T.Recall)} recall).");
        }

        var damaged = tot[bestRecall].PreserveBroken;
        sb.AppendLine(damaged == 0 ? $"- **Over-redaction check:** {bestRecall} left every must-keep item (product names, public bodies, places, ordinary numbers) untouched." : $"- **Over-redaction check:** {bestRecall} damaged {damaged} of {tot[bestRecall].PreserveTotal} must-keep items.");
        sb.AppendLine();
    }

    /// <summary>The raw scores as JSON (no text unless text was requested).</summary>
    public static string Json(RunInfo run, List<DocScore> scores) => System.Text.Json.JsonSerializer.Serialize(new {
        started = run.Started,
        elapsedSeconds = run.Elapsed.TotalSeconds,
        models = run.Models.Select(m => m.Model),
        documents = scores.Select(s => new {
            s.File,
            s.Group,
            s.Model,
            s.Present,
            s.Caught,
            s.Edits,
            s.TruePositives,
            s.TypeCorrect,
            s.LostToExtraction,
            s.DetectSeconds,
            s.WriteSeconds,
            s.PromptTokens,
            s.OutputTokens,
            s.Discarded,
            s.OutputOk,
            byCategory = s.ByCategory.ToDictionary(kv => kv.Key, kv => new { present = kv.Value.Present, caught = kv.Value.Caught }),
            leaks = s.Leaks.Select(l => new { l.Type, l.Count, text = run.ShowText ? l.Text : null }),
            falsePositives = s.FalsePositives.Select(f => new { f.Type, text = run.ShowText ? f.Text : null }),
            preserveBroken = run.ShowText ? s.PreserveBroken : null,
            preserveBrokenCount = s.PreserveBroken.Count,
        }),
    }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull });
}
