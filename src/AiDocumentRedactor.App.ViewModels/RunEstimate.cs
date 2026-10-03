namespace AiDocumentRedactor.App.ViewModels;

/// <summary>A forecast shown before Redact is pressed: how big the document is and roughly how long the run will take.</summary>
public record RunEstimate(int Pages, int Chars, int Chunks, int Models, double Seconds, bool Measured, string? Warning)
{
    /// <summary>The time in plain words, e.g. "14 s" or "3 min".</summary>
    public string TimeText => Seconds < 90 ? $"{Math.Max(1, Math.Round(Seconds)):0} s" : $"{Math.Round(Seconds / 60):0} min";
    /// <summary>One line for the screen, e.g. "2 chunks · about 14 s".</summary>
    public string Summary => $"{Chunks} chunk{(Chunks == 1 ? "" : "s")}{(Models > 1 ? $" × {Models} models" : "")} · about {TimeText}{(Measured ? "" : " (a guess; improves after a run)")}";
}

/// <summary>One category in the Categories dialog: its switch, mode, whether pronouns are redacted (GENDER only) and its definition.</summary>
public record CategoryRow(string Type, bool Enabled, string Mode, bool RedactPronouns, string Description);

/// <summary>A clean-up rule as the Categories dialog shows it: its key, a short name, what it does, and whether it is on.</summary>
public record CleanUpRuleRow(string Key, string Name, string Description, bool On);

/// <summary>Something a clean-up rule left out, with the text from the document, the rule that left it out in plain words, and its line, so a reviewer can redact it after all.</summary>
public record SuppressedRow(string Rule, string RuleLabel, string Type, int Start, int Length, string Text, int Line);
