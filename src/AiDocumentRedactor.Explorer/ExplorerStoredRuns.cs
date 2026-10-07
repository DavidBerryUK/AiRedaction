namespace AiDocumentRedactor.Explorer;

/// <summary>One redaction or flag a detector produced, as a position in the document's text.</summary>
public record StoredSpan(string Type, int Start, int Length, double? Confidence, string Source, bool Flag);

/// <summary>How a stored result scored against the document's answer key, as the evaluation recorded it. Occurrences are single appearances in the text; items are the distinct things
/// on the key (one item can appear several times). Flag figures are null for a setup that used no GLiNER.</summary>
public record StoredScores(int Present, int Caught, int EntitiesPresent, int EntitiesFullyCaught, int LostToExtraction, int Edits, int TruePositives, int TypeCorrect, int Unjudged,
    int OverRedactions, int PreserveTotal, int PreserveBroken, int? FlagsRaised, int? FlagsCorrect, double DetectSeconds, double? GlinerSeconds);

/// <summary>One model's plain result for a document, as stored in a dataset: the spans it produced, how it scored and how long it took.</summary>
public record StoredRun(string ResultId, string Model, int Repeat, int EditCount, long PromptTokens, long OutputTokens, int Discarded, StoredScores Scores, IReadOnlyList<StoredSpan> Spans);

/// <summary>A document's stored model results: the text the detectors saw (and its hash, so a reader can prove it matches the file on disk) and one plain result per model.</summary>
public record StoredRuns(string DocId, string Text, string TextHash, IReadOnlyList<StoredRun> Runs);

public partial class ExplorerService
{
    /// <summary>The stored results for one document that a user can look at again: each model on its own (plain variant, no GLiNER, no baselines), succeeded, and from the batch
    /// evaluation (results made live in the explorer are left out). Where a model has several repeats, the highest repeat is kept. Null if the dataset has no such document.</summary>
    public async Task<StoredRuns?> StoredRunsAsync(string docId)
    {
        var doc = (await QueryAsync(cmd =>
        {
            cmd.Parameters.AddWithValue("@d", docId);
            return "SELECT d.text_hash, t.text FROM documents d JOIN texts t ON t.doc_id = d.doc_id WHERE d.doc_id = @d";
        }, r => (Hash: Str(r, 0), Text: Str(r, 1)))).FirstOrDefault();
        if (doc.Hash is null)
        {
            return null;
        }

        var rows = await QueryAsync(cmd =>
        {
            cmd.Parameters.AddWithValue("@d", docId);
            return "SELECT r.result_id, r.model, r.repeat, r.edits, r.prompt_tokens, r.output_tokens, r.discarded, r.present, r.caught, r.entities_present, r.entities_fully_caught, " +
                "r.lost_to_extraction, r.true_positives, r.type_correct, r.unjudged, " +
                "(SELECT COUNT(*) FROM outcomes o WHERE o.result_id = r.result_id AND o.kind = 'over_redaction'), r.preserve_total, r.preserve_broken, r.flags_raised, r.flags_correct, " +
                "r.detect_seconds, r.gliner_seconds FROM results r " +
                "WHERE r.doc_id = @d AND r.status = 'ok' AND r.source = 'batch' AND r.model <> '' AND r.variant = 'plain' ORDER BY r.model, r.repeat DESC";
        }, r => (Id: Str(r, 0), Model: Str(r, 1), Repeat: Int(r, 2), Edits: Int(r, 3), Prompt: (long)Int(r, 4), Output: (long)Int(r, 5), Discarded: Int(r, 6),
            Scores: new StoredScores(Int(r, 7), Int(r, 8), Int(r, 9), Int(r, 10), Int(r, 11), Int(r, 3), Int(r, 12), Int(r, 13), Int(r, 14), Int(r, 15), Int(r, 16), Int(r, 17),
                NInt(r, 18), NInt(r, 19), NDouble(r, 20) ?? 0, NDouble(r, 21))));
        var runs = new List<StoredRun>();
        foreach (var row in rows.GroupBy(x => x.Model).Select(g => g.First()))
        {
            var spans = await QueryAsync(cmd =>
            {
                cmd.Parameters.AddWithValue("@r", row.Id);
                return "SELECT type, start, length, confidence, source, flag FROM spans WHERE result_id = @r ORDER BY start, length DESC";
            }, r => new StoredSpan(Str(r, 0), Int(r, 1), Int(r, 2), NDouble(r, 3), Str(r, 4), r.GetInt64(5) == 1));
            runs.Add(new StoredRun(row.Id, row.Model, row.Repeat, row.Edits, row.Prompt, row.Output, row.Discarded, row.Scores, spans));
        }

        return new StoredRuns(docId, doc.Text, doc.Hash, runs);
    }
}
