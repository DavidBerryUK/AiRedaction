using AiDocumentRedactor.App.ViewModels;
using AiDocumentRedactor.Core;
using AiDocumentRedactor.Explorer;

namespace AiDocumentRedactor.App.Web;

/// <summary>Gives the redaction session the stored results from the evaluation datasets, so a document's earlier runs can be shown without running a model again. Only reads.</summary>
public sealed class StoredRunSource(ExplorerCatalog catalog) : IPreviousRunSource
{
    readonly PreviousRunFinder finder = new(catalog);

    /// <inheritdoc />
    public async Task<IReadOnlyList<PreviousRunData>> FindAsync(string relativePath, CancellationToken ct) =>
        [.. (await finder.FindAsync(relativePath, ct)).Select(p => new PreviousRunData(
            new PreviousRunInfo(p.DatasetId, p.DatasetCreatedAt),
            p.Run.Model,
            p.TextHash,
            [.. p.Run.Spans.Select(s => new DetectedEntity(s.Type, s.Start, s.Length, s.Confidence ?? 0, s.Source, s.Flag))],
            TimeSpan.FromSeconds(p.Run.DetectSeconds),
            p.Run.PromptTokens,
            p.Run.OutputTokens,
            p.Run.Discarded))];

    /// <summary>Opens every final dataset in the background, so the first document a user picks does not wait for a dataset's database to be built. Any failure is ignored: the
    /// dataset is simply opened (or skipped) when a document needs it.</summary>
    public void WarmUp() => _ = Task.Run(async () =>
    {
        try
        {
            foreach (var dataset in (await catalog.ListAsync()).Where(d => d.Status == "final"))
            {
                await catalog.OpenAsync(dataset.Id);
            }
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or Microsoft.Data.Sqlite.SqliteException)
        {
            // Left for the first document that needs it.
        }
    });
}
