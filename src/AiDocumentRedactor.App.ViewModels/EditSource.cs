namespace AiDocumentRedactor.App.ViewModels;

/// <summary>Plain-language names for where an edit came from, shown as a chip on each edit in the review list.
/// The source is carried on every span: "human", "rule", "llm", "llm-variant", "custom-list", "gliner-only", and any of the
/// machine sources with "+gliner" added when the second-opinion model (GLiNER) agreed.</summary>
public static class EditSource
{
    /// <summary>True when GLiNER, the second-opinion model, found this on its own (nothing else did).</summary>
    public static bool IsGlinerOnly(string source) => source == "gliner-only";

    /// <summary>True when GLiNER also found it, so two detectors agree.</summary>
    public static bool IsGlinerAgreed(string source) => source.EndsWith("+gliner", StringComparison.Ordinal);

    /// <summary>The short chip text.</summary>
    public static string Label(string source)
    {
        if (IsGlinerOnly(source))
        {
            return "GLiNER only";
        }

        var main = source.Replace("+gliner", string.Empty, StringComparison.Ordinal);
        var name = main switch
        {
            "human" => "Manual",
            "rule" => "Rule",
            "custom-list" => "List",
            _ => "AI",
        };
        return IsGlinerAgreed(source) ? name + " + GLiNER" : name;
    }

    /// <summary>The longer explanation shown when the pointer rests on the chip.</summary>
    public static string Tooltip(string source)
    {
        if (IsGlinerOnly(source))
        {
            return "Found only by the second-opinion model (GLiNER). The main model and the rules did not find it, so it is flagged for you to decide.";
        }

        var main = source.Replace("+gliner", string.Empty, StringComparison.Ordinal);
        var what = main switch
        {
            "human" => "Added by a person",
            "rule" => "Found by a fixed rule (a format such as an email address or a number with a check digit)",
            "custom-list" => "Matched a term on the always-redact list",
            "llm-variant" => "A shorter form of something the model found (a surname on its own, or a company without its ending)",
            _ => "Found by the language model",
        };
        return IsGlinerAgreed(source) ? what + ", and confirmed by the second-opinion model (GLiNER)" : what;
    }

    /// <summary>The reason shown on a flagged edit that only GLiNER found.</summary>
    public static string GlinerOnlyReason(double confidence) =>
        $"Found only by the second-opinion model (GLiNER, confidence {confidence:0.00}). The main model and the rules did not find it. " +
        "Left in the text. Choose “Redact this” to remove it, or dismiss it.";
}
