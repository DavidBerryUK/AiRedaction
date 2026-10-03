using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AiDocumentRedactor.Core;
using AiDocumentRedactor.Detection;
using AiDocumentRedactor.Documents;
using AiDocumentRedactor.Explorer.Dataset;
using AiDocumentRedactor.Ocr;

namespace AiDocumentRedactor.Eval;

/// <summary>Makes an interim dataset from saved runs (the .scores.json or .scores.json.gz files). No model is run: every detector's spans are taken from the saved run
/// (the combinations with GLiNER are rebuilt from them), the documents are read again so the text matches, and everything is scored against the answer keys as they are now.
/// The result is labelled interim: it is test data for the explorer, and the final dataset comes from the evaluator.</summary>
public static class InterimConverter
{
    public static readonly string[] Baselines = ["rules only", "GLiNER only", "rules + GLiNER"];
    const string WithGliner = " + GLiNER", AllAccepted = " (all flags accepted)", CorrectAccepted = " (correct flags accepted)";
    static readonly Regex Skip = new(@"^`(?<file>.+?)` with (?<model>.+?): (?<message>.*)$", RegexOptions.Singleline | RegexOptions.CultureInvariant);

    /// <summary>How a saved row's name splits into the Ollama model (empty for the no-model baselines) and the variant.</summary>
    public static (string Model, string Variant) ParseConfig(string name)
    {
        if (Baselines.Contains(name))
        {
            return (string.Empty, "plain");
        }

        if (name.EndsWith(WithGliner + AllAccepted, StringComparison.Ordinal))
        {
            return (name[..^(WithGliner.Length + AllAccepted.Length)], "with-gliner-all-flags-accepted");
        }

        if (name.EndsWith(WithGliner + CorrectAccepted, StringComparison.Ordinal))
        {
            return (name[..^(WithGliner.Length + CorrectAccepted.Length)], "with-gliner-correct-flags-accepted");
        }

        return name.EndsWith(WithGliner, StringComparison.Ordinal) ? (name[..^WithGliner.Length], "with-gliner") : (name, "plain");
    }

    /// <summary>The corpus a saved run was made on, from the name of its corpus folder.</summary>
    public static string CorpusName(string corpusDir) => Path.GetFileName(corpusDir.TrimEnd('/', '\\')).ToLowerInvariant() switch
    {
        "heldoutcorpus" => "heldout",
        "testcorpus" => "formats",
        "externalgretelcorpus" => "external-gretel",
        "externalnemotroncorpus" => "external-nemotron",
        var other => Regex.Replace(other, "[^a-z0-9]+", "-").Trim('-'),
    };

    /// <summary>The path of a corpus file relative to the input folder. The held-out documents sit in <c>in/finance-sample</c>, each external corpus in the <c>in</c> folder named after it, the others in the same folders as in the corpus.</summary>
    public static string DocId(string corpus, string relativeToCorpus)
    {
        var rel = relativeToCorpus.Replace('\\', '/');
        if (rel.StartsWith("text/", StringComparison.Ordinal))
        {
            if (corpus == "heldout")
            {
                return "finance-sample/" + rel["text/".Length..];
            }

            if (corpus.StartsWith("external-", StringComparison.Ordinal))
            {
                return corpus + "/" + rel["text/".Length..];
            }
        }

        return rel;
    }

    /// <summary>Converts the saved runs and writes the dataset. Returns the exit code.</summary>
    public static async Task<int> RunAsync(IReadOnlyList<string> savedPaths, string inputRoot, string corpusRoot, string outDir, string datasetId, string? note)
    {
        var data = new DatasetData();
        var warnings = new List<string>();
        var sources = new List<object>();
        var corpora = new List<object>();
        var modelList = new Dictionary<string, ModelInfo>();
        var elapsed = 0.0;
        var machines = new HashSet<string>();
        var ollama = new HashSet<string>();
        var seenKeys = new HashSet<string>();
        var driftRows = 0;
        var checkedRows = 0;
        GlinerOptions? glinerOptions = null;
        var runs = savedPaths.Select(p => (Path: p, Run: SavedRun.Load(p))).ToList();
        using var ocr = runs.Any(r => r.Run.Options.Ocr.Enabled) ? new RapidOcrEngine() : null;
        foreach (var (path, run) in runs)
        {
            var corpus = CorpusName(run.CorpusDir);
            var corpusDir = Path.GetFullPath(Path.Combine(corpusRoot, Path.GetFileName(run.CorpusDir.TrimEnd('/', '\\'))));
            if (!Directory.Exists(Path.Combine(corpusDir, "ground-truth")))
            {
                Console.Error.WriteLine($"No answer keys at {corpusDir} (the run {path} was made on {run.CorpusDir}). Use --corpus-root.");
                return 2;
            }

            var truth = GroundTruthStore.Load(corpusDir);
            var settings = ResultRows.Settings(run.Options);
            var settingsHash = ResultRows.SettingsHash(settings);
            glinerOptions ??= run.Options.Gliner;
            elapsed += run.ElapsedSeconds;
            machines.Add(run.Machine);
            if (run.OllamaVersion is not null)
            {
                ollama.Add(run.OllamaVersion);
            }

            foreach (var m in run.Models.Where(m => m.Info is not null && m.Info.Name == m.Model))
            {
                modelList[m.Model] = m.Info!;
            }

            var readers = DocumentFormats.Readers(run.Options, ocr);
            var template = run.Options.Redaction.PlaceholderTemplate;
            var docCount = 0;
            Console.WriteLine($"{Path.GetFileName(path)}: {run.Scores.Select(s => s.File).Distinct().Count()} documents on the {corpus} corpus");
            foreach (var group in run.Scores.GroupBy(s => s.File).OrderBy(g => g.Key, StringComparer.Ordinal))
            {
                var file = group.Key;
                var docPath = Path.Combine(corpusDir, file);
                var gt = GroundTruthStore.For(Path.GetFileName(file), truth);
                var reader = readers.FirstOrDefault(r => r.CanRead(docPath));
                if (gt is null || reader is null || !File.Exists(docPath))
                {
                    warnings.Add($"{file}: left out ({(gt is null ? "no answer key" : reader is null ? "no reader" : "file not found")}).");
                    continue;
                }

                string text;
                try
                {
                    text = (await reader.ReadAsync(docPath, CancellationToken.None)).Text;
                }
                catch (Exception ex)
                {
                    warnings.Add($"{file}: left out ({ex.Message}).");
                    continue;
                }

                var docId = DocId(corpus, file);
                var inputFile = Path.Combine(inputRoot, docId);
                if (!File.Exists(inputFile))
                {
                    warnings.Add($"{docId}: not found in the input folder {inputRoot}.");
                }
                else if (!SameBytes(await File.ReadAllBytesAsync(inputFile), await File.ReadAllBytesAsync(docPath)))
                {
                    warnings.Add($"{docId}: the file in the input folder differs from the corpus file the run used.");
                }

                var hash = DatasetWriter.HashText(text);
                data.Texts.Add(new DocumentText(docId, hash, text));
                data.Documents.Add(new DocumentRow(docId, gt.Id, corpus, GroundTruthStore.FormatGroup(docPath), gt.Title, text.Length, hash, gt.Entities.Count, gt.Entities.Sum(e => e.Occurrences)));
                if (seenKeys.Add(gt.Id))
                {
                    AddKey(data, gt);
                }

                docCount++;
                var rows = group.ToDictionary(r => r.Model);
                var glinerSpans = rows.TryGetValue("GLiNER only", out var g) ? g.Spans.Select(ToSpan).ToList() : [];
                var keySpans = Scoring.KeySpans(text, gt);
                foreach (var row in group)
                {
                    var (model, variant) = ParseConfig(row.Model);
                    List<DetectedEntity> spans;
                    if (variant == "plain")
                    {
                        spans = row.Spans.Select(ToSpan).ToList();
                    }
                    else if (rows.TryGetValue(model, out var baseRow))
                    {
                        var combined = AgreementCombiner.Combine(baseRow.Spans.Select(ToSpan).ToList(), glinerSpans, run.Options).ToList();
                        spans = variant switch
                        {
                            "with-gliner-all-flags-accepted" => combined.Select(s => s.Source == "gliner-only" ? s with { Flag = false } : s).ToList(),
                            "with-gliner-correct-flags-accepted" => combined.Select(s => s.Source == "gliner-only" && Scoring.OverlapsKey(keySpans, s.Start, s.Length) ? s with { Flag = false } : s).ToList(),
                            _ => combined,
                        };
                    }
                    else
                    {
                        warnings.Add($"{file}: '{row.Model}' has no row for '{model}' to build it from; left out.");
                        continue;
                    }

                    var built = ResultRows.Build(docId, row.Model, model, variant, text, spans, gt, GroundTruthStore.FormatGroup(docPath), template, settingsHash);
                    var score = built.Score;
                    if (variant != "with-gliner-correct-flags-accepted")
                    {
                        checkedRows++;
                    }

                    // The variant where a reviewer accepts the correct flags depends on the answer key, which was changed after the run, so only the others can be compared.
                    if (variant != "with-gliner-correct-flags-accepted" && (score.Edits + score.Unjudged != row.Edits + row.Unjudged || score.MustPreserve != row.MustPreserve))
                    {
                        driftRows++;
                        if (driftRows <= 10)
                        {
                            warnings.Add($"{file} / {row.Model}: rescoring found {score.Edits + score.Unjudged} redactions but the saved run has {row.Edits + row.Unjudged}; the text may differ from what the run saw.");
                        }
                    }

                    // Timings, token counts and the output check are carried over from the saved run.
                    var result = built.Result;
                    result.DetectSeconds = row.DetectSeconds;
                    result.GlinerSeconds = row.GlinerSeconds > 0 ? row.GlinerSeconds : null;
                    result.WriteSeconds = row.OutputOk is null ? null : row.WriteSeconds;
                    result.PromptTokens = model.Length > 0 ? row.PromptTokens : null;
                    result.OutputTokens = model.Length > 0 ? row.OutputTokens : null;
                    result.Discarded = model.Length > 0 ? row.Discarded : null;
                    result.OutputOk = row.OutputOk;
                    result.OutputError = row.OutputError ?? string.Empty;
                    data.Results.Add(result);
                    data.Spans.AddRange(built.Spans);
                    data.Outcomes.AddRange(built.Outcomes);
                }
            }

            // A model that failed on a document (a timeout, a server error) has no scores; its rows say so, instead of leaving a gap.
            foreach (var line in run.Skipped)
            {
                var m = Skip.Match(line);
                if (!m.Success)
                {
                    continue;
                }

                var docId = DocId(corpus, m.Groups["file"].Value);
                if (!data.Documents.Any(d => d.DocId == docId))
                {
                    continue;
                }

                var message = m.Groups["message"].Value.Trim();
                foreach (var config in run.Models.Select(x => x.Model).Where(c => ParseConfig(c).Model == m.Groups["model"].Value && !Baselines.Contains(c)))
                {
                    var (model, variant) = ParseConfig(config);
                    data.Results.Add(new ResultRow
                    {
                        ResultId = $"{docId}|{config}|1", DocId = docId, Config = config, Model = model, Variant = variant, SettingsHash = settingsHash,
                        Status = message.Contains("Timeout", StringComparison.OrdinalIgnoreCase) || message.Contains("timed out", StringComparison.OrdinalIgnoreCase) ? "timeout" : "error",
                        Error = message.Length > 300 ? message[..300] : message,
                    });
                }
            }

            sources.Add(new Dictionary<string, object?>
            {
                ["file"] = Path.GetFileName(path), ["corpus"] = corpus, ["started"] = run.Started, ["elapsedSeconds"] = Math.Round(run.ElapsedSeconds, 1),
                ["ollamaVersion"] = run.OllamaVersion, ["machine"] = run.Machine, ["settingsHash"] = settingsHash, ["settings"] = settings,
            });
            corpora.Add(KeyInfo(corpus, corpusDir, docCount));
        }

        if (data.Documents.Count == 0)
        {
            Console.Error.WriteLine("No documents could be converted.");
            return 2;
        }

        data.Run = new Dictionary<string, object?>
        {
            ["formatVersion"] = Schema.FormatVersion,
            ["datasetId"] = datasetId,
            ["status"] = "interim",
            ["createdAt"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            ["elapsedSeconds"] = Math.Round(elapsed, 1),
            ["gitCommit"] = null,
            ["gitDirty"] = null,
            ["ollamaVersion"] = string.Join(", ", ollama.Order()),
            ["machine"] = string.Join("; ", machines.Order()),
            ["machineBusy"] = null,
            ["repeats"] = 1,
            ["models"] = modelList.Values.OrderBy(m => m.Name, StringComparer.Ordinal).Select(m => new Dictionary<string, object?>
            {
                ["name"] = m.Name, ["digest"] = m.Digest, ["parameterSize"] = m.ParameterSize, ["quantization"] = m.Quantization, ["family"] = m.Family, ["sizeBytes"] = m.SizeBytes,
            }).ToList(),
            ["gliner"] = glinerOptions is null ? null : new Dictionary<string, object?>
            {
                ["model"] = Path.GetFileName(glinerOptions.ModelDirectory.TrimEnd('/', '\\')), ["onnxFile"] = glinerOptions.OnnxFile, ["threshold"] = glinerOptions.Threshold,
                ["soloAction"] = glinerOptions.SoloAction, ["labels"] = glinerOptions.Labels,
            },
            ["corpora"] = corpora,
            ["sources"] = sources,
            ["notes"] = (note ?? string.Empty) + (note is null ? string.Empty : " ") + "Interim dataset: made from saved runs by the converter, with no model run. Spans come from the saved runs; the combinations with GLiNER were rebuilt from them; "
                + "everything was scored again against the answer keys as they are now. Timings, token counts and output checks are as saved. The git commit of the code that made the spans is not recorded. Not for quoting as final results.",
            ["warnings"] = warnings.Count,
        };
        await DatasetWriter.WriteAsync(outDir, data);
        Console.WriteLine($"Wrote {outDir}: {data.Documents.Count} documents, {data.Results.Count} results, {data.Spans.Count} spans, {data.Outcomes.Count} outcomes, {data.Entities.Count} key rows.");
        Console.WriteLine($"Checked {checkedRows} rescored rows against the saved runs; {driftRows} had a different number of redactions.");
        foreach (var w in warnings)
        {
            Console.WriteLine($"  warning: {w}");
        }

        var problems = await DatasetValidator.ValidateAsync(outDir);
        foreach (var p in problems)
        {
            Console.WriteLine($"  invalid: {p}");
        }

        Console.WriteLine(problems.Count == 0 ? "The dataset passes the validator." : $"The dataset has {problems.Count} problem(s).");
        return problems.Count == 0 ? 0 : 1;
    }

    static bool SameBytes(byte[] a, byte[] b) => a.AsSpan().SequenceEqual(b);

    static DetectedEntity ToSpan(SavedSpan s) => new(s.Type, s.Start, s.Length, s.Confidence, s.Source, s.Flag);

    /// <summary>Which answer key scored the corpus: its version, a checksum of the key files, and a note.</summary>
    public static Dictionary<string, object?> KeyInfo(string corpus, string corpusDir, int documents)
    {
        var files = Directory.GetFiles(Path.Combine(corpusDir, "ground-truth"), "*.json");
        var audited = Directory.Exists(Path.Combine(corpusDir, "ground-truth-original"));
        return new Dictionary<string, object?>
        {
            ["corpus"] = corpus, ["documentCount"] = documents, ["keyFiles"] = files.Length,
            ["keyVersion"] = audited ? "audited" : "hand-written",
            ["keyChecksum"] = ResultRows.KeyChecksum(corpusDir),
            ["keyNote"] = audited
                ? "The generator's key corrected by a rule-based audit (items added and removed, judged categories, ignore list); the original is kept in ground-truth-original. See tests/HeldOutCorpus/AUDIT.md."
                : "Written by hand with the test documents.",
        };
    }

    /// <summary>Adds an answer key's items, must-keep texts and undecidable texts to a dataset's entity rows.</summary>
    public static void AddKey(DatasetData data, GroundTruth gt)
    {
        for (var i = 0; i < gt.Entities.Count; i++)
        {
            var e = gt.Entities[i];
            data.Entities.Add(new EntityRow($"{gt.Id}#{i + 1}", gt.Id, "entity", e.Type, e.Text, e.Occurrences, e.Where, e.Audit == "added" ? "added" : "original", gt.Judges(e.Type)));
        }

        var n = 0;
        foreach (var keep in gt.MustPreserve ?? [])
        {
            data.Entities.Add(new EntityRow($"{gt.Id}#keep{++n}", gt.Id, "must_preserve", string.Empty, keep, null, string.Empty, string.Empty, null));
        }

        n = 0;
        foreach (var ignore in gt.Ignore ?? [])
        {
            data.Entities.Add(new EntityRow($"{gt.Id}#ignore{++n}", gt.Id, "ignore", string.Empty, ignore, null, string.Empty, string.Empty, null));
        }
    }
}
