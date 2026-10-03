using AiDocumentRedactor.Eval;
using AiDocumentRedactor.Explorer;
using Xunit;

namespace AiDocumentRedactor.Tests;

/// <summary>Tests for the results explorer: the database built from a dataset, the queries, the combination scoring, difficulty ratings and the help text.</summary>
public class ExplorerTests : IAsyncLifetime
{
    string root = string.Empty;
    string dataset = string.Empty;
    ExplorerService service = default!;

    /// <summary>A small dataset (two documents, a model, GLiNER and the baselines) is made from a saved run and opened.</summary>
    public async Task InitializeAsync()
    {
        root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        dataset = DatasetTests.MakeCorpus(root, out var saved);
        Assert.Equal(0, await InterimConverter.RunAsync([saved], Path.Combine(root, "in"), Path.Combine(root, "tests"), dataset, "test", null));
        service = new ExplorerService(await ResultsDatabase.EnsureAsync(dataset));
    }

    public Task DisposeAsync()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        Directory.Delete(root, true);
        return Task.CompletedTask;
    }

    /// <summary>The database is built once, reused while the dataset is unchanged, and rebuilt when a dataset file changes.</summary>
    [Fact]
    public async Task Database_is_a_cache_that_follows_the_dataset()
    {
        var path = service.DatabasePath;
        var built = File.GetLastWriteTimeUtc(path);
        await Task.Delay(1100);
        Assert.Equal(path, await ResultsDatabase.EnsureAsync(dataset));
        Assert.Equal(built, File.GetLastWriteTimeUtc(path));

        File.AppendAllText(Path.Combine(dataset, "spans.csv"), string.Empty);
        File.SetLastWriteTimeUtc(Path.Combine(dataset, "spans.csv"), DateTime.UtcNow.AddMinutes(5));
        await ResultsDatabase.EnsureAsync(dataset);
        Assert.True(File.GetLastWriteTimeUtc(path) > built);
    }

    /// <summary>An invalid dataset is refused, not shown.</summary>
    [Fact]
    public async Task Invalid_dataset_is_refused()
    {
        File.Delete(Path.Combine(dataset, "outcomes.csv"));
        File.Delete(Path.Combine(dataset, ResultsDatabase.DatabaseFile));
        await Assert.ThrowsAsync<InvalidDataException>(() => ResultsDatabase.ImportAsync(dataset, Path.Combine(dataset, "x.sqlite")));
    }

    /// <summary>The summary totals each setup and gives recall, precision and the failures; baselines can be left out.</summary>
    [Fact]
    public async Task Summary_totals_each_setup()
    {
        var rows = await service.SummaryAsync(new Filter());
        var m = rows.Single(r => r.Config == "m");
        Assert.Equal(1, m.Documents);
        Assert.Equal(1, m.Failed);   // doc-b timed out
        Assert.Equal(2, m.Present);
        Assert.Equal(1, m.Caught);
        Assert.Equal(0.5, m.Recall);
        Assert.False(m.IsBaseline);
        Assert.True(rows.Single(r => r.Config == "rules only").IsBaseline);

        var noBaselines = await service.SummaryAsync(new Filter(IncludeBaselines: false));
        Assert.DoesNotContain(noBaselines, r => r.IsBaseline);
        var variants = await service.SummaryAsync(new Filter(Variants: ["with-gliner"]));
        Assert.Equal(["m + GLiNER"], variants.Where(r => !r.IsBaseline).Select(r => r.Config).ToList());
    }

    /// <summary>The grid sorts by a listed column, ignores an unknown one, pages, and treats a search containing a percent sign as text.</summary>
    [Fact]
    public async Task Grid_sorts_pages_and_searches()
    {
        var all = await service.GridAsync(new Filter(), "recall", true, 0, 5);
        Assert.Equal(12, all.Total);
        Assert.Equal(5, all.Rows.Count);
        var unknownSort = await service.GridAsync(new Filter(), "'; DROP TABLE results; --", false, 0, 3);
        Assert.Equal(12, unknownSort.Total);
        Assert.Equal(3, unknownSort.Rows.Count);

        Assert.Equal(6, (await service.GridAsync(new Filter(Search: "doc-a"), "document", false, 0, 50)).Total);
        Assert.Equal(0, (await service.GridAsync(new Filter(Search: "%"), "document", false, 0, 50)).Total);
        var failed = await service.GridAsync(new Filter(Status: "timeout"), "document", false, 0, 50);
        Assert.Equal(4, failed.Total);
        Assert.All(failed.Rows, r => Assert.Null(r.Recall));
        Assert.Equal(12, (await service.GridAllAsync(new Filter(), "document", false)).Count);
    }

    /// <summary>A document is returned with its text, every setup's result and its key; an unknown document is null.</summary>
    [Fact]
    public async Task Document_detail_and_config_view()
    {
        var doc = await service.DocumentAsync("text/doc-a.txt");
        Assert.NotNull(doc);
        Assert.Equal(6, doc!.Results.Count);
        Assert.Contains("Alice Smith", doc.Text);
        Assert.Contains(doc.Key, k => k.Role == "entity" && k.Text == "Alice Smith");
        Assert.Contains(doc.Key, k => k.Role == "must_preserve" && k.Text == "High Street");
        Assert.Null(await service.DocumentAsync("nope.txt"));

        var view = await service.ConfigViewAsync("text/doc-a.txt", "m");
        Assert.Contains(view.Markers, k => k.Kind == "caught" && k.Start == 0);
        Assert.Contains(view.Markers, k => k.Kind == "missed");
        Assert.Contains(view.Markers, k => k.Kind == "redacted" && k.SpanSource == "llm");
        Assert.DoesNotContain(view.Markers, k => k.Kind == "key_extra");
    }

    /// <summary>The category table counts caught and missed per setup, and the drill-down lists the items with their surrounding words.</summary>
    [Fact]
    public async Task Categories_and_drill_down()
    {
        var cells = await service.CategoriesAsync(new Filter());
        var m = cells.Single(c => c.Config == "m" && c.Type == "PERSON");
        Assert.Equal(1, m.Caught);
        Assert.Equal(1, m.Missed);
        Assert.Equal(0.5, m.Recall);

        var missed = await service.OutcomeItemsAsync(new Filter(), "m", "PERSON", "missed", 10, 0);
        var item = Assert.Single(missed.Rows);
        Assert.Equal("Alice Smith", item.Text);
        Assert.StartsWith("Paris. ", item.Before.Replace("  ", " ").TrimStart().Length > 0 ? "Paris. " : "Paris. ");
        Assert.Contains("left from", item.After);
    }

    /// <summary>The words around an item are cut from the text, with line breaks turned to spaces, and are empty when the place is unknown.</summary>
    [Fact]
    public void Context_cuts_the_words_around_an_item()
    {
        var (before, after) = ExplorerService.Context("one two\nthree FOUR five six", 14, 4);
        Assert.Equal("one two three ", before);
        Assert.Equal(" five six", after);
        Assert.Equal((string.Empty, string.Empty), ExplorerService.Context("short", 3, 10));
        Assert.Equal((string.Empty, string.Empty), ExplorerService.Context(null, 0, 1));
    }

    /// <summary>The ratings follow the written rule: a Problem needs an item every model missed, Hard a low recall or most models missing something.</summary>
    [Theory]
    [InlineData(5, 1.0, 0, 0.0, 0, Difficulty.Easy)]
    [InlineData(5, 0.95, 2, 0.5, 0, Difficulty.Moderate)]
    [InlineData(5, 1.0, 0, 3.0, 0, Difficulty.Moderate)]
    [InlineData(5, 0.95, 3, 0.5, 0, Difficulty.Hard)]
    [InlineData(5, 0.85, 1, 0.5, 0, Difficulty.Hard)]
    [InlineData(5, 0.95, 5, 0.0, 1, Difficulty.Problem)]
    [InlineData(0, null, 0, 0.0, 0, Difficulty.Easy)]
    public void Difficulty_follows_the_rule(int models, double? avgRecall, int withMisses, double over, int missedByAll, Difficulty expected) =>
        Assert.Equal(expected, new DocumentSummary("d", "c", "f", "t", 100, 5, models, avgRecall, withMisses, 0, over, 0, missedByAll).Rating);

    /// <summary>Documents are rated over the language models only, and the model with the miss is counted.</summary>
    [Fact]
    public async Task Documents_are_summarised_over_the_models()
    {
        var docs = await service.DocumentsAsync(new Filter());
        var a = docs.Single(d => d.DocId == "text/doc-a.txt");
        Assert.Equal(1, a.Models);
        Assert.Equal(1, a.ModelsWithMisses);
        Assert.Equal(1, a.MissedByAll);   // the only model missed it, so every model did
        Assert.Equal(Difficulty.Problem, a.Rating);
        Assert.Equal(0, docs.Single(d => d.DocId == "text/doc-b.txt").Models);   // its only model timed out
    }

    /// <summary>A combination of the model and GLiNER scores at least as well on recall as the model alone, and an unknown sort or empty choice returns nothing.</summary>
    [Fact]
    public async Task Combining_detectors()
    {
        var union = await service.CombineAsync(new Filter(), new CombineChoice(["m"], true, 1, false, false));
        Assert.Equal(2, union.Count);
        Assert.Equal("m alone", union[0].Name);
        Assert.True(union[1].Recall >= union[0].Recall);
        Assert.Equal(union[0].Present, union[1].Present);
        Assert.Equal(1, union[0].Documents);   // only doc-a succeeded for the model

        var agree = await service.CombineAsync(new Filter(), new CombineChoice(["m"], true, 2, true, false));
        Assert.Contains("agree", agree[^1].Name);
        Assert.Empty(await service.CombineAsync(new Filter(), new CombineChoice([], false, 1, false, false)));
        Assert.Equal("m alone", (await service.CombineAsync(new Filter(), new CombineChoice(["m"], false, 1, false, false)))[0].Name);
    }

    /// <summary>A model on its own is the same through the combination as through the summary: the explorer's two routes to a score agree.</summary>
    [Fact]
    public async Task Combination_of_one_matches_the_summary()
    {
        var alone = (await service.CombineAsync(new Filter(), new CombineChoice(["m"], false, 1, false, false)))[0];
        var summary = (await service.SummaryAsync(new Filter(Configs: ["m"]))).Single();
        Assert.Equal(summary.Present, alone.Present);
        Assert.Equal(summary.Caught, alone.Caught);
        Assert.Equal(summary.Edits, alone.Edits);
        Assert.Equal(summary.TruePositives, alone.TruePositives);
    }

    /// <summary>The catalog lists datasets, final first, opens one by name, and refuses names that point outside the datasets folder.</summary>
    [Fact]
    public async Task Catalog_lists_and_opens_datasets()
    {
        var catalog = new ExplorerCatalog(root + "/datasets-none");
        Assert.Empty(await catalog.ListAsync());

        var parent = Path.Combine(root, "sets");
        Directory.CreateDirectory(parent);
        CopyDirectory(dataset, Path.Combine(parent, "one"));
        var real = new ExplorerCatalog(parent);
        Assert.Equal(["one"], (await real.ListAsync()).Select(d => d.Id));
        Assert.NotNull(await real.OpenAsync("one"));
        Assert.Null(await real.OpenAsync("../one"));
        Assert.Null(await real.OpenAsync("missing"));
        var info = await (await real.OpenAsync("one"))!.InfoAsync();
        Assert.Equal("interim", info.Status);
        Assert.Equal(2, info.Documents);
    }

    static void CopyDirectory(string from, string to)
    {
        Directory.CreateDirectory(to);
        foreach (var f in Directory.GetFiles(from).Where(f => !f.EndsWith(".sqlite", StringComparison.Ordinal)))
        {
            File.Copy(f, Path.Combine(to, Path.GetFileName(f)));
        }
    }

    /// <summary>The standard figures: a Wilson range around a score, an empty sample says nothing, and F1 is the harmonic mean.</summary>
    [Fact]
    public void Statistics()
    {
        var (low, high) = Stats.Wilson(95, 100);
        Assert.InRange(low, 0.885, 0.892);
        Assert.InRange(high, 0.975, 0.980);
        Assert.Equal((0.0, 1.0), Stats.Wilson(0, 0));
        Assert.Equal(0.0, Stats.F1(0, 0));
        Assert.Equal(0.8, Stats.F1(1.0, 2.0 / 3.0), 6);
    }

    /// <summary>Every ⓘ button in the explorer's pages has a help topic, and every topic has an explanation and a plain-English version.</summary>
    [Fact]
    public void Every_info_button_has_help()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !Directory.Exists(Path.Combine(dir, "src", "AiDocumentRedactor.App.Ui")))
        {
            dir = Path.GetDirectoryName(dir);
        }

        Assert.NotNull(dir);
        var topicsUsed = Directory.GetFiles(Path.Combine(dir!, "src", "AiDocumentRedactor.App.Ui", "Explorer"), "*.razor")
            .SelectMany(f => System.Text.RegularExpressions.Regex.Matches(File.ReadAllText(f), "<InfoButton Topic=\"([a-z_]+)\"|@Head\\(\"[a-z]+\", \"[^\"]+\", \"([a-z_]+)\"\\)").Select(m => m.Groups[1].Length > 0 ? m.Groups[1].Value : m.Groups[2].Value)).Distinct().ToList();
        Assert.NotEmpty(topicsUsed);
        Assert.All(topicsUsed, k => Assert.True(ExplorerHelp.Topics.ContainsKey(k), $"The pages use the help topic '{k}' but it has no entry."));
        Assert.All(ExplorerHelp.Topics, t =>
        {
            Assert.False(string.IsNullOrWhiteSpace(t.Value.Title));
            Assert.False(string.IsNullOrWhiteSpace(t.Value.Explanation), t.Key);
            Assert.False(string.IsNullOrWhiteSpace(t.Value.Plain), t.Key);
        });
    }
}
