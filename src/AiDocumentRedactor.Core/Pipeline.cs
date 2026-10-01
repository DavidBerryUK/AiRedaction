using System.Diagnostics;

namespace AiDocumentRedactor.Core;

/// <summary>Runs one document: read -> detect -> redact -> write. Shared by CLI and UI.</summary>
public class RedactionPipeline(
    IEnumerable<IDocumentReader> readers, IEnumerable<IDocumentWriter> writers,
    IEntityDetector detector, RedactorOptions options)
{
    /// <summary>Reads the file, finds sensitive spans, redacts them and (if outputPath is given) writes the result. Reports progress; failures and cancellation are reported then rethrown.</summary>
    public async Task<RedactionResult> RunAsync(string inputPath, string? outputPath,
        IProgress<RedactionProgress>? progress, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        /// <summary>Sends a progress update to the caller, if one is listening.</summary>
        void Report(RedactionStage st, string msg, int found = 0, int step = 0, int total = 0) =>
            progress?.Report(new RedactionProgress(st, msg, step, total, found, sw.Elapsed));
        try
        {
            Report(RedactionStage.Reading, $"Reading {Path.GetFileName(inputPath)}…");
            var reader = readers.FirstOrDefault(r => r.CanRead(inputPath))
                         ?? throw new NotSupportedException($"Unsupported file type: {Path.GetExtension(inputPath)}");
            var doc = await reader.ReadAsync(inputPath, ct);

            var spans = await detector.DetectAsync(doc.Text, progress, ct);

            Report(RedactionStage.Redacting, "Applying redactions", spans.Count);
            var result = Redactor.Apply(doc.Text, spans, options.Redaction.PlaceholderTemplate);

            if (outputPath is not null)
            {
                Report(RedactionStage.Writing, "Writing output", result.Edits.Count);
                var writer = writers.FirstOrDefault(w => w.CanWrite(doc))
                             ?? throw new NotSupportedException($"No writer for {doc.Format}");
                await writer.WriteAsync(doc, result, outputPath, ct);
            }
            Report(RedactionStage.Done, $"{result.Edits.Count} edits in {sw.Elapsed:mm\\:ss}", result.Edits.Count);
            return result;
        }
        catch (OperationCanceledException) { Report(RedactionStage.Cancelled, "Cancelled"); throw; }
        catch (Exception ex) { Report(RedactionStage.Failed, ex.Message); throw; }
    }

    /// <summary>Where this input's redacted file goes: output folder + same sub-folders + name with the suffix.</summary>
    public string OutputPathFor(string inputPath, string inputRoot)
    {
        var rel = Path.GetRelativePath(inputRoot, inputPath);
        var dir = options.Output.MirrorFolders ? Path.GetDirectoryName(rel) ?? "" : "";
        var name = Path.GetFileNameWithoutExtension(rel) + options.Output.Suffix + Path.GetExtension(rel);
        return Path.Combine(options.Output.Directory, dir, name);
    }
}
