namespace AiDocumentRedactor.Eval.Dataset;

/// <summary>A row of documents.csv. <c>DocId</c> is the file's path relative to the input folder; <c>KeyId</c> says which answer key applies (several files can share one).</summary>
public record DocumentRow(string DocId, string KeyId, string Corpus, string FormatGroup, string DocType, int Chars, string TextHash, int EntityCount, int OccurrenceCount)
{
    public string?[] Cells() => [DocId, KeyId, Corpus, FormatGroup, DocType, Csv.Num(Chars), TextHash, Csv.Num(EntityCount), Csv.Num(OccurrenceCount)];
}

/// <summary>A row of entities.csv: something the answer key says must be removed, must be kept, or cannot be decided.</summary>
public record EntityRow(string EntityId, string KeyId, string Role, string Type, string Text, int? Occurrences, string Where, string Origin, bool? Judged)
{
    public string?[] Cells() => [EntityId, KeyId, Role, Type, Text, Csv.Num(Occurrences), Where, Origin, Csv.Bool(Judged)];
}

/// <summary>A row of results.csv: one detector setup on one document. The counts are empty when the run failed (see <c>Status</c>).</summary>
public class ResultRow
{
    public string ResultId = string.Empty, DocId = string.Empty, Config = string.Empty, Model = string.Empty, Variant = "plain", Status = "ok", Error = string.Empty, Source = "batch";
    public int Repeat = 1;
    public double? DetectSeconds, GlinerSeconds, WriteSeconds;
    public long? PromptTokens, OutputTokens;
    public int? Discarded, Present, Caught, EntitiesPresent, EntitiesFullyCaught, LostToExtraction, Edits, TruePositives, TypeCorrect, Unjudged, FlagsRaised, FlagsCorrect, PreserveTotal, PreserveBroken;
    public bool? OutputOk;
    public string OutputError = string.Empty, SettingsHash = string.Empty;

    public string?[] Cells() =>
    [
        ResultId, DocId, Config, Csv.Num(Repeat), Model, Variant, Status, Error, Source, Csv.Num(DetectSeconds), Csv.Num(GlinerSeconds), Csv.Num(WriteSeconds),
        Csv.Num(PromptTokens), Csv.Num(OutputTokens), Csv.Num(Discarded), Csv.Num(Present), Csv.Num(Caught), Csv.Num(EntitiesPresent), Csv.Num(EntitiesFullyCaught),
        Csv.Num(LostToExtraction), Csv.Num(Edits), Csv.Num(TruePositives), Csv.Num(TypeCorrect), Csv.Num(Unjudged), Csv.Num(FlagsRaised), Csv.Num(FlagsCorrect),
        Csv.Num(PreserveTotal), Csv.Num(PreserveBroken), Csv.Bool(OutputOk), OutputError, SettingsHash,
    ];
}

/// <summary>A row of spans.csv: one redaction or flag a detector produced, as a position in the document's text.</summary>
public record SpanRow(string SpanId, string ResultId, string Type, int Start, int Length, double? Confidence, string Source, bool Flag)
{
    public string?[] Cells() => [SpanId, ResultId, Type, Csv.Num(Start), Csv.Num(Length), Csv.Num(Confidence), Source, Csv.Bool(Flag)];
}

/// <summary>A row of outcomes.csv: one judged fact about a result (caught, missed, over-redacted, flagged and so on).</summary>
public record OutcomeRow(string OutcomeId, string ResultId, string DocId, string Kind, string EntityId, string SpanId, string Type, string Text, int? Start, int? Length)
{
    public string?[] Cells() => [OutcomeId, ResultId, DocId, Kind, EntityId, SpanId, Type, Text, Csv.Num(Start), Csv.Num(Length)];
}

/// <summary>A line of document-text.jsonl: the text the detectors received, with a hash so a reader can tell it is the text the spans refer to.</summary>
public record DocumentText(string DocId, string TextHash, string Text);

/// <summary>Everything in a dataset, ready to write.</summary>
public class DatasetData
{
    public Dictionary<string, object?> Run = [];
    public List<DocumentRow> Documents = [];
    public List<DocumentText> Texts = [];
    public List<EntityRow> Entities = [];
    public List<ResultRow> Results = [];
    public List<SpanRow> Spans = [];
    public List<OutcomeRow> Outcomes = [];
}
