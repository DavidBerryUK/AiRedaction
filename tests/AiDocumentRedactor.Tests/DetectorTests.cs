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
        Assert.Equal(1, call.Number);
        Assert.Equal(0, call.ChunkStart);
        Assert.Equal(26, call.ChunkLength);
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

    /// <summary>With overlap, every chunk after the first starts inside the previous one, the pieces still cover the whole text, and none is too big.</summary>
    [Fact]
    public void Chunker_overlap_repeats_the_end_of_each_chunk_without_leaving_gaps()
    {
        var text = string.Join(" ", Enumerable.Range(0, 400).Select(i => $"word{i}"));
        var chunks = Chunker.Split(text, 500, 100);
        Assert.True(chunks.Count > 3);
        for (var i = 1; i < chunks.Count; i++)
        {
            var prevEnd = chunks[i - 1].Start + chunks[i - 1].Text.Length;
            Assert.True(chunks[i].Start < prevEnd, "next chunk should start before the previous one ends");
            Assert.True(chunks[i].Start >= chunks[i - 1].Start + 1);
            Assert.Equal(' ', text[chunks[i].Start - 1]);                 // starts at a word boundary, never mid-word
        }
        Assert.Equal(0, chunks[0].Start);
        Assert.Equal(text.Length, chunks[^1].Start + chunks[^1].Text.Length);
        Assert.All(chunks, c => { Assert.True(c.Text.Length <= 500); Assert.Equal(text.Substring(c.Start, c.Text.Length), c.Text); });
    }

    /// <summary>After the full name is found, the surname alone and a company without its legal ending are redacted too, marked as variants.</summary>
    [Fact]
    public async Task Short_forms_of_found_names_are_redacted_and_marked_as_variants()
    {
        var d = Make("{\"entities\":[{\"type\":\"PERSON\",\"text\":\"Tomasz Kowalczyk\"},{\"type\":\"COMPANY\",\"text\":\"Brightwater Analytics Ltd\"}]}");
        var text = "Tomasz Kowalczyk joined Brightwater Analytics Ltd. Mr Kowalczyk wrote to Brightwater Analytics. Tomasz is happy. Later Kowalczyk left.";
        var spans = await d.DetectAsync(text, null, default);
        string Found(DetectedEntity s) => text.Substring(s.Start, s.Length);
        Assert.Contains(spans, s => Found(s) == "Kowalczyk" && s.Source == "llm-variant");
        Assert.Contains(spans, s => Found(s) == "Tomasz" && s.Source == "llm-variant");
        Assert.Contains(spans, s => Found(s) == "Brightwater Analytics" && s.Source == "llm-variant" && s.Start > 60);
        Assert.Equal(1, spans.Count(s => Found(s) == "Tomasz Kowalczyk"));
        Assert.All(spans.Where(s => Found(s) == "Tomasz Kowalczyk"), s => Assert.Equal("llm", s.Source));
    }

    /// <summary>A name part that is also an ordinary lower-case word in the document is not turned into a variant.</summary>
    [Fact]
    public async Task Variants_skip_name_parts_that_are_ordinary_words_and_match_whole_words_only()
    {
        var d = Make("{\"entities\":[{\"type\":\"PERSON\",\"text\":\"Will Smith\"}]}");
        var text = "Will Smith said he will go. Will you come? Smithson stayed. Smith agreed.";
        var spans = await d.DetectAsync(text, null, default);
        var found = spans.Select(s => text.Substring(s.Start, s.Length)).ToList();
        Assert.Contains("Smith", found);                       // surname alone is redacted
        Assert.DoesNotContain("Will", found.Where((_, i) => spans[i].Source == "llm-variant"));   // 'will' is an ordinary word here
        Assert.DoesNotContain(spans, s => s.Source == "llm-variant" && s.Start == text.IndexOf("Smithson"));   // not inside another word
    }

    /// <summary>A category set to "flag" is reported for review but marked so it is not redacted.</summary>
    [Fact]
    public async Task Categories_in_flag_mode_are_marked_as_flagged()
    {
        var o = new RedactorOptions { Entities = { ["CONTEXTUAL"] = new EntityOptions { Mode = "flag" } } };
        var d = Make("{\"entities\":[{\"type\":\"CONTEXTUAL\",\"text\":\"head of compliance\"},{\"type\":\"PERSON\",\"text\":\"Sarah Jones\"}]}", o);
        var spans = await d.DetectAsync("Sarah Jones is head of compliance.", null, default);
        Assert.True(spans.Single(s => s.Type == "CONTEXTUAL").Flag);
        Assert.False(spans.First(s => s.Type == "PERSON" && s.Source == "llm").Flag);
    }

    /// <summary>The prompt tells the model not to return pronouns unless the pronoun switch is on, in which case they are added to GENDER.</summary>
    [Fact]
    public void Pronoun_switch_changes_the_prompt()
    {
        var off = PromptBuilder.System(new RedactorOptions());
        Assert.Contains("Do not return pronouns", off);
        var on = PromptBuilder.System(new RedactorOptions { Entities = { ["GENDER"] = new EntityOptions { RedactPronouns = true } } });
        Assert.DoesNotContain("Do not return pronouns", on);
        Assert.Contains("also gendered pronouns", on);
    }

    /// <summary>The place switch changes the prompt: place names on their own are excluded by default and included when it is on.</summary>
    [Fact]
    public void Place_switch_changes_the_prompt()
    {
        var off = PromptBuilder.System(new RedactorOptions());
        Assert.Contains("Do not return the name of a city", off);
        var on = PromptBuilder.System(new RedactorOptions { Entities = { ["ADDRESS"] = new EntityOptions { RedactPlaces = true } } });
        Assert.DoesNotContain("Do not return the name of a city", on);
        Assert.Contains("also the names of cities, towns, regions and countries", on);
    }
}
