using AiDocumentRedactor.Eval;
using AiDocumentRedactor.Explorer;
using AiDocumentRedactor.Explorer.Dataset;
using Xunit;

namespace AiDocumentRedactor.Tests;

/// <summary>Tests for reading back the stored model results for one document (the data behind "view a previous run").</summary>
public class ExplorerStoredRunsTests : IAsyncLifetime
{
    string root = string.Empty;
    ExplorerService service = default!;

    /// <summary>A small dataset is made from a saved run and opened.</summary>
    public async Task InitializeAsync()
    {
        root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var dataset = DatasetTests.MakeCorpus(root, out var saved);
        Assert.Equal(0, await InterimConverter.RunAsync([saved], Path.Combine(root, "in"), Path.Combine(root, "tests"), dataset, "test", null));
        service = new ExplorerService(await ResultsDatabase.EnsureAsync(dataset));
    }

    public Task DisposeAsync()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        Directory.Delete(root, true);
        return Task.CompletedTask;
    }

    /// <summary>Only the model on its own is returned: not the baselines, and not the GLiNER variants. The text, its hash and the spans come with it.</summary>
    [Fact]
    public async Task Returns_the_plain_result_of_each_model_with_its_spans()
    {
        var stored = await service.StoredRunsAsync("text/doc-a.txt");

        Assert.NotNull(stored);
        var run = Assert.Single(stored.Runs);
        Assert.Equal("m", run.Model);
        Assert.Equal("text/doc-a.txt|m|1", run.ResultId);
        Assert.Equal(1, run.Repeat);
        Assert.Equal(1.5, run.DetectSeconds);
        Assert.Equal(AiDocumentRedactor.Explorer.Dataset.DatasetWriter.HashText(stored.Text), stored.TextHash);
        var span = Assert.Single(run.Spans);
        Assert.Equal("PERSON", span.Type);
        Assert.Equal("Alice Smith", stored.Text.Substring(span.Start, span.Length));
        Assert.False(span.Flag);
        Assert.Equal(run.EditCount, run.Spans.Count(s => !s.Flag));
    }

    /// <summary>A result that timed out is not a result a user can look at, so a document where the only model timed out has no stored runs.</summary>
    [Fact]
    public async Task Ignores_results_that_did_not_succeed()
    {
        var stored = await service.StoredRunsAsync("text/doc-b.txt");

        Assert.NotNull(stored);
        Assert.Empty(stored.Runs);
    }

    /// <summary>A document the dataset does not hold gives null, not an empty answer.</summary>
    [Fact]
    public async Task Unknown_document_gives_null()
    {
        Assert.Null(await service.StoredRunsAsync("text/nope.txt"));
    }
}
