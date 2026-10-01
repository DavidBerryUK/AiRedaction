// Evaluation harness: runs each model over the synthetic test corpus, scores the result against the answer key and writes a Markdown report.
// Usage: dotnet run --project src/AiDocumentRedactor.Eval -- [--config redactor.config.json] [--corpus tests/TestCorpus] [--models phi4,gemma3:27b]
//        [--out eval/eval-<time>.md] [--only text] [--no-write] [--show-text]
// It is a command, not part of the app: a full run takes a long time (every model over every document).
using System.Diagnostics;
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
    Console.WriteLine("Evaluates local models on the test corpus and writes a Markdown report.\n\n  --config <file>   settings (default redactor.config.json)\n  --corpus <dir>    test corpus (default tests/TestCorpus)\n  --models a,b      models to compare (default: those with include: true in the config's evaluation.models list)\n  --out <file>      report path (default eval/eval-<date-time>.md; a .json with the raw scores is written beside it)\n  --only <text>     only corpus files whose path contains this text\n  --no-write        score the text only; skip writing and verifying the redacted files (faster)\n  --show-text       list missed and over-redacted strings in the report (synthetic data only)");
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
var scores = new List<DocScore>();
var modelInfos = new List<(string, ModelInfo?)>();

/// <summary>Writes the report and the raw scores, so a long run leaves usable results after every model.</summary>
void Save()
{
    var run = new RunInfo(started, clock.Elapsed, machine, corpus, docs.Count, docs.Select(d => d.Truth).DistinctBy(g => g.Id).Sum(g => g.Entities.Count), options, modelInfos, showText, write, skipped);
    File.WriteAllText(outPath, MarkdownReport.Build(run, scores));
    File.WriteAllText(Path.ChangeExtension(outPath, ".json"), MarkdownReport.Json(run, scores));
}

foreach (var model in wanted)
{
    modelInfos.Add((model, installed.First(i => i.Name == model)));
    var o = JsonSerializer.Deserialize<RedactorOptions>(JsonSerializer.Serialize(options, RedactorOptions.JsonOptions), RedactorOptions.JsonOptions)!;
    o.Llm.Model = model;
    var detector = new OllamaDetector(OllamaDetector.CreateClient(o.Llm), o);
    await detector.CheckAvailableAsync(CancellationToken.None);
    Console.WriteLine($"\n=== {model} ===");
    var modelClock = Stopwatch.StartNew();   // every document is run with this model before the next model is loaded
    foreach (var d in docs)
    {
        long p0 = detector.PromptTokens, o0 = detector.OutputTokens;
        var x0 = detector.Discarded;
        var sw = Stopwatch.StartNew();
        RedactionResult result;
        try
        {
            result = Redactor.Apply(d.Doc.Text, await detector.DetectAsync(d.Doc.Text, null, CancellationToken.None), o.Redaction.PlaceholderTemplate);
        }
        catch (Exception ex)
        {
            skipped.Add($"`{d.Name}` with {model}: {ex.Message}");
            Console.WriteLine($"  {d.Name}: FAILED {ex.Message}");
            continue;
        }
        sw.Stop();
        var score = Scoring.Score(d.Doc.Text, result, d.Truth, d.Group);
        score.File = d.Name;
        score.Model = model;
        score.DetectSeconds = sw.Elapsed.TotalSeconds;
        score.PromptTokens = detector.PromptTokens - p0;
        score.OutputTokens = detector.OutputTokens - o0;
        score.Discarded = detector.Discarded - x0;
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
        Console.WriteLine($"  {d.Name}: caught {score.Caught}/{score.Present}, {score.FalsePositives.Count} over, {score.DetectSeconds:0.0}s{(score.OutputOk is null ? "" : $" + {score.WriteSeconds:0.0}s writing")}{(score.OutputOk == false ? " (output refused)" : "")}");
    }
    Console.WriteLine($"  {model} finished in {modelClock.Elapsed:hh\\:mm\\:ss}");
    Save();
}
Console.WriteLine($"\nReport: {outPath}");
return 0;
