namespace AiDocumentRedactor.Core;

/// <summary>Deterministic redaction: applies spans to text. Separate from detection (spec §7.2).</summary>
public static class Redactor
{
    /// <summary>Builds the replacement text, e.g. [REDACTED:EMAIL], from the template.</summary>
    public static string Placeholder(string template, string type) => template.Replace("{type}", type);

    /// <summary>Overlaps resolve to the longest span; ties go to the earlier span. A flagged span that overlaps a real redaction is
    /// dropped, because the redaction already covers that text.</summary>
    public static List<DetectedEntity> Merge(IEnumerable<DetectedEntity> spans, int textLength)
    {
        var inRange = spans.Where(s => s.Length > 0 && s.Start >= 0 && s.Start + s.Length <= textLength).ToList();
        var redactions = inRange.Where(s => !s.Flag).ToList();
        var valid = redactions.Concat(inRange.Where(s => s.Flag && !redactions.Any(r => s.Start < r.Start + r.Length && r.Start < s.Start + s.Length)))
                              .OrderBy(s => s.Start).ThenByDescending(s => s.Length).ToList();
        var result = new List<DetectedEntity>();
        foreach (var s in valid)
        {
            if (result.Count > 0)
            {
                var last = result[^1];
                if (s.Start < last.Start + last.Length)
                {
                    if (s.Length > last.Length && s.Start == last.Start)
                    {
                        result[^1] = s;
                    }

                    continue;
                }
            }
            result.Add(s);
        }
        return result;
    }

    /// <summary>Replaces each span with its placeholder and records where every edit sits in the original and redacted text.
    /// Flagged spans stay in the text as they are, and are recorded as Flagged edits so a reviewer can see them.</summary>
    public static RedactionResult Apply(string text, IEnumerable<DetectedEntity> spans, string template)
    {
        var merged = Merge(spans, text.Length);
        var sb = new System.Text.StringBuilder();
        var edits = new List<RedactionEdit>();
        var pos = 0;
        var id = 1;
        foreach (var s in merged)
        {
            sb.Append(text, pos, s.Start - pos);
            if (s.Flag)
            {
                edits.Add(new RedactionEdit(id++, s.Type, s.Start, s.Length, sb.Length, s.Length, string.Empty, text.Substring(s.Start, s.Length), s.Confidence, s.Source, EditStatus.Flagged));
                sb.Append(text, s.Start, s.Length);
                pos = s.Start + s.Length;
                continue;
            }
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
