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
    public record Item(string Type, string Text, string? Context = null);

    /// <summary>One accepted answer: its text and category, and where it sits if the model's context quote was found in the text (otherwise empty).</summary>
    record Use(string Text, string Type, List<DetectedEntity> Located);

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
        var found = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); // text -> type, for the live view while chunks are read
        var uses = new List<Use>();
        var allow = new HashSet<string>(options.CustomTerms.Allow, StringComparer.OrdinalIgnoreCase);
        var spans = new List<DetectedEntity>();
        var liveCount = 0;   // items shown so far in the live view (the final list is built after every chunk is read)
        progress?.Report(new(RedactionStage.Chunking, $"Split into {chunks.Count} chunk(s)", 0, chunks.Count, 0, sw.Elapsed));

        for (var i = 0; i < chunks.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var chunk = chunks[i];
            var ask = await AskAsync(chunk.Text, ct);
            if (ask.Items.Count == 0 && LooksSensitive(chunk.Text))
            {
                // An empty answer for text that plainly has names or identifiers in it is a silent failure, not a clean result:
                // ask again with a reminder, then (if one is configured) ask the fallback model.
                var retry = await AskAsync(chunk.Text, ct, nudge: true);
                if (retry.Items.Count == 0 && !string.IsNullOrWhiteSpace(options.Llm.FallbackModel))
                {
                    retry = await AskAsync(chunk.Text, ct, model: options.Llm.FallbackModel, nudge: true);
                }

                EmptyRetries++;
                ask = retry with { Attempts = ask.Attempts + retry.Attempts };
            }
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
                if (!TextMatch.Contains(chunk.Text, t))   // hallucination guard (spacing may differ: a PDF line break where the model wrote a space)
                {
                    Discarded++;
                    outcomes.Add(new(it.Type, it.Text, "discarded: not found word-for-word in the text", false));
                    continue;
                }
                var located = LocateInContext(chunk.Text, t, it.Context, it.Type, chunk.Start);
                uses.Add(new Use(t, it.Type, located));
                if (found.TryAdd(t, it.Type))
                {
                    fresh.AddRange(located.Count > 0 ? located : Locate(chunk.Text, t, it.Type, chunk.Start));
                    outcomes.Add(new(it.Type, it.Text, located.Count > 0 ? "kept (placed using its context)" : "kept", true));
                }
                else
                {
                    outcomes.Add(new(it.Type, it.Text, "kept (already found in an earlier chunk)", true));
                }
            }
            calls.Add(new ModelCall(i + 1, chunk.Start, chunk.Text.Length, ask.UserMessage, ask.RawReply, outcomes,
                ask.PromptTokens, ask.OutputTokens, ask.Elapsed, ask.Attempts));
            liveCount += fresh.Count;
            progress?.Report(new(RedactionStage.Detecting, $"Detecting (chunk {i + 1} of {chunks.Count})", i + 1, chunks.Count, liveCount, sw.Elapsed, fresh));
        }

        progress?.Report(new(RedactionStage.Locating, $"Locating and propagating {found.Count} item(s)", 0, 0, liveCount, sw.Elapsed));
        // A word the model labelled in more than one way, each time with a context that was found in the text (a person called Paris and the city),
        // is placed only where its contexts say. Any other word is propagated to every occurrence in the document, as before.
        var ambiguous = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var g in uses.GroupBy(u => u.Text, StringComparer.OrdinalIgnoreCase))
        {
            if (g.Select(u => u.Type).Distinct().Count() > 1 && g.All(u => u.Located.Count > 0))
            {
                ambiguous.Add(g.Key);
                spans.AddRange(g.SelectMany(u => u.Located));
            }
        }

        found.Clear();
        foreach (var u in uses.Where(u => !ambiguous.Contains(u.Text)))
        {
            found.TryAdd(u.Text, u.Type);
        }

        foreach (var (t, type) in found)
        {
            spans.AddRange(Locate(text, t, type, 0));
        }

        foreach (var (variant, type) in Variants(found, text, allow))   // shorter forms: a surname alone, a company without "Ltd"
        {
            spans.AddRange(LocateWord(text, variant, type, "llm-variant"));
        }

        if (options.Rules.Enabled)
        {
            spans.AddRange(RuleDetector.Find(text, options));
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

    /// <summary>Finds where an answer sits using the context quote the model gave with it: every place the quote occurs in the chunk, and the answer
    /// within it. Empty if there is no quote, the quote is not in the text word for word, or the answer is not inside it.</summary>
    public static List<DetectedEntity> LocateInContext(string chunkText, string answer, string? context, string type, int offset)
    {
        var result = new List<DetectedEntity>();
        var c = context?.Trim();
        if (string.IsNullOrEmpty(c) || !TextMatch.Contains(c, answer))
        {
            return result;
        }

        var answerPattern = TextMatch.Pattern(answer, ignoreCase: false, wholeWord: true);
        var loosePattern = TextMatch.Pattern(answer, ignoreCase: false, wholeWord: false);
        foreach (var (start, length) in TextMatch.Find(chunkText, c))
        {
            var region = chunkText.Substring(start, length);
            var inner = answerPattern.Match(region);
            inner = inner.Success ? inner : loosePattern.Match(region);
            if (inner.Success)
            {
                result.Add(new DetectedEntity(type, offset + start + inner.Index, inner.Length, 1.0, "llm"));
            }
        }

        return result;
    }

    /// <summary>Finds whole-word, same-capitalisation occurrences of a variant (so "Kowalczyk" does not match inside another word).</summary>
    public static IEnumerable<DetectedEntity> LocateWord(string text, string needle, string type, string source) =>
        System.Text.RegularExpressions.Regex.Matches(text, $@"(?<![\p{{L}}\p{{N}}]){System.Text.RegularExpressions.Regex.Escape(needle)}(?![\p{{L}}\p{{N}}])")
            .Select(m => new DetectedEntity(type, m.Index, m.Length, 0.7, source));

    /// <summary>Finds every occurrence (ignoring case, and allowing different spacing or line breaks) of a string and returns it as spans.</summary>
    public static IEnumerable<DetectedEntity> Locate(string text, string needle, string type, int offset, string source = "llm") =>
        TextMatch.Find(text, needle, ignoreCase: true).Select(m => new DetectedEntity(type, offset + m.Start, m.Length, 1.0, source));

    /// <summary>Chunks where the model came back empty and was asked again (a guard against silent failures).</summary>
    public int EmptyRetries
    {
        get; private set;
    }

    static readonly System.Text.RegularExpressions.Regex NamePair = new(@"(?<![\p{L}])\p{Lu}\p{Ll}+\s+\p{Lu}\p{Ll}+(?![\p{L}])");

    /// <summary>True if a piece of text plainly contains something a redactor should find: a pair of capitalised words (a likely name) or anything the fixed rules match.</summary>
    bool LooksSensitive(string chunkText) =>
        NamePair.Matches(chunkText).Count >= 2 || RuleDetector.Find(chunkText, options).Count > 0;

    /// <summary>What to send as the model's think setting. Most thinking models take true/false, but gpt-oss ignores false and only takes a level, so "off" becomes its lowest level, "low".</summary>
    static object? ThinkSetting(string model, bool? think) =>
        think == false && model.StartsWith("gpt-oss", StringComparison.OrdinalIgnoreCase) ? "low" : think;

    /// <summary>Set when the model refused the think setting (it has no thinking mode), so later calls leave it out.</summary>
    bool thinkRejected;

    /// <summary>What one model call produced: the parsed items plus everything the inspector needs.</summary>
    record AskResult(List<Item> Items, string UserMessage, string RawReply, long PromptTokens, long OutputTokens, TimeSpan Elapsed, int Attempts);

    /// <summary>Sends one piece of text to the model and returns what it found; retries once if the reply is not valid JSON.</summary>
    async Task<AskResult> AskAsync(string chunkText, CancellationToken ct, string? model = null, bool nudge = false)
    {
        var types = PromptBuilder.EnabledTypes(options).ToList();
        var userMessage = PromptBuilder.UserMessage(chunkText);
        if (nudge)
        {
            userMessage += "\n\nYour previous answer for this text was empty. Read it again carefully: list every person, company, address, contact detail, identifier and other item in the categories above. Return an empty list only if there is truly nothing.";
        }
        model ??= options.Llm.Model;
        var sendThink = options.Llm.Think is not null && !thinkRejected;
        var body = new {
            model,
            stream = false,
            keep_alive = options.Llm.KeepAlive,
            think = sendThink ? ThinkSetting(model, options.Llm.Think) : null,
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
            if (sendThink && resp.StatusCode == System.Net.HttpStatusCode.BadRequest
                && (await resp.Content.ReadAsStringAsync(ct)).Contains("think", StringComparison.OrdinalIgnoreCase))
            {
                // This model has no thinking mode, so it refuses the setting: remember that and ask again without it.
                thinkRejected = true;
                return await AskAsync(chunkText, ct, model, nudge);
            }
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
            .Select(e => new Item(e.GetProperty("type").GetString() ?? EntityTypes.Other, e.GetProperty("text").GetString() ?? string.Empty,
                e.TryGetProperty("context", out var cx) && cx.ValueKind == JsonValueKind.String ? cx.GetString() : null))
            .ToList();
    }
}
