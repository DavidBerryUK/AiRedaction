using System.Net.Http.Json;
using System.Text.Json;
using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.Detection;

/// <summary>LLM detection via a local Ollama server. The model returns quoted strings; this class
/// locates them (never trusts offsets), discards non-verbatim answers, and propagates repeats.</summary>
public class OllamaDetector(HttpClient http, RedactorOptions options) : IEntityDetector, IDetectorMetrics, IDetectorTrace
{
    /// <summary>Answers dropped because they were not in the text (the model made them up or altered them).</summary>
    public int Discarded
    {
        get; private set;
    }
    /// <summary>Every call made to the model, with its raw reply and outcome (for the prompt inspector).</summary>
    public IReadOnlyList<ModelCall> Calls => calls;
    readonly List<ModelCall> calls = [];

    /// <summary>Tokens sent to the model so far.</summary>
    public long PromptTokens
    {
        get; private set;
    }
    /// <summary>Tokens the model generated so far.</summary>
    public long OutputTokens
    {
        get; private set;
    }

    /// <summary>One thing the model found: its category and the exact text.</summary>
    public record Item(string Type, string Text);

    /// <summary>Creates the HTTP client for the model server; refuses a non-local address unless the config allows it.</summary>
    public static HttpClient CreateClient(LlmOptions llm)
    {
        var uri = new Uri(llm.Endpoint);
        if (!uri.IsLoopback && !llm.AllowRemoteEndpoint)
        {
            throw new InvalidOperationException($"Refusing non-local Ollama endpoint {uri.Host}. Set llm.allowRemoteEndpoint to override.");
        }

        return new HttpClient { BaseAddress = uri, Timeout = TimeSpan.FromSeconds(llm.TimeoutSeconds) };
    }

    /// <summary>FR13: Ollama reachable and model installed.</summary>
    public async Task CheckAvailableAsync(CancellationToken ct)
    {
        try
        {
            var tags = await http.GetFromJsonAsync<JsonElement>("/api/tags", ct);
            var names = tags.GetProperty("models").EnumerateArray().Select(m => m.GetProperty("name").GetString()!).ToList();
            var want = options.Llm.Model;
            if (!names.Any(n => n == want || n == want + ":latest"))
            {
                throw new InvalidOperationException($"Model '{want}' is not installed. Run: ollama pull {want}");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Cannot reach Ollama at {options.Llm.Endpoint}: {ex.Message}");
        }
    }

    /// <summary>Splits the text, asks the model about each piece, keeps only answers found verbatim, then finds every occurrence of each answer across the document.</summary>
    public async Task<IReadOnlyList<DetectedEntity>> DetectAsync(string text, IProgress<RedactionProgress>? progress, CancellationToken ct)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var chunks = Chunker.Split(text, options.Llm.ChunkChars, options.Llm.ChunkOverlapChars);
        var found = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); // text -> type
        var allow = new HashSet<string>(options.CustomTerms.Allow, StringComparer.OrdinalIgnoreCase);
        var spans = new List<DetectedEntity>();
        progress?.Report(new(RedactionStage.Chunking, $"Split into {chunks.Count} chunk(s)", 0, chunks.Count, 0, sw.Elapsed));

        for (var i = 0; i < chunks.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var chunk = chunks[i];
            var ask = await AskAsync(chunk.Text, ct);
            var fresh = new List<DetectedEntity>();
            var outcomes = new List<CallItem>();
            foreach (var it in ask.Items)
            {
                var t = it.Text.Trim();
                if (t.Length < 2)
                {
                    outcomes.Add(new(it.Type, it.Text, "ignored: too short", false));
                    continue;
                }
                if (allow.Contains(t))
                {
                    outcomes.Add(new(it.Type, it.Text, "ignored: on the allow-list", false));
                    continue;
                }
                if (!chunk.Text.Contains(t, StringComparison.Ordinal))   // hallucination guard
                {
                    Discarded++;
                    outcomes.Add(new(it.Type, it.Text, "discarded: not found word-for-word in the text", false));
                    continue;
                }
                if (found.TryAdd(t, it.Type))
                {
                    fresh.AddRange(Locate(chunk.Text, t, it.Type, chunk.Start));
                    outcomes.Add(new(it.Type, it.Text, "kept", true));
                }
                else
                {
                    outcomes.Add(new(it.Type, it.Text, "kept (already found in an earlier chunk)", true));
                }
            }
            calls.Add(new ModelCall(i + 1, chunk.Start, chunk.Text.Length, ask.UserMessage, ask.RawReply, outcomes,
                ask.PromptTokens, ask.OutputTokens, ask.Elapsed, ask.Attempts));
            spans.AddRange(fresh);
            progress?.Report(new(RedactionStage.Detecting, $"Detecting (chunk {i + 1} of {chunks.Count})", i + 1, chunks.Count, spans.Count, sw.Elapsed, fresh));
        }

        progress?.Report(new(RedactionStage.Locating, $"Locating and propagating {found.Count} item(s)", 0, 0, spans.Count, sw.Elapsed));
        foreach (var (t, type) in found)            // propagate to every occurrence in the whole document
        {
            spans.AddRange(Locate(text, t, type, 0));
        }

        foreach (var (variant, type) in Variants(found, text, allow))   // shorter forms: a surname alone, a company without "Ltd"
        {
            spans.AddRange(LocateWord(text, variant, type, "llm-variant"));
        }

        foreach (var term in options.CustomTerms.Redact.Where(x => x.Length > 1))
        {
            spans.AddRange(Locate(text, term, EntityTypes.Other, 0, "custom-list"));
        }
        // Categories set to "flag" are listed for review but left in the text.
        var flagTypes = PromptBuilder.DefaultDescriptions.Keys.Where(t => options.ModeOf(t) == "flag").ToHashSet();
        return spans.Select(s => flagTypes.Contains(s.Type) ? s with { Flag = true } : s).DistinctBy(s => (s.Start, s.Length)).ToList();
    }

    static readonly HashSet<string> Titles = new(StringComparer.OrdinalIgnoreCase) { "mr", "mrs", "ms", "miss", "mx", "dr", "prof", "sir", "madam", "lord", "lady" };
    static readonly HashSet<string> LegalSuffixes = new(StringComparer.OrdinalIgnoreCase) { "ltd", "limited", "plc", "llc", "llp", "inc", "corp", "corporation", "co", "gmbh", "ag", "as", "bv", "sa", "pty" };

    /// <summary>Shorter forms of what the model found, because people and companies are usually named in full once and then briefly:
    /// each part of a full name (a surname alone), and a company without its legal ending. A part is skipped if the same word also
    /// appears in lower case in the document (so a name like "Will Smith" does not redact every "will"), and variants are matched
    /// as whole words with the same capitalisation.</summary>
    public static IEnumerable<(string Text, string Type)> Variants(IEnumerable<KeyValuePair<string, string>> found, string text, ISet<string> allow)
    {
        var known = new HashSet<string>(found.Select(f => f.Key), StringComparer.OrdinalIgnoreCase);
        bool CommonWord(string w) => System.Text.RegularExpressions.Regex.IsMatch(text, $@"(?<![\p{{L}}\p{{N}}]){System.Text.RegularExpressions.Regex.Escape(w.ToLowerInvariant())}(?![\p{{L}}\p{{N}}])");
        foreach (var (t, type) in found.Select(f => (f.Key, f.Value)))
        {
            var words = t.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries).Select(w => w.Trim(',', '.', ';')).Where(w => w.Length > 0).ToList();
            if (type == EntityTypes.Person)
            {
                var name = words.Where(w => !Titles.Contains(w)).ToList();
                if (name.Count < 2)
                {
                    continue;
                }

                foreach (var part in name.Where(w => w.Length >= 3 && char.IsUpper(w[0]) && w.All(c => char.IsLetter(c) || c is '-' or '\'')))
                {
                    if (!known.Contains(part) && !allow.Contains(part) && !CommonWord(part))
                    {
                        known.Add(part);
                        yield return (part, type);
                    }
                }
            }
            else if (type == EntityTypes.Company)
            {
                var core = words.ToList();
                while (core.Count > 1 && LegalSuffixes.Contains(core[^1]))
                {
                    core.RemoveAt(core.Count - 1);
                }

                var coreText = string.Join(' ', core);
                if (core.Count < words.Count && coreText.Length >= 4 && !known.Contains(coreText) && !allow.Contains(coreText))
                {
                    known.Add(coreText);
                    yield return (coreText, type);
                }
                if (core.Count >= 2 && core[0].Length >= 5 && char.IsUpper(core[0][0]) && !known.Contains(core[0]) && !allow.Contains(core[0]) && !CommonWord(core[0]))
                {
                    known.Add(core[0]);
                    yield return (core[0], type);
                }
            }
        }
    }

    /// <summary>Finds whole-word, same-capitalisation occurrences of a variant (so "Kowalczyk" does not match inside another word).</summary>
    public static IEnumerable<DetectedEntity> LocateWord(string text, string needle, string type, string source) =>
        System.Text.RegularExpressions.Regex.Matches(text, $@"(?<![\p{{L}}\p{{N}}]){System.Text.RegularExpressions.Regex.Escape(needle)}(?![\p{{L}}\p{{N}}])")
            .Select(m => new DetectedEntity(type, m.Index, m.Length, 0.7, source));

    /// <summary>Finds every occurrence (ignoring case) of a string and returns it as spans.</summary>
    public static IEnumerable<DetectedEntity> Locate(string text, string needle, string type, int offset, string source = "llm")
    {
        for (var i = text.IndexOf(needle, StringComparison.OrdinalIgnoreCase); i >= 0;
             i = text.IndexOf(needle, i + needle.Length, StringComparison.OrdinalIgnoreCase))
        {
            yield return new DetectedEntity(type, offset + i, needle.Length, 1.0, source);
        }
    }

    /// <summary>What one model call produced: the parsed items plus everything the inspector needs.</summary>
    record AskResult(List<Item> Items, string UserMessage, string RawReply, long PromptTokens, long OutputTokens, TimeSpan Elapsed, int Attempts);

    /// <summary>Sends one piece of text to the model and returns what it found; retries once if the reply is not valid JSON.</summary>
    async Task<AskResult> AskAsync(string chunkText, CancellationToken ct)
    {
        var types = PromptBuilder.EnabledTypes(options).ToList();
        var userMessage = PromptBuilder.UserMessage(chunkText);
        var body = new {
            model = options.Llm.Model,
            stream = false,
            keep_alive = options.Llm.KeepAlive,
            format = PromptBuilder.Schema(types),
            options = new {
                temperature = options.Llm.Temperature,
                seed = options.Llm.Seed,
                num_ctx = options.Llm.NumCtx
            },
            messages = new[]
            {
                new { role = "system", content = PromptBuilder.System(options) },
                new { role = "user", content = userMessage },
            },
        };
        var sw = System.Diagnostics.Stopwatch.StartNew();
        long pt = 0, ot = 0;
        for (var attempt = 1; ; attempt++)
        {
            using var resp = await http.PostAsJsonAsync("/api/chat", body, ct);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadFromJsonAsync<JsonElement>(ct);
            if (json.TryGetProperty("prompt_eval_count", out var pe))
            {
                PromptTokens += pe.GetInt64();
                pt += pe.GetInt64();
            }
            if (json.TryGetProperty("eval_count", out var ec))
            {
                OutputTokens += ec.GetInt64();
                ot += ec.GetInt64();
            }
            var raw = json.GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
            try
            {
                return new AskResult(Parse(raw), userMessage, raw, pt, ot, sw.Elapsed, attempt);
            }
            catch (JsonException) when (attempt == 1) { /* retry once, then fail the file */ }
        }
    }

    /// <summary>Turns the model's JSON reply into a list of items.</summary>
    public static List<Item> Parse(string content)
    {
        using var doc = JsonDocument.Parse(content);
        return doc.RootElement.GetProperty("entities").EnumerateArray()
            .Select(e => new Item(e.GetProperty("type").GetString() ?? EntityTypes.Other, e.GetProperty("text").GetString() ?? string.Empty))
            .ToList();
    }
}
