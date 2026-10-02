using System.Net;
using System.Text;
using AiDocumentRedactor.Core;
using AiDocumentRedactor.Detection;
using Xunit;

namespace AiDocumentRedactor.Tests;

/// <summary>Tests for the phase 1 accuracy changes: fixed rules, spacing-tolerant matching and the empty-reply guard.</summary>
public class RuleTests
{
    static List<string> Found(string text, RedactorOptions? o = null) =>
        RuleDetector.Find(text, o ?? new RedactorOptions()).Select(e => $"{e.Type}:{text.Substring(e.Start, e.Length)}").ToList();

    /// <summary>Structured items are found by rule, and numbers with check digits must pass them.</summary>
    [Fact]
    public void Rules_find_structured_items_and_check_digits()
    {
        var f = Found("Email a.b@example.com or accounts @fernleigh.example, call 07700 900123 or 0113 496 0123. " +
                      "Post to Leeds LS6 2QT. NHS number 943 476 5919, bad 943 476 5918. NI QQ 12 34 56 C. " +
                      "IBAN GB82 WEST 1234 5698 7654 32. Sort code 20-00-00, account ending 4821. Card 4111 1111 1111 1111. IP 203.0.113.45.");
        Assert.Contains("EMAIL:a.b@example.com", f);
        Assert.Contains("EMAIL:accounts @fernleigh.example", f);
        Assert.Contains("PHONE:07700 900123", f);
        Assert.Contains("PHONE:0113 496 0123", f);
        Assert.Contains("ADDRESS:LS6 2QT", f);
        Assert.Contains("ID_NUMBER:943 476 5919", f);
        Assert.DoesNotContain("ID_NUMBER:943 476 5918", f);
        Assert.Contains("ID_NUMBER:QQ 12 34 56 C", f);
        Assert.Contains("ID_NUMBER:GB82 WEST 1234 5698 7654 32", f);
        Assert.Contains("ID_NUMBER:20-00-00", f);
        Assert.Contains("ID_NUMBER:4821", f);
        Assert.Contains("ID_NUMBER:4111 1111 1111 1111", f);
        Assert.Contains("ONLINE_ID:203.0.113.45", f);
    }

    /// <summary>Gender words are found; pronouns only when the GENDER category asks for them; switched-off categories are skipped.</summary>
    [Fact]
    public void Gender_words_pronouns_and_switched_off_categories()
    {
        const string text = "Dear Ms Whitcombe, the man said he would call her.";
        var plain = Found(text);
        Assert.Contains("GENDER:Ms", plain);
        Assert.Contains("GENDER:man", plain);
        Assert.DoesNotContain("GENDER:he", plain);

        var o = new RedactorOptions();
        o.Entities["GENDER"] = new EntityOptions { RedactPronouns = true };
        var withPronouns = Found(text, o);
        Assert.Contains("GENDER:he", withPronouns);
        Assert.Contains("GENDER:her", withPronouns);

        o.Entities["GENDER"] = new EntityOptions { Enabled = false };
        Assert.Empty(Found(text, o));
    }

    /// <summary>Fake model that returns the given replies in turn.</summary>
    class SequenceHandler(params string[] replies) : HttpMessageHandler
    {
        int next;
        public int Calls => next;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
        {
            var inner = System.Text.Json.JsonSerializer.Serialize(replies[Math.Min(next++, replies.Length - 1)]);
            var body = "{\"message\":{\"content\":" + inner + "},\"prompt_eval_count\":10,\"eval_count\":5}";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") });
        }
    }

    static OllamaDetector Make(HttpMessageHandler h, RedactorOptions? o = null) =>
        new(new HttpClient(h) { BaseAddress = new Uri("http://localhost:11434") }, o ?? new RedactorOptions());

    /// <summary>A name the model returns with a space is still found where the document wraps it onto a new line.</summary>
    [Fact]
    public async Task Answer_with_a_space_matches_text_with_a_line_break()
    {
        const string text = "Please contact Dr Helen Okafor at Fernleigh\nSurgery with your findings.";
        var d = Make(new SequenceHandler("""{"entities":[{"type":"COMPANY","text":"Fernleigh Surgery"}]}"""));
        var spans = await d.DetectAsync(text, null, CancellationToken.None);
        Assert.Contains(spans, s => s.Type == "COMPANY" && text.Substring(s.Start, s.Length) == "Fernleigh\nSurgery");
        Assert.Equal(0, d.Discarded);
    }

    /// <summary>An empty answer for text that plainly has names in it is asked again, and the second answer is used.</summary>
    [Fact]
    public async Task Empty_reply_for_sensitive_text_is_retried()
    {
        const string text = "Jane Smith met Tom Brown in the office on Monday.";
        var h = new SequenceHandler("""{"entities":[]}""", """{"entities":[{"type":"PERSON","text":"Jane Smith"},{"type":"PERSON","text":"Tom Brown"}]}""");
        var d = Make(h);
        var spans = await d.DetectAsync(text, null, CancellationToken.None);
        Assert.Equal(2, h.Calls);
        Assert.Equal(1, d.EmptyRetries);
        Assert.Contains(spans, s => text.Substring(s.Start, s.Length) == "Jane Smith");
    }

    /// <summary>An empty answer for text with nothing sensitive in it is accepted without asking again.</summary>
    [Fact]
    public async Task Empty_reply_for_plain_text_is_accepted()
    {
        var h = new SequenceHandler("""{"entities":[]}""");
        await Make(h).DetectAsync("the weather was fine and the meeting ended early.", null, CancellationToken.None);
        Assert.Equal(1, h.Calls);
    }
}
