using System.Diagnostics;
using AiDocumentRedactor.Core;
using AiDocumentRedactor.Detection;
using AiDocumentRedactor.Documents;
using AiDocumentRedactor.Explorer;
using AiDocumentRedactor.Explorer.Dataset;

namespace AiDocumentRedactor.Eval;

/// <summary>What a detector found in one document, with the timings and token counts of finding it.</summary>
public record LiveDetection(IReadOnlyList<DetectedEntity> Primary, IReadOnlyList<DetectedEntity> Combined, double DetectSeconds, double GlinerSeconds, long PromptTokens, long OutputTokens, int Discarded);

/// <summary>The detector a live run uses. The real one is Ollama (with the rules and, optionally, GLiNER); tests give it a fake.</summary>
public interface ILiveDetector
{
    Task<LiveDetection> DetectAsync(string text, IProgress<RedactionProgress>? progress, CancellationToken ct);
}

/// <summary>The real detector: the redaction pipeline's own Ollama detector, so a live result is what the application would produce.</summary>
public sealed class OllamaLiveDetector(OllamaDetector detector) : ILiveDetector
{
    public async Task<LiveDetection> DetectAsync(string text, IProgress<RedactionProgress>? progress, CancellationToken ct)
    {
        await detector.CheckAvailableAsync(ct);
        long p0 = detector.PromptTokens, o0 = detector.OutputTokens;
        var d0 = detector.Discarded;
        var clock = Stopwatch.StartNew();
        var combined = await detector.DetectAsync(text, progress, ct);
        clock.Stop();
        return new LiveDetection(detector.LastPrimarySpans, combined, clock.Elapsed.TotalSeconds - detector.LastGlinerSeconds, detector.LastGlinerSeconds,
            detector.PromptTokens - p0, detector.OutputTokens - o0, detector.Discarded - d0);
    }
}

/// <summary>Runs one dataset document through the real pipeline with a chosen local model and adds the scored result to the dataset's live results (see the explorer's
/// live folder). It scores with the same code as the evaluation, so a live row is comparable with a batch row made with the same settings. Only one run goes at a time.</summary>
public class LiveRunner(RedactorOptions options, string inputRoot, string corpusRoot, ExplorerCatalog catalog, IOcrEngineFactory? ocr = null, Func<RedactorOptions, ILiveDetector>? detectorFactory = null,
    Func<CancellationToken, Task<IReadOnlyList<LiveModel>>>? modelLister = null) : ILiveRunner
{
    readonly SemaphoreSlim gate = new(1, 1);

    /// <inheritdoc />
    public bool GlinerAvailable
    {
        get
        {
            var o = Clone(options);
            o.Gliner.Enabled = true;
            return File.Exists(Path.Combine(o.Gliner.ModelDirectory, o.Gliner.OnnxFile));
        }
    }

    static RedactorOptions Clone(RedactorOptions o) => System.Text.Json.JsonSerializer.Deserialize<RedactorOptions>(System.Text.Json.JsonSerializer.Serialize(o, RedactorOptions.JsonOptions), RedactorOptions.JsonOptions)!;

    /// <inheritdoc />
    public async Task<IReadOnlyList<LiveModel>> ModelsAsync(CancellationToken ct)
    {
        if (modelLister is not null)
        {
            return await modelLister(ct);
        }

        try
        {
            var installed = await new OllamaModelCatalog(OllamaDetector.CreateClient(options.Llm)).ListAsync(ct);
            return installed.Select(m => new LiveModel(m.Name, m.Summary)).ToList();
        }
        catch (HttpRequestException ex)
        {
            throw new LiveRunException($"Cannot reach Ollama at {options.Llm.Endpoint}: {ex.Message}");
        }
    }

    /// <summary>The folder of the corpus a dataset's documents belong to, found by name under the corpus root.</summary>
    string? CorpusDirectory(string corpus) => Directory.Exists(corpusRoot)
        ? Directory.GetDirectories(corpusRoot).FirstOrDefault(d => InterimConverter.CorpusName(d) == corpus && Directory.Exists(Path.Combine(d, "ground-truth")))
        : null;

    /// <inheritdoc />
    public async Task<LiveRunResult> RunAsync(string datasetId, string docId, string model, bool withGliner, IProgress<string>? progress, CancellationToken ct)
    {
        if (!await gate.WaitAsync(0, CancellationToken.None))
        {
            throw new LiveRunException("Another live run is in progress. Wait for it to finish, or cancel it.");
        }

        try
        {
            var service = await catalog.OpenAsync(datasetId) ?? throw new LiveRunException($"There is no dataset called '{datasetId}'.");
            var doc = await service.DocumentAsync(docId) ?? throw new LiveRunException($"There is no document '{docId}' in this dataset.");
            var datasetDir = Path.Combine(catalog.DatasetsDirectory, datasetId);
            var warnings = new List<string>();

            // The answer key: the same files the evaluation used, and a warning if they have changed since the dataset was made.
            var corpusDir = CorpusDirectory(doc.Corpus) ?? throw new LiveRunException($"The answer keys for the '{doc.Corpus}' corpus were not found under {corpusRoot}.");
            var gt = GroundTruthStore.Load(corpusDir).FirstOrDefault(g => g.Id == doc.KeyId) ?? throw new LiveRunException($"The answer key '{doc.KeyId}' was not found in {corpusDir}.");
            if (await service.KeyChecksumAsync(doc.Corpus) is { } recorded && recorded != ResultRows.KeyChecksum(corpusDir))
            {
                warnings.Add("The answer key files have changed since this dataset was made, so this result is scored against a different key from the batch results.");
            }

            // The document: read from the input folder, and it must read to the same text the dataset's spans refer to.
            var path = Path.GetFullPath(Path.Combine(inputRoot, docId));
            if (!path.StartsWith(Path.GetFullPath(inputRoot), StringComparison.Ordinal) || !File.Exists(path))
            {
                throw new LiveRunException($"The document '{docId}' is not in the input folder ({inputRoot}).");
            }

            progress?.Report("Reading the document…");
            var readers = DocumentFormats.Readers(options, ocr?.Create());
            var reader = readers.FirstOrDefault(r => r.CanRead(path)) ?? throw new LiveRunException($"There is no reader for '{docId}'.");
            string text;
            try
            {
                text = (await reader.ReadAsync(path, ct)).Text;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new LiveRunException($"The document could not be read: {ex.Message}");
            }

            if (text != doc.Text)
            {
                throw new LiveRunException("The copy of this document in the input folder reads differently from the one the dataset was scored on, so a live run would not line up with the text shown. Refresh the input copy from the corpus and try again.");
            }

            // The run, with the settings the application uses now.
            var o = Clone(options);
            o.Llm.Model = model;
            o.Gliner.Enabled = withGliner;
            var comparable = Clone(options);
            comparable.Gliner.Enabled = true;   // the batch runs had GLiNER switched on
            var settings = ResultRows.Settings(comparable);
            var settingsHash = ResultRows.SettingsHash(settings);
            if (!(await service.SettingsHashesAsync()).Contains(settingsHash))
            {
                warnings.Add("The settings now differ from those the batch results were made with (for example the categories, the rules or the model options), so the numbers may not be directly comparable.");
            }

            warnings.Add("Timings depend on what else the machine is doing, and the model may need loading first.");
            var number = await service.NextLiveNumberAsync(docId, model);
            ILiveDetector detector;
            try
            {
                detector = detectorFactory?.Invoke(o) ?? new OllamaLiveDetector(new OllamaDetector(OllamaDetector.CreateClient(o.Llm), o, GlinerDetector.Create(o)));
            }
            catch (FileNotFoundException ex)
            {
                throw new LiveRunException(ex.Message);
            }

            var template = o.Redaction.PlaceholderTemplate;
            var format = GroundTruthStore.FormatGroup(path);
            var rows = new DatasetData();
            var configs = new List<string>();
            var plainConfig = $"{model} (live {number})";
            var started = Stopwatch.StartNew();
            progress?.Report($"Running {model}…");
            var reporter = progress is null ? null : new Progress<RedactionProgress>(p => progress.Report(p.Message));
            LiveDetection found;
            try
            {
                found = await detector.DetectAsync(text, reporter, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                // A timeout or a failure is recorded like a batch failure, so the attempt is not lost.
                var timedOut = ex is OperationCanceledException || ex.Message.Contains("Timeout", StringComparison.OrdinalIgnoreCase);
                rows.Results.Add(new ResultRow
                {
                    ResultId = $"{docId}|{plainConfig}|1", DocId = docId, Config = plainConfig, Model = model, Variant = "plain", Source = "live", SettingsHash = settingsHash,
                    Status = timedOut ? "timeout" : "error", Error = ex.Message.Length > 300 ? ex.Message[..300] : ex.Message,
                });
                await SaveAsync(datasetDir, rows);
                throw new LiveRunException($"{model} did not finish: {ex.Message}");
            }

            void Add(string config, string variant, IReadOnlyList<DetectedEntity> spans, double detectSeconds, double glinerSeconds)
            {
                var built = ResultRows.Build(docId, config, model, variant, text, spans, gt, format, template, settingsHash, "live");
                built.Result.DetectSeconds = detectSeconds;
                built.Result.GlinerSeconds = glinerSeconds > 0 ? glinerSeconds : null;
                built.Result.PromptTokens = found.PromptTokens;
                built.Result.OutputTokens = found.OutputTokens;
                built.Result.Discarded = found.Discarded;
                rows.Results.Add(built.Result);
                rows.Spans.AddRange(built.Spans);
                rows.Outcomes.AddRange(built.Outcomes);
                configs.Add(config);
            }

            Add(plainConfig, "plain", found.Primary, found.DetectSeconds, 0);
            if (withGliner)
            {
                Add($"{model} + GLiNER (live {number})", "with-gliner", found.Combined, found.DetectSeconds + found.GlinerSeconds, found.GlinerSeconds);
            }

            progress?.Report("Saving the result…");
            await SaveAsync(datasetDir, rows);
            await ResultsDatabase.EnsureAsync(datasetDir);
            return new LiveRunResult(configs, warnings, started.Elapsed.TotalSeconds);
        }
        finally
        {
            gate.Release();
        }
    }

    /// <summary>Adds the rows to the dataset's live folder (results, spans and outcomes), and rebuilds the database if anything was written.</summary>
    static async Task SaveAsync(string datasetDir, DatasetData rows)
    {
        var live = Path.Combine(datasetDir, ResultsDatabase.LiveFolder);
        await Csv.AppendAsync(Path.Combine(live, Schema.ResultsFile), Schema.Results, rows.Results.Select(r => r.Cells()));
        await Csv.AppendAsync(Path.Combine(live, Schema.SpansFile), Schema.Spans, rows.Spans.Select(r => r.Cells()));
        await Csv.AppendAsync(Path.Combine(live, Schema.OutcomesFile), Schema.Outcomes, rows.Outcomes.Select(r => r.Cells()));
        await ResultsDatabase.EnsureAsync(datasetDir);
    }
}

/// <summary>Creates the OCR engine when a live run needs one. The web host owns the engine and shares it, so the factory just hands it over.</summary>
public interface IOcrEngineFactory
{
    IOcrEngine? Create();
}
