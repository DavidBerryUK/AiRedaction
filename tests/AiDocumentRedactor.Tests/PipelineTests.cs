using AiDocumentRedactor.Core;
using AiDocumentRedactor.Documents;
using Xunit;

namespace AiDocumentRedactor.Tests;

/// <summary>End-to-end test of read, detect, redact and write for a text file.</summary>
public class PipelineTests
{
    /// <summary>Detector that always finds "Sarah Jones".</summary>
    class FakeDetector : IEntityDetector
    {
        /// <summary>Finds the one known name.</summary>
        public Task<IReadOnlyList<DetectedEntity>> DetectAsync(string text, IProgress<RedactionProgress>? p, CancellationToken ct)
        {
            var i = text.IndexOf("Sarah Jones");
            return Task.FromResult<IReadOnlyList<DetectedEntity>>([new(EntityTypes.Person, i, 11, 1, "fake")]);
        }
    }

    /// <summary>Output is named with -redacted, contains the placeholder, no original name, and the input is untouched.</summary>
    [Fact]
    public async Task Redacts_text_file_into_output_with_suffix_and_no_original_text()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var inDir = Directory.CreateDirectory(Path.Combine(root, "in")).FullName;
        var options = new RedactorOptions { Output = { Directory = Path.Combine(root, "out") } };
        var input = Path.Combine(inDir, "letter.txt");
        File.WriteAllText(input, "Dear Sarah Jones, hello.");
        var pipeline = new RedactionPipeline([new TextDocumentReader()], [new TextDocumentWriter()], new FakeDetector(), options);

        var target = pipeline.OutputPathFor(input, inDir);
        await pipeline.RunAsync(input, target, null, default);

        Assert.EndsWith("letter-redacted.txt", target);
        var text = File.ReadAllText(target);
        Assert.Equal("Dear [REDACTED:PERSON], hello.", text);
        Assert.DoesNotContain("Sarah", text);
        Assert.Equal("Dear Sarah Jones, hello.", File.ReadAllText(input)); // input untouched
        Directory.Delete(root, true);
    }
}
