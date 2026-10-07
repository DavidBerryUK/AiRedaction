namespace AiDocumentRedactor.Explorer;

/// <summary>A stored result for a document, with where it came from: the dataset and the date that dataset was made. The text is what the detectors saw, and its hash lets a reader
/// prove it matches the file on disk.</summary>
public record PreviousRun(string DatasetId, string DatasetCreatedAt, string DocText, string TextHash, StoredRun Run);

/// <summary>Finds, for one document, the stored result of each model, taking the most recent where several datasets hold one. Only final datasets are used. A result has no date of
/// its own, so "most recent" means the newest dataset.</summary>
public class PreviousRunFinder(ExplorerCatalog catalog)
{
    /// <summary>One stored result per model for the document (ordered by model name), or an empty list if there is nothing: no datasets folder, no such document, or no results. A
    /// dataset that cannot be opened is skipped so the others still count.</summary>
    public async Task<IReadOnlyList<PreviousRun>> FindAsync(string docId, CancellationToken ct = default)
    {
        var found = new List<(DatasetListing Dataset, StoredRuns? Stored)>();
        foreach (var dataset in await catalog.ListAsync())
        {
            ct.ThrowIfCancellationRequested();
            if (dataset.Status != "final")
            {
                continue;
            }

            try
            {
                var service = await catalog.OpenAsync(dataset.Id);
                found.Add((dataset, service is null ? null : await service.StoredRunsAsync(docId)));
            }
            catch (Exception ex) when (ex is InvalidDataException or IOException or Microsoft.Data.Sqlite.SqliteException)
            {
                // A dataset that is damaged or in use is left out; the others can still be shown.
            }
        }

        return PickLatest(found);
    }

    /// <summary>From the stored results found in several datasets, keeps one per model: the one from the newest dataset. Ordered by model name.</summary>
    public static IReadOnlyList<PreviousRun> PickLatest(IEnumerable<(DatasetListing Dataset, StoredRuns? Stored)> found) => [.. found
        .Where(f => f.Stored is not null)
        .OrderByDescending(f => f.Dataset.CreatedAt, StringComparer.Ordinal)
        .SelectMany(f => f.Stored!.Runs.Select(run => new PreviousRun(f.Dataset.Id, f.Dataset.CreatedAt, f.Stored.Text, f.Stored.TextHash, run)))
        .GroupBy(p => p.Run.Model)
        .Select(g => g.First())
        .OrderBy(p => p.Run.Model, StringComparer.Ordinal)];
}
