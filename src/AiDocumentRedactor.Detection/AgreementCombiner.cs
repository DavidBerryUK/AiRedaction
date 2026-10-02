using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.Detection;

/// <summary>Combines the main detector (the language model plus the fixed rules) with GLiNER by agreement.
/// Where both found something, the main detector's span stands and is marked as agreed, which raises its confidence. Something only GLiNER found is
/// a second opinion: by default it is <b>flagged for review and left in the text</b>, never redacted on GLiNER's word alone. Everything the main
/// detector found is kept as it is, so adding GLiNER can only add review items (or, if configured, redactions); it never removes a redaction.</summary>
public static class AgreementCombiner
{
    /// <summary>The combined spans, from the main detector's spans and GLiNER's.</summary>
    public static List<DetectedEntity> Combine(IReadOnlyList<DetectedEntity> primary, IReadOnlyList<DetectedEntity> gliner, RedactorOptions options)
    {
        var settings = options.Gliner;
        var allow = new HashSet<string>(options.CustomTerms.Allow, StringComparer.OrdinalIgnoreCase);
        var result = new List<DetectedEntity>(primary.Count + gliner.Count);
        var agreed = new bool[primary.Count];
        var best = new double[primary.Count];
        var solo = new List<DetectedEntity>();

        foreach (var g in gliner)
        {
            var found = false;
            for (var i = 0; i < primary.Count; i++)
            {
                if (Overlaps(primary[i], g))
                {
                    agreed[i] = true;
                    best[i] = Math.Max(best[i], g.Confidence);
                    found = true;
                }
            }

            if (!found)
            {
                solo.Add(g);
            }
        }

        for (var i = 0; i < primary.Count; i++)
        {
            var p = primary[i];
            result.Add(agreed[i] ? p with { Source = p.Source + "+gliner", Confidence = Math.Max(p.Confidence, best[i]) } : p);
        }

        if (settings.SoloAction.Equals("ignore", StringComparison.OrdinalIgnoreCase))
        {
            return result;
        }

        foreach (var g in solo.Where(g => g.Length >= 3))
        {
            var redact = settings.SoloAction.Equals("redact", StringComparison.OrdinalIgnoreCase) || g.Confidence >= settings.SoloRedactMinScore;
            result.Add(g with { Source = "gliner-only", Flag = !redact });
        }

        return result;
    }

    static bool Overlaps(DetectedEntity a, DetectedEntity b) => a.Start < b.Start + b.Length && b.Start < a.Start + a.Length;
}
