using System.Text.Json;
using AiDocumentRedactor.Core;
using AiDocumentRedactor.Eval;
using AiDocumentRedactor.Explorer.Dataset;
using Xunit;

namespace AiDocumentRedactor.Tests;

/// <summary>Tests for the results dataset: the CSV reader and writer, how saved rows are named, the item-by-item facts behind a score, the interim converter and the validator.</summary>
public class DatasetTests
{
    /// <summary>Cells with commas, quotes and line breaks survive a write and a read, and an empty cell stays empty.</summary>
    [Fact]
    public async Task Csv_round_trips_awkward_cells()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".csv");
        try
        {
            Column[] columns = [new("a"), new("b"), new("c")];
            await Csv.WriteAsync(path, columns, [["plain", "with, comma", "line one\nline \"two\""], [string.Empty, "é 😀", null]]);
            var (names, rows) = await Csv.ReadAsync(path);
            Assert.Equal(["a", "b", "c"], names);
            Assert.Equal("with, comma", rows[0]["b"]);
            Assert.Equal("line one\nline \"two\"", rows[0]["c"]);
            Assert.Equal(string.Empty, rows[1]["a"]);
            Assert.Equal("é 😀", rows[1]["b"]);
            Assert.Equal(string.Empty, rows[1]["c"]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>A row of the wrong width is refused, not guessed at.</summary>
    [Fact]
    public void Csv_rejects_ragged_rows()
    {
        Assert.Throws<InvalidDataException>(() => Csv.Parse("a,b\n1\n"));
        Assert.Throws<InvalidDataException>(() => Csv.Parse("a\n\"never closed\n"));
    }

    /// <summary>A saved row's name splits into the Ollama model and the variant, and the no-model baselines have no model.</summary>
    [Theory]
    [InlineData("rules only", "", "plain")]
    [InlineData("GLiNER only", "", "plain")]
    [InlineData("phi4", "phi4", "plain")]
    [InlineData("gemma4:31b + GLiNER", "gemma4:31b", "with-gliner")]
    [InlineData("gpt-oss + GLiNER (all flags accepted)", "gpt-oss", "with-gliner-all-flags-accepted")]
    [InlineData("qwen3.6:27b + GLiNER (correct flags accepted)", "qwen3.6:27b", "with-gliner-correct-flags-accepted")]
    public void Config_names_split_into_model_and_variant(string name, string model, string variant) => Assert.Equal((model, variant), InterimConverter.ParseConfig(name));

    /// <summary>The held-out documents are in in/finance-sample; the others keep their folder. Corpus folders become corpus names.</summary>
    [Fact]
    public void Paths_and_corpus_names()
    {
        Assert.Equal("finance-sample/heldout-001.txt", InterimConverter.DocId("heldout", "text/heldout-001.txt"));
        Assert.Equal("pdf/01-record.pdf", InterimConverter.DocId("formats", "pdf/01-record.pdf"));
        Assert.Equal("heldout", InterimConverter.CorpusName("/x/tests/HeldOutCorpus"));
        Assert.Equal("formats", InterimConverter.CorpusName("/x/tests/TestCorpus/"));
    }

    const string Text = "Alice Smith met Bob in Paris. Alice Smith left from High Street.";

    static GroundTruth Key() => new("doc-a", "Letter", [new GtEntity("body", "PERSON", "Alice Smith", 2)], ["High Street"]);

    static DetectedEntity Span(string find, string type, int skip = 0)
    {
        var start = -1;
        for (var i = 0; i <= skip; i++)
        {
            start = Text.IndexOf(find, start + 1, StringComparison.Ordinal);
        }

        return new DetectedEntity(type, start, find.Length, 1, "llm");
    }

    /// <summary>The facts behind a score list every occurrence as caught or missed, and each over-redaction and broken must-keep item, and they add up to the totals.</summary>
    [Fact]
    public void Score_keeps_the_facts_behind_the_totals()
    {
        var spans = new[] { Span("Alice Smith", "PERSON"), Span("Bob", "PERSON"), Span("High Street", "ADDRESS") };
        var score = Scoring.Score(Text, Redactor.Apply(Text, spans, "[REDACTED:{type}]"), Key(), "Plain text");
        Assert.Equal(2, score.Present);
        Assert.Equal(1, score.Caught);
        Assert.Equal(score.Caught, score.Facts.Count(f => f.Kind == "caught"));
        Assert.Equal(score.Present - score.Caught, score.Facts.Count(f => f.Kind == "missed"));
        Assert.Equal(score.FalsePositives.Count, score.Facts.Count(f => f.Kind == "over_redaction"));
        Assert.Equal(1, score.Facts.Count(f => f.Kind == "preserve_broken"));
        var caught = Assert.Single(score.Facts, f => f.Kind == "caught");
        Assert.Equal(0, caught.Start);   // the first occurrence was the one redacted
        Assert.Equal("Alice Smith", Text.Substring(caught.Start, caught.Length));
        Assert.All(score.Facts.Where(f => f.Kind is "caught" or "missed"), f => Assert.Equal(0, f.EntityIndex));
    }

    /// <summary>A key item that never appears in the text is reported as lost to extraction, with no position.</summary>
    [Fact]
    public void Score_reports_items_lost_to_extraction()
    {
        var key = new GroundTruth("doc-a", "Letter", [new GtEntity("body", "PERSON", "Nobody Here", 1)], []);
        var score = Scoring.Score(Text, Redactor.Apply(Text, [], "[REDACTED:{type}]"), key, "Plain text");
        var lost = Assert.Single(score.Facts);
        Assert.Equal("lost_to_extraction", lost.Kind);
        Assert.Equal(-1, lost.Start);
    }

    internal static string MakeCorpus(string root, out string savedPath)
    {
        Directory.CreateDirectory(Path.Combine(root, "tests", "TestCorpus", "ground-truth"));
        Directory.CreateDirectory(Path.Combine(root, "tests", "TestCorpus", "text"));
        Directory.CreateDirectory(Path.Combine(root, "in", "text"));
        foreach (var id in new[] { "doc-a", "doc-b" })
        {
            File.WriteAllText(Path.Combine(root, "tests", "TestCorpus", "ground-truth", id + ".json"),
                JsonSerializer.Serialize(new { id, title = "Letter", entities = new[] { new { where = "body", type = "PERSON", text = "Alice Smith", occurrences = 2 } }, mustPreserve = new[] { "High Street" } }));
            File.WriteAllText(Path.Combine(root, "tests", "TestCorpus", "text", id + ".txt"), Text);
            File.WriteAllText(Path.Combine(root, "in", "text", id + ".txt"), Text);
        }

        static SavedSpan Saved(DetectedEntity e) => new(e.Type, e.Start, e.Length, e.Confidence, e.Source, e.Flag);
        var options = new RedactorOptions { Ocr = { Enabled = false }, Gliner = { Enabled = true } };
        var scores = new List<DocScore>();
        foreach (var id in new[] { "doc-a", "doc-b" })
        {
            var file = $"text/{id}.txt";
            DocScore Row(string model, params DetectedEntity[] spans) => new() { File = file, Model = model, Group = "Plain text", DocType = "Letter", Spans = spans.Select(Saved).ToList(), DetectSeconds = 1.5 };
            var gliner = new DetectedEntity("PERSON", Text.IndexOf("Bob", StringComparison.Ordinal), 3, 0.9, "gliner");
            scores.Add(Row("rules only"));
            scores.Add(Row("GLiNER only", gliner));
            if (id == "doc-a")
            {
                scores.Add(Row("m", Span("Alice Smith", "PERSON")));
                scores.Add(Row("m + GLiNER"));
                scores.Add(Row("m + GLiNER (all flags accepted)"));
                scores.Add(Row("m + GLiNER (correct flags accepted)"));
            }
        }

        var models = new List<SavedModel> { new("rules only", null), new("GLiNER only", null), new("m", null), new("m + GLiNER", null), new("m + GLiNER (all flags accepted)", null), new("m + GLiNER (correct flags accepted)", null) };
        var run = new SavedRun(new DateTime(2026, 10, 3), 10, "test machine", Path.Combine(root, "elsewhere", "TestCorpus"), 2, 2, options, models, false, false,
            ["`text/doc-b.txt` with m: The request was canceled due to the configured HttpClient.Timeout of 300 seconds elapsing."], "0.35.1", scores);
        savedPath = Path.Combine(root, "run.scores.json");
        File.WriteAllText(savedPath, run.ToJson());
        return Path.Combine(root, "out");
    }

    /// <summary>A small saved run converts into a dataset that passes the validator, with results, spans, outcomes, a timeout row and the GLiNER variants rebuilt.</summary>
    [Fact]
    public async Task Converter_makes_a_valid_dataset()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            var outDir = MakeCorpus(root, out var saved);
            var code = await InterimConverter.RunAsync([saved], Path.Combine(root, "in"), Path.Combine(root, "tests"), outDir, "test", null);
            Assert.Equal(0, code);
            Assert.Empty(await DatasetValidator.ValidateAsync(outDir));

            var (_, results) = await Csv.ReadAsync(Path.Combine(outDir, "results.csv"));
            Assert.Equal(6 + 2 + 4, results.Count);   // 6 setups on doc-a, the 2 baselines on doc-b, and the model's 4 setups timed out on doc-b
            var failed = results.Where(r => r["status"] != "ok").ToList();
            Assert.Equal(4, failed.Count);
            Assert.All(failed, r => Assert.Equal("timeout", r["status"]));
            var m = results.Single(r => r["result_id"] == "text/doc-a.txt|m|1");
            Assert.Equal("2", m["present"]);
            Assert.Equal("1", m["caught"]);
            Assert.Equal("m", m["model"]);

            // GLiNER found "Bob", which is not on the key: the model + GLiNER row flags it, and a flag outcome is recorded.
            var gl = results.Single(r => r["result_id"] == "text/doc-a.txt|m + GLiNER|1");
            Assert.Equal("with-gliner", gl["variant"]);
            Assert.Equal("1", gl["flags_raised"]);
            Assert.Equal("0", gl["flags_correct"]);
            var (_, outcomes) = await Csv.ReadAsync(Path.Combine(outDir, "outcomes.csv"));
            Assert.Contains(outcomes, o => o["result_id"] == gl["result_id"] && o["kind"] == "flag_wrong" && o["text"] == "Bob");
            var accepted = results.Single(r => r["result_id"] == "text/doc-a.txt|m + GLiNER (all flags accepted)|1");
            Assert.Equal("2", accepted["edits"]);
            Assert.Contains(outcomes, o => o["result_id"] == accepted["result_id"] && o["kind"] == "over_redaction" && o["text"] == "Bob");

            var run = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(outDir, "run.json"))).RootElement;
            Assert.Equal("interim", run.GetProperty("status").GetString());
            Assert.Equal("formats", run.GetProperty("corpora")[0].GetProperty("corpus").GetString());
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    /// <summary>The validator reports a text that no longer matches its hash, a span that names no result, and a missing file.</summary>
    [Fact]
    public async Task Validator_finds_broken_datasets()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            var outDir = MakeCorpus(root, out var saved);
            Assert.Equal(0, await InterimConverter.RunAsync([saved], Path.Combine(root, "in"), Path.Combine(root, "tests"), outDir, "test", null));

            var textFile = Path.Combine(outDir, "document-text.jsonl");
            await File.WriteAllTextAsync(textFile, (await File.ReadAllTextAsync(textFile)).Replace("Alice", "Alicf", StringComparison.Ordinal));
            await File.AppendAllTextAsync(Path.Combine(outDir, "spans.csv"), "x#1,no-such-result,PERSON,0,5,1,llm,false\n");
            File.Delete(Path.Combine(outDir, "outcomes.csv"));

            var problems = await DatasetValidator.ValidateAsync(outDir);
            Assert.Contains(problems, p => p.Contains("does not match its text_hash", StringComparison.Ordinal));
            Assert.Contains(problems, p => p.Contains("unknown result 'no-such-result'", StringComparison.Ordinal) || p.Contains("outcomes.csv is missing", StringComparison.Ordinal));
            Assert.Contains(problems, p => p.Contains("outcomes.csv is missing", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    /// <summary>The setups the final evaluator writes for a model are the four the explorer and converter expect, and each parses back to its model and variant.</summary>
    [Fact]
    public void Final_evaluator_setup_names_round_trip()
    {
        var configs = FinalEvaluator.ConfigsFor("gemma4:31b");
        Assert.Equal(["gemma4:31b", "gemma4:31b + GLiNER", "gemma4:31b + GLiNER (all flags accepted)", "gemma4:31b + GLiNER (correct flags accepted)"], configs);
        Assert.Equal(["plain", "with-gliner", "with-gliner-all-flags-accepted", "with-gliner-correct-flags-accepted"], configs.Select(c => InterimConverter.ParseConfig(c).Variant));
        Assert.All(configs, c => Assert.Equal("gemma4:31b", InterimConverter.ParseConfig(c).Model));
    }

    /// <summary>The git state can be read here (the evaluator needs it to tie a dataset to its code).</summary>
    [Fact]
    public async Task Git_state_is_readable()
    {
        var git = await FinalEvaluator.GitStateAsync();
        Assert.NotNull(git);
        Assert.True(git!.Value.Commit.Length >= 7);
    }
}
