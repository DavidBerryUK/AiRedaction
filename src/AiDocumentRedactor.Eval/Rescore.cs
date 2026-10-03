using AiDocumentRedactor.Core;
using AiDocumentRedactor.Detection;
using AiDocumentRedactor.Documents;

namespace AiDocumentRedactor.Eval;

/// <summary>Rebuilds a whole report from the spans saved in an earlier run, scoring them again against the answer key as it is now (for example after the key
/// was audited). No model is run. Timings, token counts and output checks are carried over from the saved run; everything that depends on the key is recomputed.
/// Only corpora whose documents can be read without OCR can be rescored.</summary>
public static class Rescore
{
    static readonly string[] Baselines = ["rules only", "GLiNER only", "rules + GLiNER"];

    /// <summary>Rescores a saved run and writes the report and its scores file. Returns the exit code.</summary>
    public static async Task<int> RunAsync(string savedPath, string outPath, string note)
    {
        var saved = SavedRun.Load(savedPath);
        var options = saved.Options;
        var template = options.Redaction.PlaceholderTemplate;
        var truth = GroundTruthStore.Load(saved.CorpusDir);
        IDocumentReader[] readers = [new TextDocumentReader(), new DocxDocumentReader(), new PdfDocumentReader()];
        var byFile = saved.Scores.GroupBy(s => s.File).ToList();
        var rescored = new List<DocScore>();
        var skipped = new List<string>();
        foreach (var group in byFile)
        {
            var file = group.Key;
            var path = Path.Combine(saved.CorpusDir, file);
            var gt = GroundTruthStore.For(Path.GetFileName(file), truth);
            var reader = readers.FirstOrDefault(r => r.CanRead(path));
            if (gt is null || reader is null || !File.Exists(path))
            {
                skipped.Add($"`{file}`: {(gt is null ? "no answer key" : "cannot be read without OCR")}");
                continue;
            }

            string text;
            try
            {
                text = (await reader.ReadAsync(path, CancellationToken.None)).Text;
            }
            catch (Exception ex)
            {
                skipped.Add($"`{file}`: {ex.Message}");
                continue;
            }

            var rows = group.ToDictionary(r => r.Model);
            var gliner = rows.TryGetValue("GLiNER only", out var g) ? g.Spans.Select(ToSpan).ToList() : [];
            var key = Scoring.KeySpans(text, gt);
            var format = GroundTruthStore.FormatGroup(path);
            DocScore Score(IEnumerable<DetectedEntity> spans, DocScore from, string name, DocScore? flagsFrom = null)
            {
                var sc = Scoring.Score(text, Redactor.Apply(text, spans, template), gt, format);
                sc.File = file;
                sc.Model = name;
                sc.DetectSeconds = from.DetectSeconds;
                sc.WriteSeconds = from.WriteSeconds;
                sc.GlinerSeconds = from.GlinerSeconds;
                sc.PromptTokens = from.PromptTokens;
                sc.OutputTokens = from.OutputTokens;
                sc.Discarded = from.Discarded;
                sc.OutputOk = from.OutputOk;
                sc.OutputError = from.OutputError;
                sc.Spans = from.Spans;
                return sc;
            }

            foreach (var row in group)
            {
                var name = row.Model;
                if (Baselines.Contains(name) || !name.Contains(" + GLiNER", StringComparison.Ordinal))
                {
                    rescored.Add(Score(row.Spans.Select(ToSpan), row, name));
                }
            }

            // The model-plus-GLiNER rows are rebuilt from the model's own spans and GLiNER's, with the same agreement rules as the evaluation.
            foreach (var model in group.Select(r => r.Model).Where(m => !Baselines.Contains(m) && !m.Contains(" + GLiNER", StringComparison.Ordinal)))
            {
                if (!rows.ContainsKey(model + " + GLiNER") || gliner.Count == 0 && !rows.ContainsKey("GLiNER only"))
                {
                    continue;
                }

                var baseRow = rows[model];
                var combined = AgreementCombiner.Combine(baseRow.Spans.Select(ToSpan).ToList(), gliner, options);
                var auto = Score(combined, baseRow, model + " + GLiNER");
                var all = Score(combined.Select(s => s.Source == "gliner-only" ? s with { Flag = false } : s), baseRow, model + " + GLiNER (all flags accepted)");
                var reviewed = Score(combined.Select(s => s.Source == "gliner-only" && Scoring.OverlapsKey(key, s.Start, s.Length) ? s with { Flag = false } : s), baseRow, model + " + GLiNER (correct flags accepted)");
                foreach (var v in new[] { auto, all, reviewed })
                {
                    v.FlagsRaised = all.Edits - auto.Edits;
                    v.FlagsCorrect = all.TruePositives - auto.TruePositives;
                    v.DetectSeconds = baseRow.DetectSeconds + baseRow.GlinerSeconds;
                    v.GlinerSeconds = rows["GLiNER only"].GlinerSeconds;
                    v.Spans = [];
                    rescored.Add(v);
                }
            }
        }

        var order = saved.Models.Select((m, i) => (m.Model, i)).ToDictionary(x => x.Model, x => x.i);
        var run = saved with { Scores = rescored, Skipped = [.. saved.Skipped, .. skipped], Documents = rescored.Select(s => s.File).Distinct().Count(), GroundTruthEntities = truth.Sum(t => t.Entities.Count) };
        var md = MarkdownReport.Build(run.ToRunInfo(), rescored);
        var firstBreak = md.IndexOf('\n', StringComparison.Ordinal);
        md = md[..firstBreak] + "\n\n> " + note + md[firstBreak..];
        Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
        File.WriteAllText(outPath, md);
        File.WriteAllText(Path.ChangeExtension(outPath, ".scores.json"), run.ToJson());
        Console.WriteLine($"Rescored {rescored.Select(s => s.File).Distinct().Count()} documents ({skipped.Count} skipped). Report: {outPath}");
        return 0;
    }

    static DetectedEntity ToSpan(SavedSpan s) => new(s.Type, s.Start, s.Length, s.Confidence, s.Source, s.Flag);
}
