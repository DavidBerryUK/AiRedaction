using System.Text.RegularExpressions;
using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.Detection;

/// <summary>Fixed checks that tidy a language model's predictable mistakes after it has answered: a template placeholder is not a company, a masked value is not a secret, "Borrower" is
/// not a name, and an ordinary date is not a date of birth. Each rule is a switch in <see cref="CleanUpOptions"/>. They only ever take redactions away, and only from what a model
/// (or GLiNER) found: never from the fixed rules, a custom list or a person. What a rule leaves out is returned, so it can be shown and checked.</summary>
public static partial class CleanUpRules
{
    public const string BracketedRule = CleanUpRuleNames.Bracketed, MaskedRule = CleanUpRuleNames.Masked, GenericRule = CleanUpRuleNames.Generic, BirthRule = CleanUpRuleNames.Birth;

    /// <summary>A plain-English name for a rule, for the review screen.</summary>
    public static string Label(string rule) => CleanUpRuleNames.Label(rule);

    [GeneratedRegex(@"^(?:\[[^\[\]\r\n]{1,60}\]|\{\{[^{}\r\n]{1,60}\}\}|\{[^{}\r\n]{1,60}\}|<[^<>@\r\n]{1,60}>)$", RegexOptions.CultureInvariant)]
    private static partial Regex Brackets();

    [GeneratedRegex(@"^(?:MM|DD|YY|YYYY)(?:[/\-.](?:MM|DD|YY|YYYY)){1,2}$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex DateTemplate();

    [GeneratedRegex(@"birth|\bd\.?o\.?b\b|\bborn\b|\bb\.\s", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex Birth();

    /// <summary>Applies the switched-on rules. Returns what is kept and what was left out, with the rule that left it out.</summary>
    public static (List<DetectedEntity> Kept, List<SuppressedItem> Suppressed) Apply(string text, IEnumerable<DetectedEntity> spans, CleanUpOptions o)
    {
        var list = spans.ToList();
        if (!o.AnyOn)
        {
            return (list, []);
        }

        var generic = o.GenericTerms ? new HashSet<string>(o.GenericTermList.Select(Normalise), StringComparer.OrdinalIgnoreCase) : [];
        var kept = new List<DetectedEntity>();
        var suppressed = new List<SuppressedItem>();
        foreach (var s in list)
        {
            var rule = FromAModel(s) && s.Start >= 0 && s.Start + s.Length <= text.Length ? RuleFor(text, s, o, generic) : null;
            if (rule is null)
            {
                kept.Add(s);
            }
            else
            {
                suppressed.Add(new SuppressedItem(rule, s.Type, s.Start, s.Length));
            }
        }

        return (kept, suppressed);
    }

    /// <summary>True for a span a model or GLiNER produced. The fixed rules, the custom list and a person's own edits are never touched.</summary>
    static bool FromAModel(DetectedEntity s) => s.Source.StartsWith("llm", StringComparison.Ordinal) || s.Source.StartsWith("gliner", StringComparison.Ordinal);

    /// <summary>The name of the rule that says this span should not be redacted, or null if none does.</summary>
    static string? RuleFor(string text, DetectedEntity s, CleanUpOptions o, HashSet<string> generic)
    {
        var t = text.Substring(s.Start, s.Length).Trim();
        if (o.BracketedPlaceholders && IsPlaceholder(text, s, t))
        {
            return BracketedRule;
        }

        if (o.MaskedValues && IsMasked(t))
        {
            return MaskedRule;
        }

        if (o.GenericTerms && s.Type is EntityTypes.Company or EntityTypes.Person && generic.Contains(Normalise(t)))
        {
            return GenericRule;
        }

        if (o.BirthDateContext && s.Type == EntityTypes.DateOfBirth && !HasBirthWording(text, s))
        {
            return BirthRule;
        }

        return null;
    }

    static string Normalise(string t)
    {
        var x = t.Trim().Trim('.', ',', ';', ':', '"', '\'', '(', ')').Trim();
        return x.StartsWith("the ", StringComparison.OrdinalIgnoreCase) ? x[4..].Trim() : x;
    }

    /// <summary>Words a template placeholder is made of ("Company Name", "Insert Link", "Your Email Address"). A bracketed phrase counts as a placeholder only if every word is one of
    /// these (or a small joining word), so a real name or email that merely sits in brackets is never taken for one.</summary>
    static readonly HashSet<string> PlaceholderWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "name", "names", "company", "companies", "insert", "enter", "your", "you", "date", "address", "email", "e-mail", "phone", "telephone", "number", "link", "url", "amount", "account",
        "customer", "client", "party", "title", "signature", "city", "state", "country", "zip", "postal", "code", "here", "tbd", "first", "last", "full", "middle", "contact", "details",
        "info", "information", "id", "borrower", "lender", "buyer", "seller", "recipient", "sender", "employer", "employee", "month", "day", "year", "reference", "type", "text", "value",
        "of", "the", "a", "an", "and", "or", "for", "to", "xxx", "placeholder",
    };

    /// <summary>The words inside brackets, if they are all placeholder words (and there are some). Digits and an @ sign rule it out: a real email or number is not a placeholder.</summary>
    static bool IsPlaceholderPhrase(string inner)
    {
        if (inner.Any(char.IsDigit) || inner.Contains('@', StringComparison.Ordinal))
        {
            return false;
        }

        var words = inner.Split([' ', '_', '-', '\t', '{', '}', '[', ']', '<', '>'], StringSplitOptions.RemoveEmptyEntries);
        return words.Length > 0 && words.All(PlaceholderWords.Contains);
    }

    /// <summary>"[Company Name]", "{{ customer_name }}", "&lt;Insert Link&gt;" or the words inside such brackets, when those words are placeholder words; and a date template such as "MM/DD/YYYY".</summary>
    static bool IsPlaceholder(string text, DetectedEntity s, string t)
    {
        if (DateTemplate().IsMatch(t))
        {
            return true;
        }

        if (Brackets().IsMatch(t))
        {
            return IsPlaceholderPhrase(t);
        }

        var before = s.Start > 0 ? text[s.Start - 1] : ' ';
        var after = s.Start + s.Length < text.Length ? text[s.Start + s.Length] : ' ';
        return (before == '[' && after == ']' || before == '{' && after == '}') && IsPlaceholderPhrase(t);
    }

    /// <summary>Only mask characters (X, *, #, bullets), a run of zeros, or nothing but separators. A repeated digit such as the sort code 11-11-11 is a real number, not a mask.</summary>
    static bool IsMasked(string t)
    {
        var core = new string(t.Where(c => !(char.IsWhiteSpace(c) || c is '-' or '.' or '/' or '_' or ',')).ToArray());
        if (core.Length == 0)
        {
            return t.Length >= 4;
        }

        if (core.Length >= 4 && core.All(c => c is 'X' or 'x' or '*' or '#' or '•' or '●'))
        {
            return true;
        }

        return core.Length >= 6 && core.All(c => c == '0');
    }

    /// <summary>Birth wording ("born", "birth", "DOB") shortly before or after the date, or in the first few hundred characters (a heading or a column name).</summary>
    static bool HasBirthWording(string text, DetectedEntity s)
    {
        var from = Math.Max(0, s.Start - 100);
        var to = Math.Min(text.Length, s.Start + s.Length + 30);
        return Birth().IsMatch(text[from..to]) || Birth().IsMatch(text[..Math.Min(400, text.Length)]);
    }
}
