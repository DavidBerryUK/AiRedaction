namespace AiDocumentRedactor.Explorer.Dataset;

/// <summary>What a cell holds: text, a whole number, a number with decimals, or true/false.</summary>
public enum ColumnKind
{
    Text, Int, Number, Bool
}

/// <summary>One column of a dataset file. A required column must be present and never empty.</summary>
public record Column(string Name, bool Required = false, ColumnKind Kind = ColumnKind.Text);

/// <summary>The files of a dataset and their columns, as written in documentation/RESULTS_DATASET_FORMAT.md. The writer and the validator both use this.</summary>
public static class Schema
{
    public const string FormatVersion = "0.1";
    public const string RunFile = "run.json", TextFile = "document-text.jsonl", DocumentsFile = "documents.csv", EntitiesFile = "entities.csv",
        ResultsFile = "results.csv", SpansFile = "spans.csv", OutcomesFile = "outcomes.csv";

    public static readonly string[] Variants = ["plain", "with-gliner", "with-gliner-all-flags-accepted", "with-gliner-correct-flags-accepted"];
    public static readonly string[] Statuses = ["ok", "timeout", "error", "skipped"];
    public static readonly string[] Roles = ["entity", "must_preserve", "ignore"];
    public static readonly string[] OutcomeKinds = ["caught", "missed", "lost_to_extraction", "over_redaction", "unjudged", "preserve_broken", "flag_correct", "flag_wrong", "key_extra"];

    public static readonly Column[] Documents =
    [
        new("doc_id", true), new("key_id", true), new("corpus", true), new("format_group", true), new("doc_type"), new("chars", false, ColumnKind.Int),
        new("text_hash", true), new("entity_count", false, ColumnKind.Int), new("occurrence_count", false, ColumnKind.Int),
    ];

    public static readonly Column[] Entities =
    [
        new("entity_id", true), new("key_id", true), new("role", true), new("type"), new("text", true), new("occurrences", false, ColumnKind.Int),
        new("where"), new("origin"), new("judged", false, ColumnKind.Bool),
    ];

    public static readonly Column[] Results =
    [
        new("result_id", true), new("doc_id", true), new("config", true), new("repeat", true, ColumnKind.Int), new("model"), new("variant", true), new("status", true),
        new("error"), new("source", true), new("detect_seconds", false, ColumnKind.Number), new("gliner_seconds", false, ColumnKind.Number),
        new("write_seconds", false, ColumnKind.Number), new("prompt_tokens", false, ColumnKind.Int), new("output_tokens", false, ColumnKind.Int),
        new("discarded", false, ColumnKind.Int), new("present", false, ColumnKind.Int), new("caught", false, ColumnKind.Int),
        new("entities_present", false, ColumnKind.Int), new("entities_fully_caught", false, ColumnKind.Int), new("lost_to_extraction", false, ColumnKind.Int),
        new("edits", false, ColumnKind.Int), new("true_positives", false, ColumnKind.Int), new("type_correct", false, ColumnKind.Int),
        new("unjudged", false, ColumnKind.Int), new("flags_raised", false, ColumnKind.Int), new("flags_correct", false, ColumnKind.Int),
        new("preserve_total", false, ColumnKind.Int), new("preserve_broken", false, ColumnKind.Int), new("output_ok", false, ColumnKind.Bool),
        new("output_error"), new("settings_hash"),
    ];

    public static readonly Column[] Spans =
    [
        new("span_id", true), new("result_id", true), new("type", true), new("start", true, ColumnKind.Int), new("length", true, ColumnKind.Int),
        new("confidence", false, ColumnKind.Number), new("source", true), new("flag", true, ColumnKind.Bool),
    ];

    public static readonly Column[] Outcomes =
    [
        new("outcome_id", true), new("result_id", true), new("doc_id", true), new("kind", true), new("entity_id"), new("span_id"), new("type"), new("text"),
        new("start", false, ColumnKind.Int), new("length", false, ColumnKind.Int),
    ];

    /// <summary>The CSV files and their columns.</summary>
    public static readonly IReadOnlyDictionary<string, Column[]> Files = new Dictionary<string, Column[]>
    {
        [DocumentsFile] = Documents,
        [EntitiesFile] = Entities,
        [ResultsFile] = Results,
        [SpansFile] = Spans,
        [OutcomesFile] = Outcomes,
    };
}
