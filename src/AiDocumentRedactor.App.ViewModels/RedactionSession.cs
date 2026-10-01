using System.Text.Json;
using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.App.ViewModels;

/// <summary>Everything the UI shows and does: document list, selected document, per-model results,
/// running a redaction with progress, switching results, choosing the saved output. No UI framework.</summary>
public class RedactionSession
{
    readonly RedactorOptions options;
    readonly string inputRoot;
    readonly IEnumerable<IDocumentReader> readers;
    readonly IEnumerable<IDocumentWriter> writers;
    readonly IModelCatalog catalog;
    readonly Func<RedactorOptions, IEntityDetector> detectorFactory;
    /// <summary>Every model's result for each document this session (keyed by file path).</summary>
    readonly Dictionary<string, List<ModelResult>> results = new();
    /// <summary>Which result is the saved output for each document.</summary>
    readonly Dictionary<string, Guid> outputResult = new();
    readonly Dictionary<string, HashSet<string>> ranTogether = new();   // per document: models run together (they all vote)
    /// <summary>Lets the Cancel button stop a run in progress.</summary>
    CancellationTokenSource? cts;
    /// <summary>Where each document's review state (manual redactions, rejected edits) is kept, or null to keep it in memory only.</summary>
    readonly ReviewStore? reviewStore;
    /// <summary>The reviewer's changes per document, with undo/redo history and the state last written to the output file.</summary>
    readonly Dictionary<string, DocReview> reviews = new();

    /// <summary>One document's review: current state, undo and redo stacks, and the state the saved output reflects.</summary>
    sealed class DocReview
    {
        public ReviewState Current = ReviewState.Empty, Saved = ReviewState.Empty;
        public readonly Stack<ReviewState> Undo = new(), Redo = new();
    }

    /// <summary>Wires up the document list, readers/writers, model catalog and a factory that creates a detector per model.</summary>
    public RedactionSession(RedactorOptions options, string inputRoot, IEnumerable<IDocumentReader> readers,
        IEnumerable<IDocumentWriter> writers, IModelCatalog catalog, Func<RedactorOptions, IEntityDetector> detectorFactory, ReviewStore? reviewStore = null)
    {
        this.reviewStore = reviewStore;
        this.options = options; this.inputRoot = Path.GetFullPath(inputRoot); this.readers = readers; this.writers = writers;
        this.catalog = catalog; this.detectorFactory = detectorFactory;
        Documents = new DocumentListViewModel(options, inputRoot);
        SelectedModel = options.Llm.Model;
    }

    /// <summary>Raised whenever anything changes, so the UI can redraw.</summary>
    public event Action? Changed;
    /// <summary>Tells the UI something changed.</summary>
    void Notify() => Changed?.Invoke();

    /// <summary>The document list (search, sort, status).</summary>
    public DocumentListViewModel Documents { get; }
    /// <summary>The loaded configuration.</summary>
    public RedactorOptions Options => options;
    /// <summary>Path of the JSON config, so changes made in the UI can be written back (the file stays the source of truth).</summary>
    public string? ConfigPath { get; set; }
    /// <summary>Name of the OCR engine, shown in the banner (null when OCR is switched off).</summary>
    public string? OcrEngineName { get; set; }
    /// <summary>True while a document is being read (scans are read with OCR, which takes a few seconds).</summary>
    public bool IsReading { get; private set; }
    /// <summary>Average OCR confidence (0 to 1) of the selected document, or null if its text did not come from OCR.</summary>
    public double? OcrConfidence { get; private set; }
    /// <summary>True if the OCR confidence is below the configured minimum, so words may have been misread or missed.</summary>
    public bool LowOcrConfidence => OcrConfidence is { } c && c < options.Ocr.MinConfidence;

    // ---- models ----
    /// <summary>Models currently installed in Ollama.</summary>
    public IReadOnlyList<ModelInfo> Models { get; private set; } = [];
    /// <summary>Set when the model server cannot be reached.</summary>
    public string? ModelsError { get; private set; }
    /// <summary>Candidate models (config order) followed by any other installed models, each marked available or not.</summary>
    public IReadOnlyList<ModelChoice> Choices =>
        options.Llm.CandidateModels.Select(n => new ModelChoice(n, InfoFor(n)))
            .Concat(Models.Where(m => !options.Llm.CandidateModels.Contains(m.Name)).Select(m => new ModelChoice(m.Name, m)))
            .ToList();
    /// <summary>How many listed models are installed.</summary>
    public int AvailableCount => Choices.Count(c => c.Available);
    /// <summary>The model the next run will use (the picker's choice).</summary>
    public string SelectedModel { get; private set; }
    /// <summary>Details of an installed model, or null if it is not installed.</summary>
    public ModelInfo? InfoFor(string name) => Models.FirstOrDefault(m => m.Name == name);
    /// <summary>True if the model server is on this machine.</summary>
    public bool IsLoopback => Uri.TryCreate(options.Llm.Endpoint, UriKind.Absolute, out var u) && u.IsLoopback;
    /// <summary>Address of the model server.</summary>
    public string Endpoint => options.Llm.Endpoint;

    /// <summary>Asks Ollama which models are installed; also used by the re-check button.</summary>
    public async Task LoadModelsAsync()
    {
        try
        {
            Models = await catalog.ListAsync(CancellationToken.None); ModelsError = null;
            if (Models.Count > 0 && Models.All(m => m.Name != SelectedModel)) SelectedModel = Models.FirstOrDefault(m => m.Name == options.Llm.Model)?.Name ?? Models[0].Name;
        }
        catch (Exception ex) { ModelsError = $"Cannot reach Ollama at {options.Llm.Endpoint}: {ex.Message}"; }
        Notify();
    }

    /// <summary>Selects the model for the NEXT run (FR40). Never alters a result already shown.</summary>
    public void SelectModel(string name) { SelectedModel = name; Notify(); }

    // ---- confidence models (which models vote on each edit) ----
    /// <summary>Models ticked to vote on confidence.</summary>
    public IReadOnlyCollection<string> ConfidenceModels => options.Confidence.Models;
    /// <summary>True if this model is ticked as a confidence voter.</summary>
    public bool IsConfidenceModel(string name) => options.Confidence.Models.Contains(name);
    /// <summary>Ticks or unticks a confidence model and saves the choice to the config file.</summary>
    public void SetConfidenceModel(string name, bool on)
    {
        var set = options.Confidence.Models.ToList();
        if (on && !set.Contains(name)) set.Add(name); else if (!on) set.Remove(name);
        options.Confidence.Models = [.. set];
        if (ConfigPath is not null) ConfigFile.SaveConfidenceModels(ConfigPath, set);
        Notify();
    }
    /// <summary>The models the next Redact will run: the picker's model first, then each ticked, installed confidence model.</summary>
    public IReadOnlyList<string> PlannedModels =>
        new[] { SelectedModel }.Concat(options.Confidence.Models.Where(m => InfoFor(m) is not null)).Distinct().ToList();
    /// <summary>Which model of the run is executing (1-based).</summary>
    public int RunIndex { get; private set; }
    /// <summary>How many models the current run will execute.</summary>
    public int RunTotal { get; private set; }

    // ---- estimate and warnings before running ----
    /// <summary>How big the selected document is and roughly how long Redact will take, or null if no readable document is selected.
    /// Uses the speed measured on earlier runs of the planned models when there is one, otherwise a rough default. Adds a warning for large documents.</summary>
    public RunEstimate? Estimate()
    {
        if (OriginalText is null) return null;
        var pages = OriginalDoc?.PageSizes?.Count ?? 1; var chars = OriginalText.Length;
        var step = Math.Max(500, options.Llm.ChunkChars - options.Llm.ChunkOverlapChars);
        var chunks = Math.Max(1, (int)Math.Ceiling(Math.Max(0, chars - options.Llm.ChunkOverlapChars) / (double)step));
        var planned = PlannedModels; var measured = true; double perChunk = 0;
        foreach (var m in planned)
        {
            var past = results.Values.SelectMany(l => l).Where(r => r.Model == m && r.Calls.Count > 0).ToList();
            if (past.Count > 0) perChunk += past.Average(r => r.Elapsed.TotalSeconds / r.Calls.Count);
            else { perChunk += 8; measured = false; }
        }
        var seconds = chunks * perChunk;
        string? warning = pages > options.Ui.WarnPages || chars > options.Ui.WarnChars
            ? $"Large document ({pages} page{(pages == 1 ? "" : "s")}, {chars:N0} characters). Redacting it with {planned.Count} model{(planned.Count == 1 ? "" : "s")} may take about {new RunEstimate(pages, chars, chunks, planned.Count, seconds, measured, null).TimeText}."
            : null;
        return new RunEstimate(pages, chars, chunks, planned.Count, seconds, measured, warning);
    }

    // ---- prompt inspector ----
    /// <summary>What the model is told, built from the current config and the picker's model (for the Prompt tab).</summary>
    public PromptPreview Prompt()
    {
        var o = CloneFor(SelectedModel);
        var types = PromptBuilder.EnabledTypes(o).ToList();
        return new PromptPreview(SelectedModel, PromptBuilder.System(o), PromptBuilder.UserMessage("‹the chunk of document text goes here›"),
            JsonSerializer.Serialize(PromptBuilder.Schema(types), new JsonSerializerOptions { WriteIndented = true }),
            $"temperature {o.Llm.Temperature} · seed {o.Llm.Seed} · context {o.Llm.NumCtx} tokens · keep-alive {o.Llm.KeepAlive} · chunks of about {o.Llm.ChunkChars} characters · sent to {o.Llm.Endpoint}");
    }

    // ---- document selection ----
    /// <summary>Text of the selected document.</summary>
    public string? OriginalText { get; private set; }
    /// <summary>The selected document as read: text plus (for PDFs and images) the position and OCR confidence of every word.</summary>
    public ExtractedDocument? OriginalDoc { get; private set; }
    /// <summary>Why the selected document could not be read, if it could not.</summary>
    public string? LoadError { get; private set; }
    /// <summary>True when the selected file can be viewed (PDF, scan image) but not read for redaction yet.</summary>
    public bool PreviewOnly { get; private set; }
    /// <summary>Why a viewable document cannot be redacted (for example, a PDF with no text layer).</summary>
    public string? PreviewNote { get; private set; }
    /// <summary>The viewer URL for a document: served by the local host from the input folder, protected by the access token.</summary>
    public static string FileUrl(DocumentItem d) => "/files/" + string.Join('/', d.RelativePath.Replace('\\', '/').Split('/').Select(Uri.EscapeDataString));
    /// <summary>The selected document.</summary>
    public DocumentItem? Selected => Documents.Selected;

    /// <summary>Selects a document, reads its text and restores any results already produced for it.</summary>
    public async Task SelectAsync(DocumentItem? item)
    {
        Documents.Selected = item; OriginalText = null; OriginalDoc = null; LoadError = null; PreviewOnly = false; PreviewNote = null; OcrConfidence = null; ActiveResultId = null; LiveSpans = [];
        if (item is not null)
        {
            var reader = readers.FirstOrDefault(r => r.CanRead(item.FullPath));
            if (reader is null && item.CanPreview) PreviewOnly = true;   // viewable, but reading/redacting it is not built yet
            else if (reader is null) LoadError = $"Unsupported file type: .{item.Type.ToLowerInvariant()} (not supported yet)";
            else
                try
                {
                    IsReading = true; Notify();
                    var doc = await reader.ReadAsync(item.FullPath, CancellationToken.None);
                    if (Documents.Selected?.FullPath != item.FullPath) return;   // the user moved on while this was being read
                    OriginalText = doc.Text; OriginalDoc = doc; OcrConfidence = doc.OcrConfidence;
                }
                catch (NoTextLayerException ex) { PreviewOnly = true; PreviewNote = ex.Message; }   // a scanned PDF: view it, but it needs OCR
                catch (Exception ex) { LoadError = ex.Message; }
                finally { IsReading = false; }
            if (OriginalText is not null && !reviews.ContainsKey(item.FullPath) && reviewStore is not null)
                reviews[item.FullPath] = new DocReview { Current = reviewStore.Load(item.FullPath) };
            if (OriginalText is not null && !(results.TryGetValue(item.FullPath, out var any) && any.Count > 0) && !ReviewOf(item.FullPath).Current.IsEmpty)
                EnsureManualResult();   // a saved review from an earlier session: bring its edits back
            if (results.TryGetValue(item.FullPath, out var list) && list.Count > 0)
                ActiveResultId = outputResult.TryGetValue(item.FullPath, out var o) ? o : list[^1].Id;
        }
        Notify();
    }

    /// <summary>Rescans the source folder.</summary>
    public void Refresh() { Documents.Refresh(); Notify(); }
    /// <summary>Lets the UI ask for a redraw after it changes search or sort.</summary>
    public void NotifyChanged() => Notify();

    // ---- results ----
    /// <summary>All results (one per model) for the selected document.</summary>
    public IReadOnlyList<ModelResult> ResultsForSelected =>
        Selected is { } s && results.TryGetValue(s.FullPath, out var l) ? l : [];
    /// <summary>The result currently shown in the redacted pane.</summary>
    public Guid? ActiveResultId { get; private set; }
    /// <summary>The result currently shown.</summary>
    public ModelResult? ActiveResult => ResultsForSelected.FirstOrDefault(r => r.Id == ActiveResultId);
    /// <summary>The result saved as the output file.</summary>
    public Guid? OutputResultId => Selected is { } s && outputResult.TryGetValue(s.FullPath, out var o) ? o : null;
    /// <summary>Explains why a run did not save an output file.</summary>
    public string? SavedNote { get; private set; }

    /// <summary>Voters: results from the ticked confidence models, the models run together with the primary, and the active result.</summary>
    public IReadOnlyList<ModelResult> Voters => ResultsForSelected.Where(r => r.Id == ActiveResultId || options.Confidence.Models.Contains(r.Model)
        || (Selected is { } d && ranTogether.TryGetValue(d.FullPath, out var set) && set.Contains(r.Model))).ToList();
    /// <summary>Confidence of each edit of the active result.</summary>
    public Dictionary<int, EditConfidence> Confidence() => ActiveResult is { } a ? ConfidenceGrader.Grade(a, Voters, new ConfidenceContext(OcrConfidenceOf, options.Ocr.MinConfidence, options.Confidence.CategoryCaps)) : new();

    /// <summary>The lowest OCR confidence among the words an edit covers, or null if its text did not come from OCR.</summary>
    double? OcrConfidenceOf(RedactionEdit e) =>
        OriginalDoc?.Words?.Where(w => w.Confidence is not null && w.Start < e.OriginalStart + e.OriginalLength && w.Start + w.Length > e.OriginalStart).Select(w => w.Confidence).Min();
    /// <summary>How many edits of the active result are High, Medium and Low.</summary>
    public (int High, int Medium, int Low) ConfidenceCounts()
    {
        var c = Confidence().Values; return (c.Count(x => x.Level == ConfidenceLevel.High), c.Count(x => x.Level == ConfidenceLevel.Medium), c.Count(x => x.Level == ConfidenceLevel.Low));
    }

    /// <summary>Switches the redacted pane to another model's result.</summary>
    public void SelectResult(Guid id) { ActiveResultId = id; Notify(); }

    /// <summary>Builds the edit list for the active result; snippets come from the redacted text so they hold no sensitive data.</summary>
    public IReadOnlyList<Bookmark> Bookmarks()
    {
        var r = ActiveResult; if (r is null || OriginalText is null) return [];
        var red = r.Result.RedactedText;
        var conf = Confidence();
        return r.Result.Edits.Select(e =>
        {
            var from = Math.Max(0, e.RedactedStart - 24); var to = Math.Min(red.Length, e.RedactedStart + e.RedactedLength + 24);
            var snippet = red[from..to].Replace('\n', ' ').Replace('\r', ' ');
            var line = 1 + OriginalText.AsSpan(0, Math.Min(e.OriginalStart, OriginalText.Length)).Count('\n');
            return new Bookmark(e.Id, e.Type, (from > 0 ? "…" : "") + snippet + (to < red.Length ? "…" : ""), line, e.OriginalStart, e.RedactedStart,
                conf.GetValueOrDefault(e.Id) ?? new EditConfidence(ConfidenceLevel.Medium, 1, 1, e.Status == EditStatus.Flagged ? "Flagged for review. Left in the text, not redacted." : ""),
                e.Status == EditStatus.Flagged, e.Source, e.Status == EditStatus.Rejected);
        }).ToList();
    }

    /// <summary>The hand-drawn page rectangles of the viewed result as list entries (numbered from <see cref="Bookmark.AreaIdBase"/>).</summary>
    public IReadOnlyList<Bookmark> AreaBookmarks() =>
        ActiveResult?.Result.AreaList.Select((a, i) => new Bookmark(Bookmark.AreaIdBase + i, "AREA", $"Box drawn on page {a.Page + 1} (signature, logo, stamp or other non-text)", a.Page + 1, 0, 0,
            new EditConfidence(ConfidenceLevel.High, 1, 1, "Drawn by a person."), false, "human", false, true, a.Page + 1)).ToList() ?? [];

    // ---- running ----
    /// <summary>True while a redaction run is in progress.</summary>
    public bool IsRunning { get; private set; }
    /// <summary>The document being processed.</summary>
    public string? RunningPath { get; private set; }
    /// <summary>The model currently running.</summary>
    public string? RunningModel { get; private set; }
    /// <summary>Latest progress update.</summary>
    public RedactionProgress? Progress { get; private set; }
    /// <summary>Items found so far in the current run, shown as live highlights.</summary>
    public IReadOnlyList<DetectedEntity> LiveSpans { get; private set; } = [];
    /// <summary>Error or cancel message from the last run.</summary>
    public string? RunError { get; private set; }

    /// <summary>Progress reporter that calls the handler immediately (the built-in one posts to another thread).</summary>
    sealed class SyncProgress<T>(Action<T> h) : IProgress<T> { public void Report(T v) => h(v); }

    /// <summary>Copy of the config with a different model, so each run is independent of the picker.</summary>
    RedactorOptions CloneFor(string model)
    {
        var c = JsonSerializer.Deserialize<RedactorOptions>(JsonSerializer.Serialize(options, RedactorOptions.JsonOptions), RedactorOptions.JsonOptions)!;
        c.Llm.Model = model; return c;
    }

    /// <summary>True if pressing Redact would overwrite an existing result.</summary>
    public bool WillReplaceExisting => ResultsForSelected.Any(r => PlannedModels.Contains(r.Model));

    /// <summary>Runs the picker's model, then each ticked confidence model, one after another. Each produces its own result.</summary>
    public async Task RunAsync()
    {
        if (IsRunning || Selected is not { } doc || OriginalText is null || string.IsNullOrEmpty(SelectedModel)) return;
        var plan = PlannedModels.ToList(); var primary = SelectedModel;
        if (plan.Count > 1) ranTogether[doc.FullPath] = [.. plan];
        cts = new CancellationTokenSource(); IsRunning = true; RunningPath = doc.FullPath; RunTotal = plan.Count;
        LiveSpans = []; Progress = null; RunError = null; SavedNote = null;
        Documents.SetStatus(doc.FullPath, DocumentStatus.Processing);
        Notify();
        var failures = new List<string>(); var cancelled = false;
        try
        {
            for (var i = 0; i < plan.Count; i++)
            {
                RunIndex = i + 1; RunningModel = plan[i]; LiveSpans = []; Progress = null; Notify();
                try { await RunOneAsync(doc, plan[i], isPrimary: plan[i] == primary); }
                catch (OperationCanceledException) { cancelled = true; break; }
                catch (Exception ex) { failures.Add($"{plan[i]}: {ex.Message}"); }
            }
            var best = ResultsForSelected.FirstOrDefault(r => r.Id == ActiveResultId);
            if (cancelled) { Documents.SetStatus(doc.FullPath, DocumentStatus.Cancelled); RunError = "Cancelled"; }
            else if (best is null) { RunError = string.Join("; ", failures); Documents.SetStatus(doc.FullPath, DocumentStatus.Error, RunError); }
            else
            {
                if (failures.Count > 0) RunError = "Some models failed — " + string.Join("; ", failures);
                var saved = ResultsForSelected.FirstOrDefault(r => r.Id == OutputResultId) ?? best;
                Documents.SetStatus(doc.FullPath, saved.EditCount == 0 || LowOcrConfidence ? DocumentStatus.NeedsReview : DocumentStatus.Processed, null, saved.EditCount, saved.Model);
            }
        }
        finally { IsRunning = false; RunningPath = null; RunningModel = null; LiveSpans = []; RunIndex = RunTotal = 0; Notify(); }
    }

    /// <summary>Runs one model on one document, stores its result, and saves the output file if this is the first primary run.</summary>
    async Task RunOneAsync(DocumentItem doc, string model, bool isPrimary)
    {
        var opts = CloneFor(model);
        var detector = detectorFactory(opts);
        var pipeline = new RedactionPipeline(readers, writers, detector, opts);
        var list = results.TryGetValue(doc.FullPath, out var l) ? l : results[doc.FullPath] = [];
        var old = list.FirstOrDefault(r => r.Model == model);
        var target = pipeline.OutputPathFor(doc.FullPath, inputRoot);
        // FR42: the primary model's first successful run is saved automatically; a re-run of the saved model replaces its file.
        // Otherwise the saved output only changes when the user chooses "Use as output".
        var hasOutput = outputResult.ContainsKey(doc.FullPath);
        var replacesOutput = old is not null && outputResult.TryGetValue(doc.FullPath, out var oid) && oid == old.Id;
        var autoSave = (isPrimary && !hasOutput || replacesOutput) && (options.Output.Overwrite || replacesOutput || !File.Exists(target));
        if (isPrimary && !hasOutput && !autoSave) SavedNote = "An output file already exists; choose “Use as output” to replace it.";

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var r = await pipeline.RunAsync(doc.FullPath, autoSave ? target : null, new SyncProgress<RedactionProgress>(p =>
        {
            Progress = p;
            if (p.NewDetections is { Count: > 0 } nd) LiveSpans = [.. LiveSpans, .. nd];
            Notify();
        }), cts!.Token);
        sw.Stop();

        var od = detector as IDetectorMetrics;
        var mr = new ModelResult(Guid.NewGuid(), model, InfoFor(model), r, sw.Elapsed, od?.PromptTokens ?? 0, od?.OutputTokens ?? 0, od?.Discarded ?? 0, DateTime.UtcNow) { Calls = (detector as IDetectorTrace)?.Calls ?? [], BaseResult = r };
        var rv = ReviewOf(doc.FullPath);
        if (!rv.Current.IsEmpty) mr = mr with { Result = rv.Current.ApplyTo(OriginalText!, r, options.Redaction.PlaceholderTemplate) };
        if (old is not null) list.Remove(old);
        list.Add(mr);
        while (list.Count > Math.Max(1, options.Ui.MaxResultsPerDocument))
            list.Remove(list.First(x => outputResult.GetValueOrDefault(doc.FullPath) != x.Id && x.Id != mr.Id));
        if (autoSave) { outputResult[doc.FullPath] = mr.Id; rv.Saved = ReviewState.Empty; }   // the file written by the run holds the model's edits only
        if (isPrimary || ActiveResultId is null) ActiveResultId = mr.Id;   // the primary model's result is the one shown
        Notify();
    }

    /// <summary>Stops the run in progress.</summary>
    public void Cancel() => cts?.Cancel();

    /// <summary>Renders a result's redacted document (a PDF or image with black boxes) into memory, for the in-app viewer.
    /// Nothing is written to disk. Returns null if the result is unknown or its format cannot be rendered.</summary>
    public async Task<(byte[] Bytes, string ContentType)?> RenderRedactedAsync(Guid resultId)
    {
        var entry = results.FirstOrDefault(kv => kv.Value.Any(r => r.Id == resultId));
        if (entry.Value is null) return null;
        var mr = entry.Value.First(r => r.Id == resultId);
        if (renderCache.TryGetValue(resultId, out var hit) && ReferenceEquals(hit.Result, mr.Result)) return (hit.Bytes, hit.ContentType);   // unchanged since last render
        var reader = readers.FirstOrDefault(r => r.CanRead(entry.Key)); if (reader is null) return null;
        var src = await reader.ReadAsync(entry.Key, CancellationToken.None);
        if (writers.FirstOrDefault(w => w.CanWrite(src)) is not IStreamDocumentWriter streamWriter) return null;
        using var ms = new MemoryStream();
        await streamWriter.WriteAsync(src, mr.Result, ms, CancellationToken.None);
        var rendered = (Bytes: ms.ToArray(), ContentType: DocumentFiles.ContentType(entry.Key) ?? "application/octet-stream");
        renderCache[resultId] = (mr.Result, rendered.Bytes, rendered.ContentType);
        return rendered;
    }

    /// <summary>The last in-memory rendering of each result's redacted document, reused until the result changes (rendering a scan runs OCR, so it is slow).</summary>
    readonly Dictionary<Guid, (RedactionResult Result, byte[] Bytes, string ContentType)> renderCache = new();
    /// <summary>A number that changes whenever a result's redacted output changes, used to make the page images refresh.</summary>
    public int RenderStamp(Guid resultId) => ResultsForSelected.FirstOrDefault(r => r.Id == resultId) is { } r ? System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(r.Result) : 0;

    /// <summary>Writes the chosen result as the saved output (FR42).</summary>
    public async Task UseAsOutputAsync(Guid id)
    {
        if (Selected is not { } doc || ResultsForSelected.FirstOrDefault(r => r.Id == id) is not { } mr) return;
        var reader = readers.First(r => r.CanRead(doc.FullPath));
        var src = await reader.ReadAsync(doc.FullPath, CancellationToken.None);
        var target = new RedactionPipeline(readers, writers, new NullDetector(), options).OutputPathFor(doc.FullPath, inputRoot);
        await writers.First(w => w.CanWrite(src)).WriteAsync(src, mr.Result, target, CancellationToken.None);
        outputResult[doc.FullPath] = id; SavedNote = null; ReviewOf(doc.FullPath).Saved = ReviewOf(doc.FullPath).Current;
        Documents.SetStatus(doc.FullPath, mr.EditCount == 0 ? DocumentStatus.NeedsReview : DocumentStatus.Processed, null, mr.EditCount, mr.Model);
        Notify();
    }

    // ---- manual review ----
    /// <summary>The review record for a document (created when first needed).</summary>
    DocReview ReviewOf(string path) => reviews.TryGetValue(path, out var r) ? r : reviews[path] = new DocReview();
    /// <summary>The selected document's review, or null if none is selected.</summary>
    DocReview? CurrentReview => Selected is { } s ? ReviewOf(s.FullPath) : null;
    /// <summary>True when a person can add, reject and restore edits (a document is loaded and readable).</summary>
    public bool CanReview => OriginalText is not null && !PreviewOnly && !IsRunning;
    /// <summary>The categories a person can choose when redacting by hand (those switched on in the config).</summary>
    public IReadOnlyList<string> ManualTypes => PromptBuilder.EnabledTypes(options).ToList();
    /// <summary>True if there is a change to undo.</summary>
    public bool CanUndo => CurrentReview?.Undo.Count > 0;
    /// <summary>True if there is an undone change to redo.</summary>
    public bool CanRedo => CurrentReview?.Redo.Count > 0;
    /// <summary>True when the reviewer's changes are not yet written to the output file.</summary>
    public bool IsModified => CurrentReview is { } r && !r.Current.SameAs(r.Saved);
    /// <summary>How many reviewer changes the selected document has (added plus rejected).</summary>
    public int ReviewChangeCount => CurrentReview is { } r ? r.Current.Manual.Count + r.Current.Rejected.Count : 0;

    /// <summary>Shows the hand-made result, creating it (with no model) if the document has none yet. This is how a document is redacted without the LLM.</summary>
    public ModelResult? EnsureManualResult()
    {
        if (Selected is not { } doc || OriginalText is null) return null;
        var list = results.TryGetValue(doc.FullPath, out var l) ? l : results[doc.FullPath] = [];
        var m = list.FirstOrDefault(r => r.IsManual);
        if (m is null)
        {
            var empty = new RedactionResult(OriginalText, []);
            m = new ModelResult(Guid.NewGuid(), ModelResult.ManualModelName, null, ReviewOf(doc.FullPath).Current.ApplyTo(OriginalText, empty, options.Redaction.PlaceholderTemplate),
                TimeSpan.Zero, 0, 0, 0, DateTime.UtcNow) { BaseResult = empty };
            list.Add(m);
        }
        if (ActiveResult is null) ActiveResultId = m.Id;
        Notify();
        return m;
    }

    /// <summary>Records a new review state as one undoable step, saves it, and rebuilds every result of the document with it.</summary>
    void Commit(ReviewState next)
    {
        if (Selected is not { } doc || OriginalText is null) return;
        var rv = ReviewOf(doc.FullPath);
        if (next.SameAs(rv.Current)) return;
        rv.Undo.Push(rv.Current); rv.Redo.Clear(); SetReview(doc, rv, next);
    }

    /// <summary>Makes a review state current: persists it (offsets only) and recomputes each result's edits and redacted text from it.</summary>
    void SetReview(DocumentItem doc, DocReview rv, ReviewState state)
    {
        rv.Current = state;
        reviewStore?.Save(doc.FullPath, state);
        if (results.TryGetValue(doc.FullPath, out var list))
            for (var i = 0; i < list.Count; i++)
                if (list[i].BaseResult is { } b) list[i] = list[i] with { Result = state.ApplyTo(OriginalText!, b, options.Redaction.PlaceholderTemplate) };
        Notify();
    }

    /// <summary>Redacts a stretch of the original text by hand. With allOccurrences, every identical piece of text is redacted too.
    /// Returns how many redactions were added.</summary>
    public int AddManual(int start, int length, string type, bool allOccurrences)
    {
        if (!CanReview || OriginalText is not { } text || start < 0 || length <= 0 || start + length > text.Length) return 0;
        while (length > 0 && char.IsWhiteSpace(text[start])) { start++; length--; }          // ignore stray spaces at the ends of the selection
        while (length > 0 && char.IsWhiteSpace(text[start + length - 1])) length--;
        if (length == 0) return 0;
        EnsureManualResult();
        var rv = ReviewOf(Selected!.FullPath);
        var needle = text.Substring(start, length);
        var starts = new List<int> { start };
        if (allOccurrences)
            for (var i = text.IndexOf(needle, StringComparison.Ordinal); i >= 0; i = text.IndexOf(needle, i + length, StringComparison.Ordinal))
                if (i != start) starts.Add(i);
        var manual = rv.Current.Manual.ToList(); var rejected = rv.Current.Rejected.ToList(); var added = 0;
        foreach (var i in starts)
        {
            if (manual.Any(m => m.Start == i && m.Length == length && m.Type == type)) continue;
            rejected.RemoveAll(r => r.Start < i + length && i < r.Start + r.Length);   // redacting over a rejected edit supersedes the rejection
            manual.Add(new ManualSpan(type, i, length)); added++;
        }
        if (added > 0) Commit(rv.Current with { Manual = manual, Rejected = rejected });
        return added;
    }

    /// <summary>Rejects an edit of the viewed result: an AI edit is left in the text (and can be restored), a manual one is removed.</summary>
    public void RejectEdit(int editId)
    {
        if (CurrentReview is not { } rv || FindEdit(editId) is not { } e) return;
        if (e.Status == EditStatus.Rejected) return;
        Commit(Without(rv.Current, e));
    }

    /// <summary>Puts a rejected AI edit back.</summary>
    public void RestoreEdit(int editId)
    {
        if (CurrentReview is not { } rv || FindEdit(editId) is not { Status: EditStatus.Rejected } e) return;
        Commit(rv.Current with { Rejected = rv.Current.Rejected.Where(r => r != new SpanKey(e.OriginalStart, e.OriginalLength)).ToList() });
    }

    /// <summary>Changes an edit's category. A manual edit is updated; an AI edit is replaced by a manual one of the new category.</summary>
    public void ChangeType(int editId, string type)
    {
        if (CurrentReview is not { } rv || FindEdit(editId) is not { } e || e.Type == type || e.Status == EditStatus.Rejected) return;
        var without = Without(rv.Current, e);
        Commit(without with { Manual = [.. without.Manual, new ManualSpan(type, e.OriginalStart, e.OriginalLength)] });
    }

    /// <summary>The review state with this edit taken out: removed if it was manual, rejected if it came from a model.</summary>
    static ReviewState Without(ReviewState s, RedactionEdit e) => e.Source == "human"
        ? s with { Manual = s.Manual.Where(m => !(m.Start == e.OriginalStart && m.Length == e.OriginalLength)).ToList() }
        : s with { Rejected = [.. s.Rejected, new SpanKey(e.OriginalStart, e.OriginalLength)] };

    /// <summary>An edit of the viewed result by its number.</summary>
    RedactionEdit? FindEdit(int id) => ActiveResult?.Result.Edits.FirstOrDefault(e => e.Id == id);

    /// <summary>Undoes the last review change.</summary>
    public void Undo()
    {
        if (Selected is not { } doc || CurrentReview is not { Undo.Count: > 0 } rv) return;
        rv.Redo.Push(rv.Current); SetReview(doc, rv, rv.Undo.Pop());
    }

    /// <summary>Redoes the last undone review change.</summary>
    public void Redo()
    {
        if (Selected is not { } doc || CurrentReview is not { Redo.Count: > 0 } rv) return;
        rv.Undo.Push(rv.Current); SetReview(doc, rv, rv.Redo.Pop());
    }

    /// <summary>Writes the viewed result, with the reviewer's changes, as the saved output and clears the modified marker.</summary>
    public async Task SaveAsync()
    {
        if (ActiveResult is { } a) await UseAsOutputAsync(a.Id);
    }

    /// <summary>Blacks out a rectangle a person drew on a page (for example a signature or logo). Works without any model.
    /// Coordinates are PDF points from the page's bottom-left; boxes smaller than a few points are ignored.</summary>
    public bool AddArea(int page, double x, double y, double width, double height)
    {
        if (!CanReview || OriginalDoc?.PageSizes is not { } sizes || page < 0 || page >= sizes.Count || width < 3 || height < 3) return false;
        var (pw, ph) = sizes[page];
        x = Math.Max(0, x); y = Math.Max(0, y); width = Math.Min(width, pw - x); height = Math.Min(height, ph - y);
        if (width < 3 || height < 3) return false;
        EnsureManualResult();
        var rv = ReviewOf(Selected!.FullPath);
        Commit(rv.Current with { Areas = [.. rv.Current.Areas, new AreaBox(page, x, y, width, height)] });
        return true;
    }

    /// <summary>Removes a hand-drawn rectangle (by its list number).</summary>
    public void RemoveArea(int bookmarkId)
    {
        if (CurrentReview is not { } rv) return;
        var i = bookmarkId - Bookmark.AreaIdBase;
        if (ActiveResult?.Result.AreaList is not { } list || i < 0 || i >= list.Count) return;
        Commit(rv.Current with { Areas = rv.Current.Areas.Where(a => a != list[i]).ToList() });
    }

    /// <summary>Detector that finds nothing; used when only rewriting an existing result.</summary>
    sealed class NullDetector : IEntityDetector
    {
        /// <summary>Returns no detections.</summary>
        public Task<IReadOnlyList<DetectedEntity>> DetectAsync(string t, IProgress<RedactionProgress>? p, CancellationToken c) => Task.FromResult<IReadOnlyList<DetectedEntity>>([]);
    }
}
