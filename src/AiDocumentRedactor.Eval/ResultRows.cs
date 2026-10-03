using System.Security.Cryptography;
using System.Text;
using AiDocumentRedactor.Core;
using AiDocumentRedactor.Explorer.Dataset;

namespace AiDocumentRedactor.Eval;

/// <summary>A scored result as dataset rows: the result, its spans, its judged facts, and the score they came from.</summary>
public record BuiltResult(ResultRow Result, List<SpanRow> Spans, List<OutcomeRow> Outcomes, DocScore Score);

/// <summary>Turns one detector setup's spans on one document into the dataset's rows, scored against the answer key. The interim converter, the live run and the final
/// evaluator all use this, so every result in a dataset is scored the same way.</summary>
public static class ResultRows
{
    /// <summary>Scores <paramref name="spans"/> against <paramref name="gt"/> and builds the rows. Timings, tokens and the output check are left for the caller to fill in.</summary>
    public static BuiltResult Build(string docId, string config, string model, string variant, string text, IReadOnlyList<DetectedEntity> spans, GroundTruth gt, string formatGroup,
        string template, string settingsHash, string source = "batch", int repeat = 1)
    {
        var score = Scoring.Score(text, Redactor.Apply(text, spans, template), gt, formatGroup);
        var keySpans = Scoring.KeySpans(text, gt);
        var result = new ResultRow
        {
            ResultId = $"{docId}|{config}|{repeat}", DocId = docId, Config = config, Model = model, Variant = variant, SettingsHash = settingsHash, Source = source, Repeat = repeat,
            Present = score.Present, Caught = score.Caught, EntitiesPresent = score.EntitiesPresent, EntitiesFullyCaught = score.EntitiesFullyCaught,
            LostToExtraction = score.LostToExtraction, Edits = score.Edits, TruePositives = score.TruePositives, TypeCorrect = score.TypeCorrect, Unjudged = score.Unjudged,
            PreserveTotal = score.MustPreserve, PreserveBroken = score.PreserveBroken.Count,
        };
        if (variant != "plain")
        {
            var solo = spans.Where(s => s.Source == "gliner-only").ToList();
            result.FlagsRaised = solo.Count;
            result.FlagsCorrect = solo.Count(s => Scoring.OverlapsKey(keySpans, s.Start, s.Length));
        }

        var spanRows = new List<SpanRow>();
        for (var i = 0; i < spans.Count; i++)
        {
            var s = spans[i];
            spanRows.Add(new SpanRow($"{result.ResultId}#{i + 1}", result.ResultId, s.Type, s.Start, s.Length, s.Confidence, s.Source, s.Flag));
        }

        var outcomes = new List<OutcomeRow>();
        var n = 0;
        string SpanFor(Fact f)
        {
            var i = spans.ToList().FindIndex(s => s.Start == f.Start && s.Length == f.Length && s.Type == f.Type);
            return i >= 0 ? spanRows[i].SpanId : string.Empty;
        }

        foreach (var f in score.Facts)
        {
            outcomes.Add(new OutcomeRow($"{result.ResultId}#o{++n}", result.ResultId, docId, f.Kind, f.EntityIndex >= 0 ? $"{gt.Id}#{f.EntityIndex + 1}" : string.Empty,
                f.Kind is "over_redaction" or "unjudged" ? SpanFor(f) : string.Empty, f.Type, f.Text, f.Start >= 0 ? f.Start : null, f.Start >= 0 ? f.Length : null));
        }

        for (var i = 0; i < spans.Count; i++)
        {
            if (spans[i].Flag && spans[i].Source == "gliner-only")
            {
                var ok = Scoring.OverlapsKey(keySpans, spans[i].Start, spans[i].Length);
                outcomes.Add(new OutcomeRow($"{result.ResultId}#o{++n}", result.ResultId, docId, ok ? "flag_correct" : "flag_wrong", string.Empty, spanRows[i].SpanId, spans[i].Type,
                    text.Substring(spans[i].Start, spans[i].Length), spans[i].Start, spans[i].Length));
            }
        }

        return new BuiltResult(result, spanRows, outcomes, score);
    }

    /// <summary>The settings that change results, as recorded with each source run.</summary>
    public static Dictionary<string, object?> Settings(RedactorOptions o)
    {
        var settings = BaseSettings(o);
        if (o.Rules.CleanUp.AnyOn)
        {
            // Recorded only when a rule is on, so datasets made before the clean-up rules existed keep their settings fingerprint.
            var c = o.Rules.CleanUp;
            settings["cleanUp"] = new Dictionary<string, object?>
            {
                ["bracketedPlaceholders"] = c.BracketedPlaceholders, ["maskedValues"] = c.MaskedValues, ["genericTerms"] = c.GenericTerms, ["genericTermList"] = c.GenericTerms ? c.GenericTermList : null,
                ["birthDateContext"] = c.BirthDateContext, ["ipv6"] = c.Ipv6,
            };
        }

        return settings;
    }

    static Dictionary<string, object?> BaseSettings(RedactorOptions o) => new()
    {
        ["llm"] = new Dictionary<string, object?>
        {
            ["temperature"] = o.Llm.Temperature, ["seed"] = o.Llm.Seed, ["numCtx"] = o.Llm.NumCtx, ["chunkChars"] = o.Llm.ChunkChars, ["chunkOverlapChars"] = o.Llm.ChunkOverlapChars,
            ["think"] = o.Llm.Think, ["keepAlive"] = o.Llm.KeepAlive, ["timeoutSeconds"] = o.Llm.TimeoutSeconds, ["fallbackModel"] = o.Llm.FallbackModel,
        },
        ["rules"] = new Dictionary<string, object?> { ["enabled"] = o.Rules.Enabled, ["organisationSuffixes"] = o.Rules.OrganisationSuffixes },
        ["categories"] = o.Entities.OrderBy(e => e.Key, StringComparer.Ordinal).ToDictionary(e => e.Key, e => (object?)new Dictionary<string, object?> { ["enabled"] = e.Value.Enabled, ["mode"] = e.Value.Mode, ["redactPronouns"] = e.Value.RedactPronouns }),
        ["placeholderTemplate"] = o.Redaction.PlaceholderTemplate,
        ["gliner"] = new Dictionary<string, object?> { ["enabled"] = o.Gliner.Enabled, ["threshold"] = o.Gliner.Threshold, ["soloAction"] = o.Gliner.SoloAction },
    };

    /// <summary>A short fingerprint of a settings object, recorded on every result so results made with different settings can be told apart.</summary>
    public static string SettingsHash(Dictionary<string, object?> settings) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(settings)))).ToLowerInvariant()[..12];

    /// <summary>A checksum of an answer key folder's files (names and contents), recorded in run.json so a later reader can tell whether the key has changed.</summary>
    public static string KeyChecksum(string corpusDir)
    {
        using var sha = SHA256.Create();
        foreach (var f in Directory.GetFiles(Path.Combine(corpusDir, "ground-truth"), "*.json").Order(StringComparer.Ordinal))
        {
            var name = Encoding.UTF8.GetBytes(Path.GetFileName(f));
            sha.TransformBlock(name, 0, name.Length, null, 0);
            var bytes = File.ReadAllBytes(f);
            sha.TransformBlock(bytes, 0, bytes.Length, null, 0);
        }

        sha.TransformFinalBlock([], 0, 0);
        return Convert.ToHexString(sha.Hash!).ToLowerInvariant();
    }
}
