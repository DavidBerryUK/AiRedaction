namespace AiDocumentRedactor.App.ViewModels;

/// <summary>Stored results from earlier evaluation runs. They are kept apart from live results, so running a model never replaces or evicts them, and they are read-only.</summary>
public partial class RedactionSession
{
    /// <summary>Stored results per document (keyed by file path), built once and kept for the session.</summary>
    readonly Dictionary<string, List<ModelResult>> previousRuns = new();

    /// <summary>Where stored results come from. Null switches the feature off.</summary>
    public IPreviousRunSource? PreviousRunSource
    {
        get; init;
    }

    /// <summary>True while the stored results for the selected document are being looked up.</summary>
    public bool PreviousRunsLoading
    {
        get; private set;
    }

    /// <summary>Completes when the lookup started by the last <see cref="SelectAsync"/> has finished (for tests and callers that must wait).</summary>
    public Task PreviousRunsReady
    {
        get; private set;
    } = Task.CompletedTask;

    /// <summary>The stored results for the selected document, one per model, shown beside the live ones.</summary>
    public IReadOnlyList<ModelResult> PreviousRunsForSelected =>
        Selected is { } s && previousRuns.TryGetValue(s.FullPath, out var l) ? l : [];

    /// <summary>True while a stored result is the one being shown. Nothing can be changed or saved then.</summary>
    public bool IsPreviousRunActive => ActiveResult is { IsPreviousRun: true };

    /// <summary>Starts looking up stored results for the document just selected. The page is not held up: the buttons appear when the lookup ends.</summary>
    void BeginPreviousRunLookup(DocumentItem item, string originalText)
    {
        if (PreviousRunSource is null || previousRuns.ContainsKey(item.FullPath))
        {
            return;
        }

        PreviousRunsLoading = true;
        PreviousRunsReady = LoadPreviousRunsAsync(item, originalText);
    }

    async Task LoadPreviousRunsAsync(DocumentItem item, string originalText)
    {
        var built = new List<ModelResult>();
        try
        {
            var found = await PreviousRunSource!.FindAsync(item.RelativePath.Replace('\\', '/'), CancellationToken.None);
            foreach (var data in found)
            {
                if (PreviousRunBuilder.Build(data, originalText, options.Redaction.PlaceholderTemplate) is { } result)
                {
                    built.Add(result);
                }
            }

            previousRuns[item.FullPath] = built;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Stored results are a convenience: if they cannot be read, the document works as it always did, and nothing is cached so the next selection tries again.
        }
        finally
        {
            PreviousRunsLoading = false;
            Notify();
        }
    }
}
