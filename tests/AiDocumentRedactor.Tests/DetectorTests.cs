using System.Net;
using System.Text;
using AiDocumentRedactor.Core;
using AiDocumentRedactor.Detection;
using Xunit;

namespace AiDocumentRedactor.Tests;

/// <summary>Tests for the Ollama detector, using a stub in place of the real model.</summary>
public class DetectorTests
{
    /// <summary>Fake HTTP handler that returns a fixed model reply instead of calling Ollama.</summary>
    class StubHandler(string entitiesJson) : HttpMessageHandler
    {
        /// <summary>Returns the canned reply wrapped like a real Ollama chat response.</summary>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
        {
            var inner = System.Text.Json.JsonSerializer.Serialize(entitiesJson);
            var body = "{\"message\":{\"content\":" + inner + "},\"prompt_eval_count\":10,\"eval_count\":5}";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") });
        }
    }

    /// <summary>Creates a detector wired to the stub reply.</summary>
    static OllamaDetector Make(string json, RedactorOptions? o = null) =>
        new(new HttpClient(new StubHandler(json)) { BaseAddress = new Uri("http://localhost:11434") }, o ?? new RedactorOptions());

    /// <summary>Every occurrence of a found string is redacted (any case), and strings the model made up are discarded.</summary>
    [Fact]
    public async Task Locates_and_propagates_every_occurrence_and_discards_hallucinations()
    {
        var d = Make("{\"entities\":[{\"type\":\"PERSON\",\"text\":\"Sarah Jones\"},{\"type\":\"PERSON\",\"text\":\"Not In Text\"}]}");
        var text = "Sarah Jones wrote. Later, sarah jones called.";
        var spans = await d.DetectAsync(text, null, default);
        Assert.Equal(2, spans.Count);
        Assert.Equal(1, d.Discarded);
    }

    /// <summary>Terms on the allow-list are never redacted, even if the model returns them.</summary>
    [Fact]
    public async Task Allow_list_terms_are_not_redacted()
    {
        var o = new RedactorOptions { CustomTerms = { Allow = ["HMRC"] } };
        var d = Make("{\"entities\":[{\"type\":\"COMPANY\",\"text\":\"HMRC\"}]}", o);
        Assert.Empty(await d.DetectAsync("Write to HMRC today.", null, default));
    }

    /// <summary>Each model call is recorded with its raw reply, and every returned item gets an outcome (kept or why dropped).</summary>
    [Fact]
    public async Task Records_each_call_with_outcomes_for_every_item()
    {
        var o = new RedactorOptions { CustomTerms = { Allow = ["HMRC"] } };
        var d = Make("{\"entities\":[{\"type\":\"PERSON\",\"text\":\"Sarah Jones\"},{\"type\":\"PERSON\",\"text\":\"Made Up\"},{\"type\":\"COMPANY\",\"text\":\"HMRC\"},{\"type\":\"AGE\",\"text\":\"4\"}]}", o);
        await d.DetectAsync("Sarah Jones wrote to HMRC.", null, default);
        var call = Assert.Single(d.Calls);
        Assert.Equal(1, call.Number); Assert.Equal(0, call.ChunkStart); Assert.Equal(26, call.ChunkLength);
        Assert.Contains("Sarah Jones wrote to HMRC.", call.UserMessage);
        Assert.Contains("Sarah Jones", call.RawReply);
        Assert.Equal(["kept", "discarded: not found word-for-word in the text", "ignored: on the allow-list", "ignored: too short"], call.Items.Select(i => i.Outcome));
        Assert.Equal([true, false, false, false], call.Items.Select(i => i.Kept));
    }

    /// <summary>The user message wraps the chunk between markers so the model treats it as data.</summary>
    [Fact]
    public void User_message_wraps_the_chunk_between_markers() =>
        Assert.Equal("Document text:\n<<<\nhello\n>>>", PromptBuilder.UserMessage("hello"));

    /// <summary>Chunks rejoin to the exact original text and none exceeds the size limit.</summary>
    [Fact]
    public void Chunker_covers_whole_text_without_gaps()
    {
        var text = string.Join("\n\n", Enumerable.Range(0, 50).Select(i => $"Paragraph {i} " + new string('x', 100)));
        var chunks = Chunker.Split(text, 500);
        Assert.Equal(text, string.Concat(chunks.Select(c => c.Text)));
        Assert.All(chunks, c => Assert.True(c.Text.Length <= 500));
    }

    /// <summary>A non-local model server is refused unless the config allows it.</summary>
    [Fact]
    public void Refuses_remote_endpoint_by_default() =>
        Assert.Throws<InvalidOperationException>(() => OllamaDetector.CreateClient(new LlmOptions { Endpoint = "http://example.com:11434" }));
}
