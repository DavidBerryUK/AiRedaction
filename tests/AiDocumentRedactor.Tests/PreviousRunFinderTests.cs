using System.Text.Json.Nodes;
using AiDocumentRedactor.Eval;
using AiDocumentRedactor.Explorer;
using Xunit;

namespace AiDocumentRedactor.Tests;

/// <summary>Tests for choosing which stored result to show for a document when several datasets hold one.</summary>
public class PreviousRunFinderTests : IDisposable
{
    readonly string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        if (Directory.Exists(root))
        {
            Directory.Delete(root, true);
        }
    }

    static StoredRuns Stored(params string[] models) => new("doc.txt", "text", "hash",
        [.. models.Select(m => new StoredRun("doc.txt|" + m + "|1", m, 1, 0, 0, 0, 0, 0, []))]);

    static DatasetListing Listing(string id, string created) => new(id, "final", created);

    /// <summary>For a model in two datasets the newer dataset's result is the one shown.</summary>
    [Fact]
    public void Newest_dataset_wins_for_a_model()
    {
        var picked = PreviousRunFinder.PickLatest([(Listing("old", "2026-10-03T10:00:00Z"), Stored("phi4")), (Listing("new", "2026-10-04T10:00:00Z"), Stored("phi4"))]);

        var run = Assert.Single(picked);
        Assert.Equal("new", run.DatasetId);
    }

    /// <summary>A model that only the older dataset has is still shown, beside the newer dataset's models, in model order.</summary>
    [Fact]
    public void A_model_only_in_an_older_dataset_is_still_shown()
    {
        var picked = PreviousRunFinder.PickLatest([(Listing("old", "2026-10-03T10:00:00Z"), Stored("phi4", "gemma")), (Listing("new", "2026-10-04T10:00:00Z"), Stored("phi4"))]);

        Assert.Equal(["gemma", "phi4"], picked.Select(p => p.Run.Model));
        Assert.Equal("old", picked.Single(p => p.Run.Model == "gemma").DatasetId);
        Assert.Equal("new", picked.Single(p => p.Run.Model == "phi4").DatasetId);
    }

    /// <summary>A dataset that has no such document adds nothing.</summary>
    [Fact]
    public void Datasets_without_the_document_are_ignored()
    {
        Assert.Empty(PreviousRunFinder.PickLatest([(Listing("a", "2026-10-03T10:00:00Z"), null)]));
        Assert.Empty(PreviousRunFinder.PickLatest([]));
    }

    /// <summary>With no datasets folder there is nothing to show, and no error.</summary>
    [Fact]
    public async Task No_datasets_folder_gives_an_empty_list()
    {
        var finder = new PreviousRunFinder(new ExplorerCatalog(Path.Combine(root, "missing")));

        Assert.Empty(await finder.FindAsync("text/doc-a.txt"));
    }

    /// <summary>Real datasets on disk: only final datasets count, and the newest of them supplies the result.</summary>
    [Fact]
    public async Task Finds_the_newest_final_result_on_disk()
    {
        var datasets = Path.Combine(root, "datasets");
        await MakeDatasetAsync(Path.Combine(root, "a"), Path.Combine(datasets, "old"), "final", "2026-10-03T10:00:00Z");
        await MakeDatasetAsync(Path.Combine(root, "b"), Path.Combine(datasets, "new"), "final", "2026-10-04T10:00:00Z");
        await MakeDatasetAsync(Path.Combine(root, "c"), Path.Combine(datasets, "interim"), "interim", "2026-10-05T10:00:00Z");

        var picked = await new PreviousRunFinder(new ExplorerCatalog(datasets)).FindAsync("text/doc-a.txt");

        var run = Assert.Single(picked);
        Assert.Equal("new", run.DatasetId);
        Assert.Equal("m", run.Run.Model);
    }

    static async Task MakeDatasetAsync(string corpusRoot, string datasetDir, string status, string createdAt)
    {
        DatasetTests.MakeCorpus(corpusRoot, out var saved);
        Assert.Equal(0, await InterimConverter.RunAsync([saved], Path.Combine(corpusRoot, "in"), Path.Combine(corpusRoot, "tests"), datasetDir, "test", null));
        var path = Path.Combine(datasetDir, "run.json");
        var run = JsonNode.Parse(await File.ReadAllTextAsync(path))!;
        run["status"] = status;
        run["createdAt"] = createdAt;
        run["gitDirty"] = false;
        await File.WriteAllTextAsync(path, run.ToJsonString());
    }
}
