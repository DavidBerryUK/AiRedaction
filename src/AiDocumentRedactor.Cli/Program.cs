// Command-line runner: redacts every document in a folder using the local model.
// Usage: dotnet run --project src/AiDocumentRedactor.Cli -- --config redactor.config.json [--input dir] [--output dir] [--model name] [--dry-run]
using System.Text.Json;
using AiDocumentRedactor.Core;
using AiDocumentRedactor.Detection;
using AiDocumentRedactor.Documents;
using AiDocumentRedactor.Ocr;

// 1. Load the JSON config (the single source of settings). Flags below override it.
var options = ConfigurationLoader.LoadConfiguration(args);
if (options == null) return 2;

// 2. Validate input/output paths
var (input, output, isValid) = InputOutputValidator.ValidatePaths(args, options);
if (!isValid) return 2;
options.Output.Directory = output;

var dryRun = args.Contains("--dry-run");

// 3. Build the pipeline
var (detector, pipeline) = PipelineBuilder.BuildPipeline(options);
if (detector == null || pipeline == null) return 2;

// 4. List the files to process.
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
    if (!options.Output.Overwrite && File.Exists(target))
    {
        report.Add(new { file = Path.GetFileName(f), status = "skipped-exists" });
        continue;
    }
    try
    {
        var progress = new Progress<RedactionProgress>(p => Console.WriteLine($"  [{p.Stage}] {p.Message}"));
        Console.WriteLine(Path.GetRelativePath(input, f));
        var r = await pipeline.RunAsync(f, dryRun ? null : target, progress, CancellationToken.None);
        var rel = Path.GetRelativePath(input, f);
        if (!dryRun)
            store.Set(rel, new DocumentStatusRecord(r.Edits.Count == 0 && model is not null ? DocumentStatus.NeedsReview : DocumentStatus.Processed, null, r.Edits.Count, DateTime.UtcNow, model));
        report.Add(new { file = Path.GetFileName(f), status = "done", edits = r.Edits.Count, byType = r.Edits.GroupBy(e => e.Type).ToDictionary(g => g.Key, g => g.Count()) });
        done++;
    }
    catch (Exception ex)
    {
        if (!dryRun)
            store.Set(Path.GetRelativePath(input, f), new DocumentStatusRecord(DocumentStatus.Error, ex.Message, 0, DateTime.UtcNow, model));
        report.Add(new { file = Path.GetFileName(f), status = "failed", error = ex.GetType().Name });
        failed++;
    }
}
// 6. Write the run report (counts only, never document text) and print a summary.
if (!dryRun)
{
    Directory.CreateDirectory(options.ReportDirectory);
    File.WriteAllText(Path.Combine(options.ReportDirectory, "report.json"), JsonSerializer.Serialize(report, RedactorOptions.JsonOptions));
}
if (detector is OllamaDetector o)
    Console.WriteLine($"Tokens: {o.PromptTokens} in / {o.OutputTokens} out; discarded non-verbatim: {o.Discarded}");
Console.WriteLine($"Done: {done}, failed: {failed}");
return failed > 0 ? 1 : 0;