// Command-line runner: redacts every document in a folder using the local model.
// Usage: dotnet run --project src/AiDocumentRedactor.Cli -- --config redactor.config.json [--input dir] [--output dir] [--model name] [--dry-run]
using System.Text.Json;
using AiDocumentRedactor.Core;
using AiDocumentRedactor.Detection;
using AiDocumentRedactor.Documents;
using AiDocumentRedactor.Ocr;

// Reads the value after a command-line flag, e.g. --config path. Returns null if the flag is absent.
string? Arg(string name)
{
    var i = Array.IndexOf(args, name);
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
}

// 1. Load the JSON config (the single source of settings). Flags below override it.
var configPath = Arg("--config") ?? "redactor.config.json";
RedactorOptions options;
try
{
    options = RedactorOptions.Load(configPath);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Config error: {ex.Message}");
    return 2;
}

if (Arg("--model") is { } modelOverride) options.Llm.Model = modelOverride;   // FR15
// 2. Work out the input/output folders and refuse unsafe combinations (output inside input).
var input = Path.GetFullPath(Arg("--input") ?? options.Input.Directory);
var output = Path.GetFullPath(Arg("--output") ?? options.Output.Directory);
options.Output.Directory = output;
var dryRun = args.Contains("--dry-run");

if (!Directory.Exists(input))
{
    Console.Error.WriteLine($"Input directory not found: {input}");
    return 2;
}

if (output == input || output.StartsWith(input + Path.DirectorySeparatorChar))
{
    Console.Error.WriteLine("Output directory must not be the input directory or inside it.");
    return 2;
}

// 3. Choose the detector: the local Ollama model if configured (checking it is reachable), else one that finds nothing.
IEntityDetector detector = new NoOpDetector();
if (options.Llm.Provider == "ollama")
{
    try
    {
        var od = new OllamaDetector(OllamaDetector.CreateClient(options.Llm), options);
        await od.CheckAvailableAsync(CancellationToken.None);
        detector = od;
        Console.WriteLine($"Model: {options.Llm.Model} at {options.Llm.Endpoint}");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex.Message);
        return 2;
    }
}
// 4. Build the pipeline (read -> detect -> redact -> write) and list the files to process.
using var ocr = options.Ocr.Enabled ? new RapidOcrEngine() : null;   // local OCR for scanned PDFs and images
var pipeline = new RedactionPipeline(DocumentFormats.Readers(options, ocr), DocumentFormats.Writers(options, ocr), detector, options);

var files = options.Input.Include
    .SelectMany(p => Directory.EnumerateFiles(input, p, options.Input.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
    .Distinct().Order().ToList();
Console.WriteLine($"{files.Count} file(s) found in {input}");

// 5. Process each file one at a time. A failure is recorded and the run continues with the next file.
var report = new List<object>(); int done = 0, failed = 0;
var store = new StatusStore(options.ReportDirectory);
var model = detector is OllamaDetector ? options.Llm.Model : null;
foreach (var f in files)
{
    var target = pipeline.OutputPathFor(f, input);
    if (!options.Output.Overwrite && File.Exists(target)) { report.Add(new { file = Path.GetFileName(f), status = "skipped-exists" }); continue; }
    try
    {
        var progress = new Progress<RedactionProgress>(p => Console.WriteLine($"  [{p.Stage}] {p.Message}"));
        Console.WriteLine(Path.GetRelativePath(input, f));
        var r = await pipeline.RunAsync(f, dryRun ? null : target, progress, CancellationToken.None);
        var rel = Path.GetRelativePath(input, f);
        if (!dryRun) store.Set(rel, new DocumentStatusRecord(r.Edits.Count == 0 && model is not null ? DocumentStatus.NeedsReview : DocumentStatus.Processed, null, r.Edits.Count, DateTime.UtcNow, model));
        report.Add(new { file = Path.GetFileName(f), status = "done", edits = r.Edits.Count, byType = r.Edits.GroupBy(e => e.Type).ToDictionary(g => g.Key, g => g.Count()) });
        done++;
    }
    catch (Exception ex)
    {
        if (!dryRun) store.Set(Path.GetRelativePath(input, f), new DocumentStatusRecord(DocumentStatus.Error, ex.Message, 0, DateTime.UtcNow, model));
        report.Add(new { file = Path.GetFileName(f), status = "failed", error = ex.GetType().Name }); failed++;
    }
}
// 6. Write the run report (counts only, never document text) and print a summary.
if (!dryRun)
{
    Directory.CreateDirectory(options.ReportDirectory);
    File.WriteAllText(Path.Combine(options.ReportDirectory, "report.json"), JsonSerializer.Serialize(report, RedactorOptions.JsonOptions));
}
if (detector is OllamaDetector o) Console.WriteLine($"Tokens: {o.PromptTokens} in / {o.OutputTokens} out; discarded non-verbatim: {o.Discarded}");
Console.WriteLine($"Done: {done}, failed: {failed}");
return failed > 0 ? 1 : 0;
