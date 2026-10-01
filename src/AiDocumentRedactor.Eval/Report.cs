using System.Globalization;
using System.Text;
using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.Eval;

/// <summary>What was run: the models, the settings and the corpus, for the top of the report.</summary>
public record RunInfo(DateTime Started, TimeSpan Elapsed, string Machine, string CorpusDir, int Documents, int GroundTruthEntities,
    RedactorOptions Options, List<(string Model, ModelInfo? Info)> Models, bool ShowText, bool WroteOutputs, List<string> Skipped);

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
        sb.AppendLine("- Categories on: " + string.Join(", ", PromptBuilder.EnabledTypes(o)) + (o.Entities.TryGetValue("GENDER", out var g) && g.RedactPronouns ? " (pronouns included)" : "") + ".");
        var flag = o.Entities.Where(kv => kv.Value.Mode == "flag").Select(kv => kv.Key).ToList();
        if (flag.Count > 0)
        {
            sb.AppendLine("- Flag-only (reported, not redacted, so they count as missed here): " + string.Join(", ", flag) + ".");
        }

        sb.AppendLine("- Models: " + string.Join("; ", run.Models.Select(m => m.Info is null ? m.Model : $"{m.Model} ({m.Info.Summary}, digest {m.Info.ShortDigest})")) + ".").AppendLine();

        sb.AppendLine("## How to read this, and its limits").AppendLine();
        sb.AppendLine("- **Synthetic corpus.** The documents are invented and every sensitive item is known exactly. The scores compare models fairly with each other; they are not a promise about a client's real documents, which are messier.");
        sb.AppendLine("- **Strict recall.** An item counts as caught only when its text is gone from the redacted text. A partial redaction (for example only the surname of a full name) leaves the rest visible and counts as a miss.");
        sb.AppendLine("- **Precision** counts a redaction as correct if it overlaps anything on the answer key, whatever its label; label accuracy is reported separately. Text the key does not list but which a person might also want removed counts against precision.");
        sb.AppendLine("- **Scans** are read by OCR first. Words OCR misreads can't be found by the model, so scan scores mix model and OCR quality.");
        sb.AppendLine("- Results with a local model can vary slightly between runs and machines; the temperature and seed are fixed to keep this small.");
        return sb.ToString();
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
