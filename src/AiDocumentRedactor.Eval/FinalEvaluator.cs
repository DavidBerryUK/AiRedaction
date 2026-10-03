using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.Json;
using AiDocumentRedactor.Core;
using AiDocumentRedactor.Detection;
using AiDocumentRedactor.Documents;
using AiDocumentRedactor.Explorer;
using AiDocumentRedactor.Explorer.Dataset;
using AiDocumentRedactor.Ocr;

namespace AiDocumentRedactor.Eval;

/// <summary>What the final evaluation is asked to do.</summary>
public record FinalOptions(RedactorOptions Options, string InputRoot, string CorpusRoot, string OutDir, string DatasetId, IReadOnlyList<string> Models, IReadOnlyList<string>? OnlyDocs,
    bool AllowDirty, bool WriteOutputs, int TimeoutSeconds);

/// <summary>The final evaluation: every chosen model over every document of every corpus, with the rules and GLiNER, written straight into a dataset (the format in
/// documentation/RESULTS_DATASET_FORMAT.md). Each document's rows are saved as soon as they are scored, so a run that stops can be started again and carries on where it
/// left off. It records the code version and refuses to run on uncommitted code, and it checks the finished dataset for anything missing.</summary>
public static class FinalEvaluator
{
    const string Gl = " + GLiNER", Accepted = " (all flags accepted)", Reviewed = " (correct flags accepted)";

    /// <summary>The setups a model produces on each document, in order.</summary>
    public static string[] ConfigsFor(string model) => [model, model + Gl, model + Gl + Accepted, model + Gl + Reviewed];

    /// <summary>A document to evaluate: where it is, its answer key, and its text as the detectors will see it.</summary>
    sealed record Doc(string DocId, string Corpus, string CorpusDir, string File, GroundTruth Key, string Format, ExtractedDocument Extracted, string Hash);

    /// <summary>The current commit and whether there are uncommitted changes (tracked or new files); null when git is not available.</summary>
    public static async Task<(string Commit, bool Dirty)?> GitStateAsync()
    {
        try
        {
            var commit = (await RunAsync("git", "rev-parse --short=12 HEAD")).Trim();
            var dirty = (await RunAsync("git", "status --porcelain")).Trim().Length > 0;
            return commit.Length == 0 ? null : (commit, dirty);
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            return null;
        }
    }

    static async Task<string> RunAsync(string file, string args)
    {
        using var p = Process.Start(new ProcessStartInfo(file, args) { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false }) ?? throw new InvalidOperationException(file);
        var output = await p.StandardOutput.ReadToEndAsync();
        await p.WaitForExitAsync();
        return p.ExitCode == 0 ? output : string.Empty;
    }

    /// <summary>The one-minute load average, or null where the system does not report it. A high load while evaluating means timings are unreliable.</summary>
    public static async Task<double?> LoadAverageAsync()
    {
        try
        {
            var text = File.Exists("/proc/loadavg") ? await File.ReadAllTextAsync("/proc/loadavg") : await RunAsync("sysctl", "-n vm.loadavg");
            var first = text.Replace("{", " ", StringComparison.Ordinal).Replace("}", " ", StringComparison.Ordinal).Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            return double.TryParse(first, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : null;
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException or IOException)
        {
            return null;
        }
    }

    /// <summary>Runs (or resumes) the evaluation. Returns the exit code: 0 when the dataset is complete and valid, 1 when something is missing or invalid, 2 when it could not start.</summary>
    public static async Task<int> RunAsync(FinalOptions f)
    {
        var options = f.Options;
        options.Gliner.Enabled = true;
        options.Llm.TimeoutSeconds = Math.Max(options.Llm.TimeoutSeconds, f.TimeoutSeconds);
        var template = options.Redaction.PlaceholderTemplate;

        // ---- before anything else: the things that would spoil the run ----
        var git = await GitStateAsync();
        if (git is null)
        {
            Console.Error.WriteLine("Cannot read the git state. The final evaluation records the code version, so it must run inside the repository.");
            return 2;
        }

        if (git.Value.Dirty && !f.AllowDirty)
        {
            Console.Error.WriteLine("There are uncommitted changes. Commit them first, so the dataset can be tied to exact code (or add --allow-dirty for a trial that is marked as not final).");
            return 2;
        }

        Uri endpoint = new(options.Llm.Endpoint);
        if (!endpoint.IsLoopback && !options.Llm.AllowRemoteEndpoint)
        {
            Console.Error.WriteLine($"The Ollama endpoint {endpoint} is not on this computer. The evaluation uses local models only.");
            return 2;
        }

        List<ModelInfo> installed;
        try
        {
            installed = (await new OllamaModelCatalog(OllamaDetector.CreateClient(options.Llm)).ListAsync(CancellationToken.None)).ToList();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            Console.Error.WriteLine($"Cannot reach Ollama at {options.Llm.Endpoint}: {ex.Message}");
            return 2;
        }

        var missing = f.Models.Where(m => installed.All(i => i.Name != m && i.Name != m + ":latest")).ToList();
        if (missing.Count > 0)
        {
            Console.Error.WriteLine($"Not installed: {string.Join(", ", missing)}. Run: ollama pull <name>");
            return 2;
        }

        GlinerDetector gliner;
        try
        {
            gliner = GlinerDetector.Create(options) ?? throw new FileNotFoundException("GLiNER is switched off.");
        }
        catch (FileNotFoundException ex)
        {
            Console.Error.WriteLine($"The GLiNER model files are missing ({options.Gliner.ModelDirectory}): {ex.Message}");
            return 2;
        }

        // ---- the documents: every file of every corpus, read from the input folder, each with its answer key ----
        using var ocr = options.Ocr.Enabled ? new RapidOcrEngine() : null;
        var readers = DocumentFormats.Readers(options, ocr);
        var writers = DocumentFormats.Writers(options, ocr);
        var docs = new List<Doc>();
        var problems = new List<string>();
        var corpora = new List<(string Name, string Dir, int Count)>();
        foreach (var dir in Directory.GetDirectories(f.CorpusRoot).Where(d => Directory.Exists(Path.Combine(d, "ground-truth"))).Order(StringComparer.Ordinal))
        {
            var corpus = InterimConverter.CorpusName(dir);
            var truth = GroundTruthStore.Load(dir);
            var count = 0;
            foreach (var file in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
            {
                var rel = Path.GetRelativePath(dir, file).Replace('\\', '/');
                if (rel.StartsWith("ground-truth", StringComparison.Ordinal) || rel is "README.md" or "AUDIT.md")
                {
                    continue;
                }

                var docId = InterimConverter.DocId(corpus, rel);
                if (f.OnlyDocs is not null && !f.OnlyDocs.Contains(docId))
                {
                    continue;
                }

                var key = GroundTruthStore.For(Path.GetFileName(file), truth);
                var input = Path.Combine(f.InputRoot, docId);
                var reader = readers.FirstOrDefault(r => r.CanRead(input));
                if (key is null || reader is null || !File.Exists(input))
                {
                    problems.Add($"{docId}: {(key is null ? "no answer key" : reader is null ? "no reader" : $"not found in {f.InputRoot}")}");
                    continue;
                }

                count++;
                docs.Add(new Doc(docId, corpus, dir, input, key, GroundTruthStore.FormatGroup(input), default!, string.Empty));
            }

            corpora.Add((corpus, dir, count));
        }

        if (problems.Count > 0)
        {
            problems.ForEach(p => Console.Error.WriteLine($"  problem: {p}"));
            Console.Error.WriteLine("Some documents cannot be evaluated. Fix these first so that nothing is silently left out.");
            return 2;
        }

        if (docs.Count == 0)
        {
            Console.Error.WriteLine("No documents to evaluate.");
            return 2;
        }

        Console.WriteLine($"Reading {docs.Count} documents (scans are read with OCR, which takes a few minutes)…");
        var read = new List<Doc>();
        var readClock = Stopwatch.StartNew();
        foreach (var d in docs)
        {
            try
            {
                var extracted = await readers.First(r => r.CanRead(d.File)).ReadAsync(d.File, CancellationToken.None);
                read.Add(d with { Extracted = extracted, Hash = DatasetWriter.HashText(extracted.Text) });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"  problem: {d.DocId} could not be read: {ex.Message}");
                return 2;
            }
        }

        docs = read;
        Console.WriteLine($"  read in {readClock.Elapsed:mm\\:ss}.");

        // ---- the dataset folder: the fixed files first, then rows as they are scored ----
        var settingsHash = ResultRows.SettingsHash(ResultRows.Settings(options));
        var resume = File.Exists(Path.Combine(f.OutDir, Schema.ResultsFile));
        var state = await StartAsync(f, docs, git.Value, resume, settingsHash);
        if (state is null)
        {
            return 2;
        }

        var prior = state.TryGetValue("elapsedSeconds", out var priorValue) ? priorValue is JsonElement { ValueKind: JsonValueKind.Number } pe ? pe.GetDouble() : priorValue is double pd ? pd : 0 : 0;
        var done = await LoadDoneAsync(f.OutDir);
        var ollama = await OllamaVersionAsync(options.Llm);
        var loads = new List<double>();
        var load = await LoadAverageAsync();
        var cores = Environment.ProcessorCount;
        if (load is { } l0)
        {
            loads.Add(l0);
            if (l0 > cores / 3.0)
            {
                Console.WriteLine($"  warning: the machine is busy (load {l0:0.0} on {cores} cores). Timings will be unreliable; stop other work for a clean run.");
            }
        }

        var clock = Stopwatch.StartNew();
        var processed = 0;
        var failures = 0;

        // ---- no-model baselines: rules only, GLiNER only, rules + GLiNER ----
        Console.WriteLine($"\n=== Baselines without a language model ===");
        foreach (var d in docs)
        {
            processed++;
            if (InterimConverter.Baselines.All(b => done.Contains($"{d.DocId}|{b}|1")))
            {
                continue;
            }

            var text = d.Extracted.Text;
            var ruleClock = Stopwatch.StartNew();
            var rules = RuleDetector.Find(text, options);
            var ruleSeconds = ruleClock.Elapsed.TotalSeconds;
            var glinerClock = Stopwatch.StartNew();
            var gl = gliner.Detect(text);
            var glinerSeconds = glinerClock.Elapsed.TotalSeconds;
            var group = new List<BuiltResult>();
            foreach (var (name, spans, seconds, g) in new[] { ("rules only", rules, ruleSeconds, 0.0), ("GLiNER only", gl.ToList(), glinerSeconds, glinerSeconds), ("rules + GLiNER", rules.Concat(gl).ToList(), ruleSeconds + glinerSeconds, glinerSeconds) })
            {
                var built = ResultRows.Build(d.DocId, name, string.Empty, "plain", text, spans, d.Key, d.Format, template, settingsHash);
                built.Result.DetectSeconds = seconds;
                built.Result.GlinerSeconds = g > 0 ? g : null;
                group.Add(built);
            }

            await SaveGroupAsync(f.OutDir, group);
            done.UnionWith(group.Select(b => b.Result.ResultId));
            if (processed % 50 == 0)
            {
                Console.WriteLine($"  baselines: {processed} of {docs.Count} documents");
            }
        }

        // ---- the models, one at a time, so each is loaded once and the timings are comparable ----
        var modelNumber = 0;
        foreach (var model in f.Models)
        {
            modelNumber++;
            var info = installed.FirstOrDefault(i => i.Name == model || i.Name == model + ":latest")!;
            var o = JsonSerializer.Deserialize<RedactorOptions>(JsonSerializer.Serialize(options, RedactorOptions.JsonOptions), RedactorOptions.JsonOptions)!;
            o.Llm.Model = model;
            var detector = new OllamaDetector(OllamaDetector.CreateClient(o.Llm), o, GlinerDetector.Create(o));
            await detector.CheckAvailableAsync(CancellationToken.None);
            var configs = ConfigsFor(model);
            var pending = docs.Count(d => !configs.All(c => done.Contains($"{d.DocId}|{c}|1")));
            Console.WriteLine($"\n=== Model {modelNumber} of {f.Models.Count}: {model} ({pending} of {docs.Count} documents to do) ===");
            var modelClock = Stopwatch.StartNew();
            var number = 0;
            var finished = 0;
            foreach (var d in docs)
            {
                number++;
                processed++;
                if (configs.All(c => done.Contains($"{d.DocId}|{c}|1")))
                {
                    continue;
                }

                var text = d.Extracted.Text;
                Console.Write($"  [{model} {modelNumber}/{f.Models.Count}] document {number} of {docs.Count}: {d.DocId} ... ");
                long p0 = detector.PromptTokens, t0 = detector.OutputTokens;
                var x0 = detector.Discarded;
                var sw = Stopwatch.StartNew();
                IReadOnlyList<DetectedEntity> combined;
                try
                {
                    combined = await detector.DetectAsync(text, null, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    // A timeout or a failure is recorded, not dropped: every setup of this model gets a row saying why.
                    failures++;
                    var message = ex.Message.Length > 300 ? ex.Message[..300] : ex.Message;
                    var status = ex is TaskCanceledException || ex.Message.Contains("Timeout", StringComparison.OrdinalIgnoreCase) ? "timeout" : "error";
                    var failed = configs.Select(c =>
                    {
                        var (m, v) = InterimConverter.ParseConfig(c);
                        return new BuiltResult(new ResultRow { ResultId = $"{d.DocId}|{c}|1", DocId = d.DocId, Config = c, Model = m, Variant = v, Status = status, Error = message, SettingsHash = settingsHash }, [], [], new DocScore());
                    }).ToList();
                    await SaveGroupAsync(f.OutDir, failed);
                    done.UnionWith(failed.Select(b => b.Result.ResultId));
                    finished++;
                    Console.WriteLine($"FAILED ({status}): {message}");
                    continue;
                }

                sw.Stop();
                var primary = detector.LastPrimarySpans;
                var glinerSeconds = detector.LastGlinerSeconds;
                var key = Scoring.KeySpans(text, d.Key);
                var variants = new (string Config, string Variant, IReadOnlyList<DetectedEntity> Spans)[]
                {
                    (configs[0], "plain", primary),
                    (configs[1], "with-gliner", combined),
                    (configs[2], "with-gliner-all-flags-accepted", combined.Select(s => s.Source == "gliner-only" ? s with { Flag = false } : s).ToList()),
                    (configs[3], "with-gliner-correct-flags-accepted", combined.Select(s => s.Source == "gliner-only" && Scoring.OverlapsKey(key, s.Start, s.Length) ? s with { Flag = false } : s).ToList()),
                };
                var group = new List<BuiltResult>();
                foreach (var (config, variant, spans) in variants)
                {
                    var built = ResultRows.Build(d.DocId, config, model, variant, text, spans, d.Key, d.Format, template, settingsHash);
                    var plain = variant == "plain";
                    built.Result.DetectSeconds = plain ? sw.Elapsed.TotalSeconds - glinerSeconds : sw.Elapsed.TotalSeconds;
                    built.Result.GlinerSeconds = plain || glinerSeconds <= 0 ? null : glinerSeconds;
                    built.Result.PromptTokens = detector.PromptTokens - p0;
                    built.Result.OutputTokens = detector.OutputTokens - t0;
                    built.Result.Discarded = detector.Discarded - x0;
                    group.Add(built);
                }

                // The redacted file is written and checked, as the application would: no recoverable text in a PDF, nothing hidden in a Word file, a scan read again by OCR.
                var plainRow = group[0].Result;
                if (f.WriteOutputs && writers.FirstOrDefault(w => w.CanWrite(d.Extracted)) is { } writer)
                {
                    var tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + Path.GetExtension(d.File));
                    var writeClock = Stopwatch.StartNew();
                    try
                    {
                        await writer.WriteAsync(d.Extracted, Redactor.Apply(text, primary, template), tmp, CancellationToken.None);
                        plainRow.OutputOk = true;
                    }
                    catch (Exception ex)
                    {
                        plainRow.OutputOk = false;
                        plainRow.OutputError = ex.Message.Length > 300 ? ex.Message[..300] : ex.Message;
                    }
                    finally
                    {
                        File.Delete(tmp);
                        plainRow.WriteSeconds = writeClock.Elapsed.TotalSeconds;
                    }
                }

                await SaveGroupAsync(f.OutDir, group);
                done.UnionWith(group.Select(b => b.Result.ResultId));
                var s0 = group[0].Score;
                finished++;
                var eta = TimeSpan.FromSeconds(modelClock.Elapsed.TotalSeconds / finished * Math.Max(0, pending - finished));
                Console.WriteLine($"caught {s0.Caught}/{s0.Present}, {s0.FalsePositives.Count} over, {plainRow.DetectSeconds:0.0}s (about {eta:hh\\:mm\\:ss} left for this model)");
            }

            Console.WriteLine($"  {model} finished in {modelClock.Elapsed:hh\\:mm\\:ss}");
            await UnloadAsync(o.Llm, model);   // free the memory so the next model starts cold
            if (await LoadAverageAsync() is { } l)
            {
                loads.Add(l);
            }

            state["models"] = Models(f, installed);
            await PersistAsync(f.OutDir, state, prior + clock.Elapsed.TotalSeconds, ollama, loads, cores, failures);
        }

        // ---- finish: record, check, and make the database and the summary ----
        state["models"] = Models(f, installed);
        await PersistAsync(f.OutDir, state, prior + clock.Elapsed.TotalSeconds, ollama, loads, cores, failures);
        return await FinishAsync(f, docs, state, git.Value);
    }

    static List<object> Models(FinalOptions f, List<ModelInfo> installed) => f.Models.Select(m => installed.FirstOrDefault(i => i.Name == m || i.Name == m + ":latest")).Where(i => i is not null).Select(i => (object)new Dictionary<string, object?>
    {
        ["name"] = i!.Name, ["digest"] = i.Digest, ["parameterSize"] = i.ParameterSize, ["quantization"] = i.Quantization, ["family"] = i.Family, ["sizeBytes"] = i.SizeBytes,
    }).ToList();

    /// <summary>Writes the fixed files of a new dataset (or checks that a dataset being resumed is the same one) and returns the run's record.</summary>
    static async Task<Dictionary<string, object?>?> StartAsync(FinalOptions f, List<Doc> docs, (string Commit, bool Dirty) git, bool resume, string settingsHash)
    {
        var runPath = Path.Combine(f.OutDir, Schema.RunFile);
        if (resume)
        {
            if (!File.Exists(runPath))
            {
                Console.Error.WriteLine($"{f.OutDir} has results but no run.json; it cannot be resumed. Use a new --id.");
                return null;
            }

            var existing = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(await File.ReadAllTextAsync(runPath))!;
            var commit = existing.TryGetValue("gitCommit", out var c) && c.ValueKind == JsonValueKind.String ? c.GetString() : null;
            if (commit != git.Commit)
            {
                Console.Error.WriteLine($"The dataset was started on code version {commit} but the code is now {git.Commit}. A dataset must come from one version of the code, so it cannot be resumed. Use a new --id to start again.");
                return null;
            }

            var (_, rows) = await Csv.ReadAsync(Path.Combine(f.OutDir, Schema.DocumentsFile));
            var stored = rows.ToDictionary(r => r["doc_id"], r => r["text_hash"]);
            var changed = docs.Where(d => !stored.TryGetValue(d.DocId, out var h) || h != d.Hash).Select(d => d.DocId).ToList();
            if (changed.Count > 0 || stored.Count != docs.Count)
            {
                Console.Error.WriteLine($"The documents are not the same as when this dataset was started ({changed.Count} differ, {stored.Count} stored against {docs.Count} now). It cannot be resumed.");
                return null;
            }

            await RemoveIncompleteAsync(f.OutDir);
            Console.WriteLine($"Resuming {f.OutDir}.");
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(await File.ReadAllTextAsync(runPath))!;
        }

        Directory.CreateDirectory(f.OutDir);
        var data = new DatasetData();
        var seenKeys = new HashSet<string>();
        foreach (var d in docs)
        {
            data.Texts.Add(new DocumentText(d.DocId, d.Hash, d.Extracted.Text));
            data.Documents.Add(new DocumentRow(d.DocId, d.Key.Id, d.Corpus, d.Format, d.Key.Title, d.Extracted.Text.Length, d.Hash, d.Key.Entities.Count, d.Key.Entities.Sum(e => e.Occurrences)));
            if (seenKeys.Add(d.Key.Id))
            {
                InterimConverter.AddKey(data, d.Key);
            }
        }

        await DatasetWriter.WriteAsync(f.OutDir, data);
        foreach (var file in new[] { Schema.ResultsFile, Schema.SpansFile, Schema.OutcomesFile })
        {
            await Csv.WriteAsync(Path.Combine(f.OutDir, file), Schema.Files[file], []);   // headers only; rows are added as documents are scored
        }

        var options = f.Options;
        var settings = ResultRows.Settings(options);
        var run = new Dictionary<string, object?>
        {
            ["formatVersion"] = Schema.FormatVersion,
            ["datasetId"] = f.DatasetId,
            ["status"] = "running",
            ["createdAt"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            ["gitCommit"] = git.Commit,
            ["gitDirty"] = git.Dirty,
            ["repeats"] = 1,
            ["models"] = new List<object>(),
            ["gliner"] = new Dictionary<string, object?>
            {
                ["model"] = Path.GetFileName(options.Gliner.ModelDirectory.TrimEnd('/', '\\')), ["onnxFile"] = options.Gliner.OnnxFile, ["threshold"] = options.Gliner.Threshold,
                ["soloAction"] = options.Gliner.SoloAction, ["labels"] = options.Gliner.Labels,
            },
            ["corpora"] = docs.GroupBy(d => d.Corpus).Select(g => (object)InterimConverter.KeyInfo(g.Key, g.First().CorpusDir, g.Count())).ToList(),
            ["sources"] = new List<object>
            {
                new Dictionary<string, object?> { ["file"] = "final evaluator", ["corpus"] = "all", ["settingsHash"] = settingsHash, ["settings"] = settings },
            },
            ["notes"] = "Made by the final evaluator: every chosen model over every document, one pass, the rules and GLiNER with each, on one version of the code. The redacted file for each result was written and checked.",
        };
        await File.WriteAllTextAsync(runPath, JsonSerializer.Serialize(run, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        return run;
    }

    /// <summary>The ids of results already saved as finished. Failed results are removed so that resuming tries them again.</summary>
    static async Task<HashSet<string>> LoadDoneAsync(string dir)
    {
        var (_, rows) = await Csv.ReadAsync(Path.Combine(dir, Schema.ResultsFile));
        return rows.Where(r => r["status"] == "ok").Select(r => r["result_id"]).ToHashSet();
    }

    /// <summary>Tidies a dataset that stopped part-way: drops failed results (so they are tried again) and any spans or outcomes whose result row was never written.</summary>
    static async Task RemoveIncompleteAsync(string dir)
    {
        var resultsPath = Path.Combine(dir, Schema.ResultsFile);
        var (resultColumns, results) = await Csv.ReadAsync(resultsPath);
        var keep = results.Where(r => r["status"] == "ok").ToList();
        var ids = keep.Select(r => r["result_id"]).ToHashSet();
        var dropped = results.Count - keep.Count;
        if (dropped > 0)
        {
            await Csv.WriteAsync(resultsPath, Schema.Results, keep.Select(r => Schema.Results.Select(c => r.GetValueOrDefault(c.Name)).ToArray()));
        }

        foreach (var file in new[] { Schema.SpansFile, Schema.OutcomesFile })
        {
            var path = Path.Combine(dir, file);
            var (_, rows) = await Csv.ReadAsync(path);
            var kept = rows.Where(r => ids.Contains(r["result_id"])).ToList();
            if (kept.Count != rows.Count)
            {
                Console.WriteLine($"  removed {rows.Count - kept.Count} unfinished rows from {file}");
                await Csv.WriteAsync(path, Schema.Files[file], kept.Select(r => Schema.Files[file].Select(c => r.GetValueOrDefault(c.Name)).ToArray()));
            }
        }

        if (dropped > 0)
        {
            Console.WriteLine($"  {dropped} failed results will be tried again.");
        }
    }

    /// <summary>Saves one document's rows for one detector setup group. The spans and outcomes go first and the result row last, so a result row always has its spans and outcomes.</summary>
    static async Task SaveGroupAsync(string dir, List<BuiltResult> group)
    {
        await Csv.AppendAsync(Path.Combine(dir, Schema.SpansFile), Schema.Spans, group.SelectMany(g => g.Spans).Select(s => s.Cells()));
        await Csv.AppendAsync(Path.Combine(dir, Schema.OutcomesFile), Schema.Outcomes, group.SelectMany(g => g.Outcomes).Select(o => o.Cells()));
        await Csv.AppendAsync(Path.Combine(dir, Schema.ResultsFile), Schema.Results, group.Select(g => g.Result.Cells()));
    }

    /// <summary>Writes run.json with the totals so far (the status stays "running" until the dataset is checked complete).</summary>
    static async Task PersistAsync(string dir, Dictionary<string, object?> state, double elapsedSeconds, string? ollama, List<double> loads, int cores, int failures)
    {
        state["elapsedSeconds"] = Math.Round(elapsedSeconds, 1);
        state["ollamaVersion"] = ollama;
        state["machine"] = $"{RuntimeInformation.OSDescription}, {cores} cores";
        state["loadAverages"] = loads.Select(l => Math.Round(l, 2)).ToList();
        state["machineBusy"] = loads.Count == 0 ? null : loads.Any(l => l > cores / 3.0);
        state["failedModelRuns"] = failures;
        await File.WriteAllTextAsync(Path.Combine(dir, Schema.RunFile), JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true }) + "\n");
    }

    /// <summary>Checks the finished dataset for anything missing, marks it final if it is complete, builds the database and writes a short report.</summary>
    static async Task<int> FinishAsync(FinalOptions f, List<Doc> docs, Dictionary<string, object?> state, (string Commit, bool Dirty) git)
    {
        Console.WriteLine("\n=== Checking the dataset ===");
        var problems = await DatasetValidator.ValidateAsync(f.OutDir);
        problems.AddRange(await CompletenessAsync(f.OutDir, docs, f.Models, f.WriteOutputs));
        problems.ForEach(p => Console.WriteLine($"  PROBLEM: {p}"));
        var runPath = Path.Combine(f.OutDir, Schema.RunFile);
        var run = JsonSerializer.Deserialize<Dictionary<string, object?>>(await File.ReadAllTextAsync(runPath))!;
        run["status"] = problems.Count == 0 && !git.Dirty ? "final" : "draft";
        run["finishedAt"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        run["warnings"] = problems.Count;
        await File.WriteAllTextAsync(runPath, JsonSerializer.Serialize(run, new JsonSerializerOptions { WriteIndented = true }) + "\n");
        if (problems.Count > 0)
        {
            Console.WriteLine($"The dataset has {problems.Count} problem(s) and is marked draft. Fix them (a resume will redo what is missing) and run again.");
            return 1;
        }

        var db = await ResultsDatabase.EnsureAsync(f.OutDir);
        var service = new ExplorerService(db);
        await File.WriteAllTextAsync(Path.Combine(f.OutDir, "report.md"), await ReportAsync(service, run));
        Console.WriteLine($"The dataset is complete and valid ({docs.Count} documents). Marked {run["status"]}.\n  Dataset: {f.OutDir}\n  Database: {db}\n  Report: {Path.Combine(f.OutDir, "report.md")}");
        return 0;
    }

    /// <summary>What the dataset must hold, listed and checked: a result for every document under every setup, scored and complete, with the output checks recorded.</summary>
    static async Task<List<string>> CompletenessAsync(string dir, List<Doc> docs, IReadOnlyList<string> models, bool outputsChecked)
    {
        var problems = new List<string>();
        var (_, results) = await Csv.ReadAsync(Path.Combine(dir, Schema.ResultsFile));
        var byId = results.ToDictionary(r => r["result_id"]);
        var spans = (await Csv.ReadAsync(Path.Combine(dir, Schema.SpansFile))).Rows.Select(r => r["result_id"]).ToHashSet();
        var factCounts = new Dictionary<string, (int Caught, int Missed)>();
        foreach (var o in (await Csv.ReadAsync(Path.Combine(dir, Schema.OutcomesFile))).Rows.Where(o => o["kind"] is "caught" or "missed"))
        {
            var c = factCounts.GetValueOrDefault(o["result_id"]);
            factCounts[o["result_id"]] = o["kind"] == "caught" ? (c.Caught + 1, c.Missed) : (c.Caught, c.Missed + 1);
        }

        var configs = InterimConverter.Baselines.Concat(models.SelectMany(ConfigsFor)).ToList();
        var missing = 0;
        var notOk = 0;
        var incomplete = new List<string>();
        foreach (var d in docs)
        {
            foreach (var config in configs)
            {
                if (!byId.TryGetValue($"{d.DocId}|{config}|1", out var r))
                {
                    missing++;
                    continue;
                }

                if (r["status"] != "ok")
                {
                    notOk++;
                    continue;
                }

                var (model, variant) = InterimConverter.ParseConfig(config);
                if (r["settings_hash"].Length == 0 || r["detect_seconds"].Length == 0 || r["present"].Length == 0 || r["edits"].Length == 0 || r["true_positives"].Length == 0 || r["preserve_total"].Length == 0)
                {
                    incomplete.Add($"{r["result_id"]}: a score or timing is missing");
                }

                if (model.Length > 0 && (r["prompt_tokens"].Length == 0 || r["output_tokens"].Length == 0))
                {
                    incomplete.Add($"{r["result_id"]}: token counts are missing");
                }

                if (variant != "plain" && (r["flags_raised"].Length == 0 || r["gliner_seconds"].Length == 0))
                {
                    incomplete.Add($"{r["result_id"]}: the GLiNER figures are missing");
                }

                if (outputsChecked && variant == "plain" && model.Length > 0 && r["output_ok"].Length == 0)
                {
                    incomplete.Add($"{r["result_id"]}: the redacted-file check was not recorded");
                }

                if (r["output_ok"] == "false")
                {
                    incomplete.Add($"{r["result_id"]}: the redacted file FAILED its safety check ({r["output_error"]})");
                }

                if (int.TryParse(r["present"], out var present) && factCounts.GetValueOrDefault(r["result_id"]) is var fc && fc.Caught + fc.Missed != present)
                {
                    incomplete.Add($"{r["result_id"]}: the item-by-item facts do not add up to the totals");
                }

                if ((r["edits"] != "0" || r["unjudged"] != "0") && !spans.Contains(r["result_id"]))
                {
                    incomplete.Add($"{r["result_id"]}: it made redactions but no spans were saved");
                }
            }
        }

        if (missing > 0)
        {
            problems.Add($"{missing} results are missing (of {docs.Count * configs.Count} expected). Run again with the same --id to carry on.");
        }

        if (notOk > 0)
        {
            problems.Add($"{notOk} results failed (a timeout or an error). Run again with the same --id to try them again.");
        }

        problems.AddRange(incomplete.Take(20));
        if (incomplete.Count > 20)
        {
            problems.Add($"…and {incomplete.Count - 20} more incomplete results.");
        }

        return problems;
    }

    static async Task<string> ReportAsync(ExplorerService service, Dictionary<string, object?> run)
    {
        var info = await service.InfoAsync();
        var rows = await service.SummaryAsync(new Filter(Variants: ["plain"]));
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"# Final evaluation: {info.DatasetId}").AppendLine();
        sb.AppendLine($"Code version `{info.GitCommit}`, {info.Documents} documents, {info.Results} results, answer keys: {info.KeyVersions}. Ollama {info.OllamaVersion}. {info.Machine}.").AppendLine();
        sb.AppendLine("Each model alone (with the fixed rules). Recall: how much of what should be removed was removed. Precision: how much of what was removed should have been. Full detail, the GLiNER variants and every document are in the results explorer.").AppendLine();
        sb.AppendLine("| Setup | Documents | Recall (95% range) | Precision (95% range) | F1 | Missed | Over-redactions | Seconds per document |");
        sb.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|");
        foreach (var r in rows)
        {
            sb.AppendLine($"| {r.Config}{(r.IsBaseline ? " (no model)" : string.Empty)} | {r.Documents}{(r.Failed > 0 ? $" ({r.Failed} failed)" : string.Empty)} | {r.Recall * 100:0.0}% ({r.RecallRange.Low * 100:0.0}–{r.RecallRange.High * 100:0.0}) | {r.Precision * 100:0.0}% ({r.PrecisionRange.Low * 100:0.0}–{r.PrecisionRange.High * 100:0.0}) | {r.F1 * 100:0.0}% | {r.Missed} | {r.OverRedactions} | {(r.AvgDetectSeconds is { } s ? s.ToString("0.0", CultureInfo.InvariantCulture) : "–")} |");
        }

        if (run.TryGetValue("machineBusy", out var busy) && busy is JsonElement { ValueKind: JsonValueKind.True })
        {
            sb.AppendLine().AppendLine("**The machine was busy during this run, so the timings are not reliable.**");
        }

        return sb.ToString();
    }

    static async Task<string?> OllamaVersionAsync(LlmOptions llm)
    {
        try
        {
            using var http = OllamaDetector.CreateClient(llm);
            return (await http.GetFromJsonAsync<JsonElement>("/api/version")).GetProperty("version").GetString();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            return null;
        }
    }

    static async Task UnloadAsync(LlmOptions llm, string model)
    {
        try
        {
            using var http = OllamaDetector.CreateClient(llm);
            using var _ = await http.PostAsJsonAsync("/api/generate", new { model, keep_alive = 0 });
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // The model simply stays loaded a little longer.
        }
    }
}
