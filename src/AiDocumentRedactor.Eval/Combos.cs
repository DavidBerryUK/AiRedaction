using System.Globalization;
using System.Text;
using AiDocumentRedactor.Core;
using AiDocumentRedactor.Detection;
using AiDocumentRedactor.Documents;
using AiDocumentRedactor.Explorer;

namespace AiDocumentRedactor.Eval;

/// <summary>Scores real combinations of detectors from the spans saved in an earlier run, without running any model again: a union of models, majority
/// or unanimous agreement, and each of these with GLiNER's flags left for review or accepted by an ideal reviewer.</summary>
public static class Combos
{
    /// <summary>Spans from several detectors, grouped where they overlap. See <see cref="Voting.Vote"/>.</summary>
    public static List<DetectedEntity> Vote(IReadOnlyList<IReadOnlyList<DetectedEntity>> voters, int needed, bool flagRest) => Voting.Vote(voters, needed, flagRest);

    sealed record Row(string Name, Totals T, int Flags, int FlagsCorrect);

    /// <summary>Runs the combinations for the given models over the documents of a saved run and writes a Markdown report. Returns the exit code.</summary>
    public static async Task<int> RunAsync(string savedPath, string[] models, string outPath)
    {
        var saved = SavedRun.Load(savedPath);
        var options = saved.Options;
        var template = options.Redaction.PlaceholderTemplate;
        var truth = GroundTruthStore.Load(saved.CorpusDir);
        IDocumentReader[] readers = [new TextDocumentReader(), new DocxDocumentReader(), new PdfDocumentReader()];
        foreach (var m in models)
        {
            if (!saved.Scores.Any(s => s.Model == m && s.Spans.Count > 0))
            {
                Console.Error.WriteLine($"The saved run has no spans for '{m}'. Run the evaluation again (spans are saved from now on), then combine.");
                return 2;
            }
        }

        var hasGliner = saved.Scores.Any(s => s.Model == "GLiNER only");
        var files = saved.Scores.Where(s => s.Model == models[0]).Select(s => s.File).Distinct().Order().ToList();
        var rows = new Dictionary<string, List<DocScore>>();
        var flagCounts = new Dictionary<string, (int Raised, int Correct)>();
        var used = 0;
        void Add(string name, DocScore score) => (rows.TryGetValue(name, out var l) ? l : rows[name] = []).Add(score);
        foreach (var file in files)
        {
            var path = Path.Combine(saved.CorpusDir, file);
            var gt = GroundTruthStore.For(Path.GetFileName(file), truth);
            var reader = readers.FirstOrDefault(r => r.CanRead(path));
            if (gt is null || reader is null || !File.Exists(path))
            {
                continue;
            }

            string text;
            try
            {
                text = (await reader.ReadAsync(path, CancellationToken.None)).Text;
            }
            catch (Exception)
            {
                continue;
            }

            List<DetectedEntity> Spans(string model, string f) => saved.Scores.FirstOrDefault(s => s.Model == model && s.File == f)?.Spans
                .Select(x => new DetectedEntity(x.Type, x.Start, x.Length, x.Confidence, x.Source, x.Flag)).ToList() ?? [];
            var perModel = models.Select(m => Spans(m, file)).ToList();
            var gliner = hasGliner ? Spans("GLiNER only", file) : [];
            var group = GroundTruthStore.FormatGroup(path);
            var key = Scoring.KeySpans(text, gt);
            used++;

            DocScore Score(IEnumerable<DetectedEntity> spans, string name)
            {
                var sc = Scoring.Score(text, Redactor.Apply(text, spans, template), gt, group);
                sc.File = file;
                sc.Model = name;
                return sc;
            }

            void Strategy(string name, List<DetectedEntity> spans, bool hasFlags = false)
            {
                var auto = Score(spans, name);
                Add(name, auto);
                if (!hasFlags)
                {
                    return;
                }

                var accepted = Score(spans.Select(s => s.Flag && (s.Source is "gliner-only" or "single-detector") ? s with { Flag = false } : s), name + " (all flags accepted)");
                var reviewed = Score(spans.Select(s => s.Flag && (s.Source is "gliner-only" or "single-detector") && Scoring.OverlapsKey(key, s.Start, s.Length) ? s with { Flag = false } : s), name + " (correct flags accepted)");
                Add(name + " (correct flags accepted)", reviewed);
                var f = flagCounts.GetValueOrDefault(name);
                flagCounts[name] = (f.Raised + accepted.Edits - auto.Edits, f.Correct + accepted.TruePositives - auto.TruePositives);
            }

            for (var i = 0; i < models.Length; i++)
            {
                Strategy(models[i] + " alone", perModel[i]);
            }

            if (models.Length > 1)
            {
                var label = string.Join(" + ", models);
                Strategy($"union: {label}", perModel.SelectMany(x => x).ToList());
                Strategy($"all {models.Length} agree: {label}", Vote(perModel, models.Length, false));
                if (models.Length > 2)
                {
                    Strategy($"majority ({models.Length / 2 + 1} of {models.Length}): {label}", Vote(perModel, models.Length / 2 + 1, false));
                }

                Strategy($"singles flagged (2 or more redact, 1 flags): {label}", Vote(perModel, 2, true), hasFlags: true);
            }

            if (hasGliner)
            {
                var union = perModel.SelectMany(x => x).ToList();
                Strategy($"{(models.Length > 1 ? "union" : models[0])} + GLiNER flags", AgreementCombiner.Combine(union.DistinctBy(s => (s.Start, s.Length)).ToList(), gliner, options), hasFlags: true);
                var voters = perModel.Select(x => (IReadOnlyList<DetectedEntity>)x).Append(gliner).ToList();
                Strategy($"vote (2 of {voters.Count}, singles flagged): {string.Join(" + ", models)} + GLiNER", Vote(voters, 2, true), hasFlags: true);
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine("# Combination report").AppendLine();
        sb.AppendLine($"Built from `{Path.GetFileName(savedPath)}` ({saved.Started:yyyy-MM-dd HH:mm}) without running any model again: each detector's saved spans are combined and scored on {used} documents of `{saved.CorpusDir}` with the same strict scoring as the evaluation. Models: {string.Join(", ", models)}{(hasGliner ? ", and GLiNER" : "")}. Text-readable documents only (no OCR).").AppendLine();
        sb.AppendLine("**How to read the rows.** *alone*: the model by itself (with the fixed rules). *union*: redact whatever any model found. *all agree*: redact only what every model found. *majority*: what most models found. *singles flagged*: what two or more models found is redacted and what only one found is flagged for review and left in the text. *+ GLiNER flags*: the models' union, with anything only GLiNER found flagged. *vote*: groups found by at least two of the detectors, GLiNER counting as one. Rows marked *(correct flags accepted)* show an ideal reviewer who accepts only the flags that really were sensitive; the other rows leave every flag in the text (so flagged items count as missed).").AppendLine();
        sb.AppendLine("| Combination | Recall (95% range) | Precision (95% range) | F1 | Missed | Over-redactions | Must-keep damaged | Flags raised (really sensitive) |");
        sb.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|");
        foreach (var (name, list) in rows)
        {
            var t = Totals.Of(list);
            var baseName = name.Replace(" (correct flags accepted)", string.Empty, StringComparison.Ordinal);
            var f = flagCounts.GetValueOrDefault(baseName);
            sb.AppendLine($"| {name} | {MarkdownReport.Rng(t.Caught, t.Present)} | {MarkdownReport.Rng(t.TruePositives, t.Edits)} | {(t.F1 * 100).ToString("0.0", CultureInfo.InvariantCulture)}% | {t.Leaked} of {t.Present} | {t.Edits - t.TruePositives} of {t.Edits} | {(t.PreserveTotal == 0 ? "–" : $"{t.PreserveBroken} of {t.PreserveTotal}")} | {(f.Raised == 0 ? "–" : $"{f.Raised} ({f.Correct})")} |");
        }

        sb.AppendLine();
        sb.AppendLine("Ranges treat items as independent, so they are a little optimistic. Differences smaller than the ranges are not reliable.");
        Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
        File.WriteAllText(outPath, sb.ToString());
        Console.WriteLine($"Report: {outPath}");
        return 0;
    }
}
