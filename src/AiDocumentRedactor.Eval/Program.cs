// Evaluation harness: runs each model over the synthetic test corpus, scores the result against the answer key and writes a Markdown report.
// Usage: dotnet run --project src/AiDocumentRedactor.Eval -- [--config redactor.config.json] [--corpus tests/TestCorpus] [--models phi4,gemma3:27b]
//        [--out eval/eval-<time>.md] [--only text] [--no-write] [--show-text]
// It is a command, not part of the app: a full run takes a long time (every model over every document).
using System.Diagnostics;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.Json;
using AiDocumentRedactor.Core;
using AiDocumentRedactor.Detection;
using AiDocumentRedactor.Documents;
using AiDocumentRedactor.Eval;
using AiDocumentRedactor.Ocr;

string? Arg(string name)
{
    var i = Array.IndexOf(args, name);
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
}
if (args.Contains("--help") || args.Contains("-h"))
{
    Console.WriteLine("Evaluates local models on the test corpus and writes a Markdown report.\n\n  --config <file>   settings (default redactor.config.json)\n  --corpus <dir>    test corpus (default tests/TestCorpus)\n  --models a,b      models to compare (default: those with include: true in the config's evaluation.models list)\n  --out <file>      report path (default eval/eval-<date-time>.md; a .json with the raw scores is written beside it)\n  --only <text>     only corpus files whose path contains this text\n  --no-write        score the text only; skip writing and verifying the redacted files (faster)\n  --no-baselines    skip the no-model baseline rows (rules only; with --gliner also GLiNER only and rules + GLiNER)\n  --gliner          also score each model combined with GLiNER by agreement (needs the model files; see gliner in the config)\n  --show-text       list missed and over-redacted strings in the report (synthetic data only)\n  --rescore run.scores.json   rebuild a whole report from a saved run against the answer key as it is now (no model is run)\n  --audit-key run.scores.json --models a,b,c   audit the answer key against what most models agree on (a log; add --apply to rewrite the key)\n  --combine run.scores.json --models a,b[,c]   score real combinations (union, agreement, with GLiNER flags) from the spans saved in an earlier run, without running any model again\n  --merge a,b       rebuild one report from saved runs (the .scores.json beside each report); a model in a later file replaces the same model in earlier ones. Use --out for the report path.");
    return 0;
}

if (Arg("--rescore") is { } rescoreRun)
{
    // Rebuild a whole report from a saved run against the answer key as it is now, without running any model.
    return await Rescore.RunAsync(rescoreRun, Path.GetFullPath(Arg("--out") ?? Path.Combine("eval", $"eval-rescored-{DateTime.Now:yyyyMMdd-HHmm}.md")),
        Arg("--note") ?? "Rescored from the spans saved in an earlier run against the answer key as it is now. No model was run again.");
}

if (Arg("--audit-key") is { } auditRun)
{
    // Compare the answer key with what most models agree on and decide, by written rules, which is right. Nothing changes without --apply.
    var plain = (Arg("--models") ?? throw new ArgumentException("--audit-key needs --models a,b,c,d,e")).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    var log = Path.GetFullPath(Arg("--out") ?? Path.Combine("eval", "key-audit.md"));
    return KeyAudit.Run(auditRun, plain, args.Contains("--apply"), log, Path.ChangeExtension(log, ".decisions.json"));
}

if (Arg("--combine") is { } savedRun)
{
    // Score real combinations of the detectors saved in an earlier run, without running any model again.
    var combined = (Arg("--models") ?? throw new ArgumentException("--combine needs --models a,b[,c]")).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    return await Combos.RunAsync(savedRun, combined, Path.GetFullPath(Arg("--out") ?? Path.Combine("eval", $"eval-combos-{DateTime.Now:yyyyMMdd-HHmm}.md")));
}

if (Arg("--merge") is { } toMerge)
{
    // Rebuild one report from several saved runs (the .scores.json beside each report); a model in a later file replaces the same model in earlier ones.
    var merged = SavedRun.Merge(toMerge.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(SavedRun.Load).ToList());
    var mergedPath = Path.GetFullPath(Arg("--out") ?? Path.Combine("eval", $"eval-merged-{DateTime.Now:yyyyMMdd-HHmm}.md"));
    Directory.CreateDirectory(Path.GetDirectoryName(mergedPath)!);
    File.WriteAllText(mergedPath, MarkdownReport.Build(merged.ToRunInfo(), merged.Scores));
    File.WriteAllText(Path.ChangeExtension(mergedPath, ".scores.json"), merged.ToJson());
    Console.WriteLine($"Report: {mergedPath}");
    return 0;
}

RedactorOptions options;
try
{
    options = RedactorOptions.Load(Arg("--config") ?? "redactor.config.json");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Config error: {ex.Message}");
    return 2;
}

var corpus = Path.GetFullPath(Arg("--corpus") ?? Path.Combine("tests", "TestCorpus"));
if (!Directory.Exists(Path.Combine(corpus, "ground-truth")))
{
    Console.Error.WriteLine($"No ground-truth folder in {corpus}");
    return 2;
}
var truth = GroundTruthStore.Load(corpus);
var showText = args.Contains("--show-text");
if (args.Contains("--gliner"))
{
    options.Gliner.Enabled = true;   // also score each model combined with GLiNER by agreement
}

var write = !args.Contains("--no-write");
var only = Arg("--only");

// Which models: those asked for, else the configured candidates that are installed.
List<ModelInfo> installed;
try
{
    installed = (await new OllamaModelCatalog(OllamaDetector.CreateClient(options.Llm)).ListAsync(CancellationToken.None)).ToList();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Cannot reach Ollama at {options.Llm.Endpoint}: {ex.Message}");
    return 2;
}
string[] wanted;
if (Arg("--models") is { } asked)
{
    wanted = asked.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    var missing = wanted.Where(w => installed.All(i => i.Name != w)).ToList();
    if (missing.Count > 0)
    {
        Console.Error.WriteLine($"Not installed: {string.Join(", ", missing)}. Run: ollama pull <name>");
        return 2;
    }
}
else
{
    // From the config's evaluation.models list: the ones switched on, skipping (with a note) any that are not installed.
    var listed = options.Evaluation.Models.Where(m => m.Include).Select(m => m.Name).ToList();
    foreach (var n in listed.Where(n => installed.All(i => i.Name != n)))
    {
        Console.WriteLine($"Skipping {n}: switched on in the config but not installed (ollama pull {n})");
    }

    wanted = listed.Where(n => installed.Any(i => i.Name == n)).ToArray();
}
if (wanted.Length == 0)
{
    Console.Error.WriteLine("No models to run. Switch some on in evaluation.models in the config (include: true), or use --models.");
    return 2;
}

// Read every corpus document once (OCR for scans is slow); all models then see exactly the same text.
using var ocr = options.Ocr.Enabled ? new RapidOcrEngine() : null;
var readers = DocumentFormats.Readers(options, ocr);
var writers = DocumentFormats.Writers(options, ocr);
var skipped = new List<string>();
var docs = new List<(string Path, string Name, string Group, GroundTruth Truth, ExtractedDocument Doc)>();
foreach (var f in Directory.EnumerateFiles(corpus, "*", SearchOption.AllDirectories).Where(f => !f.Contains($"{Path.DirectorySeparatorChar}ground-truth{Path.DirectorySeparatorChar}") && !f.EndsWith("README.md")).Order())
{
    var rel = Path.GetRelativePath(corpus, f);
    if (only is not null && !rel.Contains(only, StringComparison.OrdinalIgnoreCase))
    {
        continue;
    }

    var gt = GroundTruthStore.For(Path.GetFileName(f), truth);
    var reader = readers.FirstOrDefault(r => r.CanRead(f));
    if (gt is null || reader is null)
    {
        skipped.Add($"`{rel}`: {(gt is null ? "no answer key" : "no reader for this type (is OCR switched off?)")}");
        continue;
    }
    try
    {
        Console.WriteLine($"Reading {rel}");
        docs.Add((f, rel, GroundTruthStore.FormatGroup(f), gt, await reader.ReadAsync(f, CancellationToken.None)));
    }
    catch (Exception ex)
    {
        skipped.Add($"`{rel}`: {ex.Message}");
    }
}
if (docs.Count == 0)
{
    Console.Error.WriteLine("No documents to evaluate.");
    return 2;
}

var started = DateTime.Now;
var clock = Stopwatch.StartNew();
var machine = $"{RuntimeInformation.OSDescription}, {Environment.ProcessorCount} cores";
var outPath = Path.GetFullPath(Arg("--out") ?? Path.Combine("eval", $"eval-{started:yyyyMMdd-HHmm}.md"));
Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
var ollamaVersion = await OllamaVersionAsync(options.Llm);
var scores = new List<DocScore>();
var modelInfos = new List<(string, ModelInfo?)>();

/// <summary>Writes the report and the raw scores, so a long run leaves usable results after every model.</summary>
void Save()
{
    var run = new RunInfo(started, clock.Elapsed, machine, corpus, docs.Count, docs.Select(d => d.Truth).DistinctBy(g => g.Id).Sum(g => g.Entities.Count), options, modelInfos, showText, write, skipped, ollamaVersion);
    File.WriteAllText(outPath, MarkdownReport.Build(run, scores));
    File.WriteAllText(Path.ChangeExtension(outPath, ".json"), MarkdownReport.Json(run, scores));
    File.WriteAllText(Path.ChangeExtension(outPath, ".scores.json"), SavedRun.From(run, scores).ToJson());
}

// No-model baselines: what the cheap layers do alone, which is what a language model has to improve on.
if (!args.Contains("--no-baselines"))
{
    var baselineGliner = GlinerDetector.Create(options);   // null unless GLiNER is on
    var names = new List<string> { "rules only" };
    if (baselineGliner is not null)
    {
        names.AddRange(["GLiNER only", "rules + GLiNER"]);
    }

    foreach (var name in names)
    {
        modelInfos.Add((name, null));
    }

    Console.WriteLine($"\n=== Baselines without a language model: {string.Join(", ", names)} ===");
    foreach (var d in docs)
    {
        var text = d.Doc.Text;
        var ruleClock = Stopwatch.StartNew();
        var rules = RuleDetector.Find(text, options);
        var ruleSeconds = ruleClock.Elapsed.TotalSeconds;
        var glinerClock = Stopwatch.StartNew();
        var gl = baselineGliner?.Detect(text) ?? [];
        var glinerSeconds = glinerClock.Elapsed.TotalSeconds;
        foreach (var (name, spans, seconds) in new[] { ("rules only", rules, ruleSeconds), ("GLiNER only", gl.ToList(), glinerSeconds), ("rules + GLiNER", rules.Concat(gl).ToList(), ruleSeconds + glinerSeconds) })
        {
            if (!names.Contains(name))
            {
                continue;
            }

            var result = Redactor.Apply(text, spans, options.Redaction.PlaceholderTemplate);
            var score = Scoring.Score(text, result, d.Truth, d.Group);
            score.File = d.Name;
            score.Model = name;
            score.DetectSeconds = seconds;
            score.GlinerSeconds = name == "rules only" ? 0 : glinerSeconds;
            score.Spans = spans.Select(ToSaved).ToList();
            scores.Add(score);
        }
    }

    foreach (var name in names)
    {
        var t = Totals.Of(scores.Where(x => x.Model == name));
        Console.WriteLine($"  {name}: recall {t.Recall:P1}, precision {t.Precision:P1}, {t.Leaked} missed of {t.Present}");
    }

    Save();
}

var modelNumber = 0;
foreach (var model in wanted)
{
    modelNumber++;
    modelInfos.Add((model, installed.First(i => i.Name == model)));
    var o = JsonSerializer.Deserialize<RedactorOptions>(JsonSerializer.Serialize(options, RedactorOptions.JsonOptions), RedactorOptions.JsonOptions)!;
    o.Llm.Model = model;
    var detector = new OllamaDetector(OllamaDetector.CreateClient(o.Llm), o, GlinerDetector.Create(o));
    var withGliner = o.Gliner.Enabled;
    var variantAuto = $"{model} + GLiNER";
    var variantAccepted = $"{model} + GLiNER (all flags accepted)";
    var variantReviewed = $"{model} + GLiNER (correct flags accepted)";
    if (withGliner)
    {
        modelInfos.Add((variantAuto, installed.First(i => i.Name == model)));
        modelInfos.Add((variantAccepted, installed.First(i => i.Name == model)));
        modelInfos.Add((variantReviewed, installed.First(i => i.Name == model)));
    }

    await detector.CheckAvailableAsync(CancellationToken.None);
    Console.WriteLine($"\n=== Model {modelNumber} of {wanted.Length}: {model} ===");
    var modelClock = Stopwatch.StartNew();   // every document is run with this model before the next model is loaded
    var docNumber = 0;
    foreach (var d in docs)
    {
        docNumber++;
        Console.Write($"  [model {modelNumber}/{wanted.Length} {model}] document {docNumber} of {docs.Count()}: {d.Name} ... ");
        long p0 = detector.PromptTokens, o0 = detector.OutputTokens;
        var x0 = detector.Discarded;
        var sw = Stopwatch.StartNew();
        RedactionResult result;
        IReadOnlyList<DetectedEntity> combined;
        try
        {
            combined = await detector.DetectAsync(d.Doc.Text, null, CancellationToken.None);
            result = Redactor.Apply(d.Doc.Text, withGliner ? detector.LastPrimarySpans : combined, o.Redaction.PlaceholderTemplate);
        }
        catch (Exception ex)
        {
            skipped.Add($"`{d.Name}` with {model}: {ex.Message}");
            Console.WriteLine($"FAILED {ex.Message}");
            continue;
        }
        sw.Stop();
        var score = Scoring.Score(d.Doc.Text, result, d.Truth, d.Group);
        score.File = d.Name;
        score.Model = model;
        score.DetectSeconds = sw.Elapsed.TotalSeconds - detector.LastGlinerSeconds;
        score.PromptTokens = detector.PromptTokens - p0;
        score.OutputTokens = detector.OutputTokens - o0;
        score.Discarded = detector.Discarded - x0;
        score.Spans = detector.LastPrimarySpans.Select(ToSaved).ToList();
        if (write && writers.FirstOrDefault(w => w.CanWrite(d.Doc)) is { } writer)
        {
            var tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + Path.GetExtension(d.Path));
            var writeClock = Stopwatch.StartNew();
            try
            {
                await writer.WriteAsync(d.Doc, result, tmp, CancellationToken.None);
                score.OutputOk = true;
            }
            catch (Exception ex)
            {
                score.OutputOk = false;
                score.OutputError = ex.Message;
            }
            finally
            {
                File.Delete(tmp);
                score.WriteSeconds = writeClock.Elapsed.TotalSeconds;
            }
        }
        scores.Add(score);
        if (withGliner)
        {
            // With GLiNER: its solo finds are left in the text and flagged ("auto"), or, if a reviewer accepted every flag, redacted.
            var auto = Scoring.Score(d.Doc.Text, Redactor.Apply(d.Doc.Text, combined, o.Redaction.PlaceholderTemplate), d.Truth, d.Group);
            var accepted = Scoring.Score(d.Doc.Text, Redactor.Apply(d.Doc.Text, combined.Select(s => s.Source == "gliner-only" ? s with { Flag = false } : s), o.Redaction.PlaceholderTemplate), d.Truth, d.Group);
            var key = Scoring.KeySpans(d.Doc.Text, d.Truth);
            var reviewed = Scoring.Score(d.Doc.Text, Redactor.Apply(d.Doc.Text, combined.Select(s => s.Source == "gliner-only" && Scoring.OverlapsKey(key, s.Start, s.Length) ? s with { Flag = false } : s), o.Redaction.PlaceholderTemplate), d.Truth, d.Group);
            foreach (var (name, v) in new[] { (variantAuto, auto), (variantAccepted, accepted), (variantReviewed, reviewed) })
            {
                v.File = d.Name;
                v.Model = name;
                v.DetectSeconds = sw.Elapsed.TotalSeconds;
                v.GlinerSeconds = detector.LastGlinerSeconds;
                v.PromptTokens = score.PromptTokens;
                v.OutputTokens = score.OutputTokens;
                v.FlagsRaised = accepted.Edits - auto.Edits;
                v.FlagsCorrect = accepted.TruePositives - auto.TruePositives;
                scores.Add(v);
            }

            Console.Write($"[+GLiNER: caught {auto.Caught}/{auto.Present}, {auto.FalsePositives.Count} over, {accepted.Edits - auto.Edits} flagged ({accepted.TruePositives - auto.TruePositives} right)] ");
        }

        Console.WriteLine($"caught {score.Caught}/{score.Present}, {score.FalsePositives.Count} over, {score.DetectSeconds:0.0}s{(score.OutputOk is null ? "" : $" + {score.WriteSeconds:0.0}s writing")}{(score.OutputOk == false ? " (output refused)" : "")}");
    }
    Console.WriteLine($"  {model} finished in {modelClock.Elapsed:hh\\:mm\\:ss}");
    await UnloadAsync(o.Llm, model);   // free the memory so the next model starts cold and the timings are comparable
    Save();
}
Console.WriteLine($"\nReport: {outPath}");
return 0;

/// <summary>A span reduced to positions only, for saving.</summary>
static SavedSpan ToSaved(DetectedEntity e) => new(e.Type, e.Start, e.Length, e.Confidence, e.Source, e.Flag);

/// <summary>The Ollama server version, for the report (null when it cannot be read).</summary>
static async Task<string?> OllamaVersionAsync(LlmOptions llm)
{
    try
    {
        using var http = OllamaDetector.CreateClient(llm);
        return (await http.GetFromJsonAsync<JsonElement>("/api/version")).GetProperty("version").GetString();
    }
    catch (Exception)
    {
        return null;
    }
}

/// <summary>Asks Ollama to unload a model now (a zero keep-alive); a failure only means the model stays loaded a little longer.</summary>
static async Task UnloadAsync(LlmOptions llm, string model)
{
    try
    {
        using var http = OllamaDetector.CreateClient(llm);
        using var _ = await http.PostAsJsonAsync("/api/generate", new { model, keep_alive = 0 });
    }
    catch (Exception)
    {
    }
}
