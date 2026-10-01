namespace AiDocumentRedactor.Core;

/// <summary>Deterministic redaction: applies spans to text. Separate from detection (spec §7.2).</summary>
public static class Redactor
{
    /// <summary>Builds the replacement text, e.g. [REDACTED:EMAIL], from the template.</summary>
    public static string Placeholder(string template, string type) => template.Replace("{type}", type);

    /// <summary>Overlaps resolve to the longest span; ties go to the earlier span.</summary>
    public static List<DetectedEntity> Merge(IEnumerable<DetectedEntity> spans, int textLength)
    {
        var valid = spans.Where(s => s.Length > 0 && s.Start >= 0 && s.Start + s.Length <= textLength)
                         .OrderBy(s => s.Start).ThenByDescending(s => s.Length).ToList();
        var result = new List<DetectedEntity>();
        foreach (var s in valid)
        {
            if (result.Count > 0)
            {
                var last = result[^1];
                if (s.Start < last.Start + last.Length)
                {
                    if (s.Length > last.Length && s.Start == last.Start) result[^1] = s;
                    continue;
                }
            }
            result.Add(s);
        }
        return result;
    }

    /// <summary>Replaces each span with its placeholder and records where every edit sits in the original and redacted text.</summary>
    public static RedactionResult Apply(string text, IEnumerable<DetectedEntity> spans, string template)
    {
        var merged = Merge(spans, text.Length);
        var sb = new System.Text.StringBuilder();
        var edits = new List<RedactionEdit>();
        var pos = 0; var id = 1;
        foreach (var s in merged)
        {
            sb.Append(text, pos, s.Start - pos);
            var placeholder = Placeholder(template, s.Type);
            edits.Add(new RedactionEdit(id++, s.Type, s.Start, s.Length, sb.Length, placeholder.Length,
                placeholder, text.Substring(s.Start, s.Length), s.Confidence, s.Source, EditStatus.Active));
            sb.Append(placeholder);
            pos = s.Start + s.Length;
        }
        sb.Append(text, pos, text.Length - pos);
        return new RedactionResult(sb.ToString(), edits);
    }
}
