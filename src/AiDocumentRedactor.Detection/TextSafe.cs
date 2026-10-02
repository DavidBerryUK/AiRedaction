namespace AiDocumentRedactor.Detection;

/// <summary>Guards for text that is not valid UTF-16, such as an unpaired surrogate half left by a bad conversion or a split emoji.</summary>
public static class TextSafe
{
    /// <summary>The text with every unpaired surrogate replaced by U+FFFD (one character for one, so positions never move). Valid text, including emoji, is returned as it is.</summary>
    public static string Clean(string text)
    {
        char[]? fixedChars = null;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            var bad = char.IsHighSurrogate(c) ? i + 1 >= text.Length || !char.IsLowSurrogate(text[i + 1]) : char.IsLowSurrogate(c) && (i == 0 || !char.IsHighSurrogate(text[i - 1]));
            if (bad)
            {
                fixedChars ??= text.ToCharArray();
                fixedChars[i] = '�';
            }
        }

        return fixedChars is null ? text : new string(fixedChars);
    }
}
