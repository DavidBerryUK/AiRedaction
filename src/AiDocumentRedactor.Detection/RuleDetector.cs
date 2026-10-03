using System.Text.RegularExpressions;
using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.Detection;

/// <summary>Finds predictable items with fixed rules, before and alongside the model: emails, UK phone numbers, postcodes, NHS and
/// National Insurance numbers, IBANs, sort codes, account and card numbers, IP addresses, organisation names ending in a suffix such as Ltd or Surgery, and gender words. Rules are instant,
/// repeatable and never forget, so these categories no longer depend on the model noticing them. Each rule only runs when its
/// category is switched on, and numbers with a check digit (NHS, IBAN, card) must pass it.</summary>
public static class RuleDetector
{
    const string Source = "rule";
    const RegexOptions Opt = RegexOptions.CultureInvariant;

    static readonly Regex Email = new(@"(?<![\w.%+-])[A-Za-z0-9._%+-]+\s?@\s?[A-Za-z0-9-]+(?:\.[A-Za-z0-9-]+)*\.[A-Za-z]{2,}(?![\w-])", Opt);
    static readonly Regex Phone = new(@"(?<![\w+])(?:\+44\s?\(?0?\)?\s?\d{2,4}|\(?0\d{2,4}\)?)[\s-]?\d{3,4}[\s-]?\d{3,4}(?![\w])", Opt);
    static readonly Regex Postcode = new(@"(?<![A-Za-z0-9])[A-Z]{1,2}\d[A-Z\d]?\s?\d[A-Z]{2}(?![A-Za-z0-9])", Opt);
    static readonly Regex Nhs = new(@"(?<!\d)\d{3}[\s-]?\d{3}[\s-]?\d{4}(?!\d)", Opt);
    static readonly Regex Nino = new(@"(?<![A-Za-z0-9])[A-Z]{2}\s?\d{2}\s?\d{2}\s?\d{2}\s?[A-D](?![A-Za-z0-9])", Opt);
    static readonly Regex Iban = new(@"(?<![A-Za-z0-9])[A-Z]{2}\d{2}(?:\s?[A-Z0-9]{4}){2,7}(?:\s?[A-Z0-9]{1,3})?(?![A-Za-z0-9])", Opt);
    static readonly Regex SortCode = new(@"(?i:sort\s*code)[:\s]*(\d{2}[-\s]\d{2}[-\s]\d{2})(?!\d)", Opt);
    static readonly Regex Account = new(@"(?i:account\s*(?:number|no\.?|ending(?:\s+in)?))[:\s]*(\d{4,10})(?!\d)", Opt);
    static readonly Regex Card = new(@"(?<!\d)(?:\d[ -]?){12,18}\d(?!\d)", Opt);
    static readonly Regex Ipv4 = new(@"(?<![\d.])(?:\d{1,3}\.){3}\d{1,3}(?!\d|\.\d)", Opt);
    static readonly Regex GenderWords = new(@"(?<![\p{L}])(?:man|woman|men|women|male|female|Mr|Mrs|Ms|Miss)(?![\p{L}])", Opt);
    static readonly Regex Pronouns = new(@"(?<![\p{L}])(?:[Hh]e|[Ss]he|[Hh]im|[Hh]er|[Hh]is|[Hh]ers|[Hh]imself|[Hh]erself)(?![\p{L}])", Opt);

    static readonly Regex Ipv6Candidate = new(@"(?<![\w:.])(?:[0-9A-Fa-f]{0,4}:){2,7}[0-9A-Fa-f]{0,4}(?![\w:])", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>IPv6 addresses in the text: candidates made of hex groups and colons that really parse as IPv6 (so a time such as 10:30:45 or a MAC address is not one).</summary>
    public static List<DetectedEntity> FindIpv6(string text)
    {
        var found = new List<DetectedEntity>();
        foreach (Match m in Ipv6Candidate.Matches(text))
        {
            if (m.Value.Count(c => c == ':') >= 2 && System.Net.IPAddress.TryParse(m.Value, out var ip) && ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                found.Add(new DetectedEntity(EntityTypes.OnlineId, m.Index, m.Length, 1.0, Source));
            }
        }

        return found;
    }

    /// <summary>Everything the rules find in the text, for the categories that are switched on.</summary>
    public static List<DetectedEntity> Find(string text, RedactorOptions options)
    {
        var on = PromptBuilder.EnabledTypes(options).ToHashSet();
        var found = new List<DetectedEntity>();
        void Add(string type, int start, int length)
        {
            if (on.Contains(type))
            {
                found.Add(new DetectedEntity(type, start, length, 1.0, Source));
            }
        }

        foreach (Match m in Email.Matches(text))
        {
            Add(EntityTypes.Email, m.Index, m.Length);
        }

        if (options.Rules.CleanUp.Ipv6)
        {
            foreach (var ip in FindIpv6(text))
            {
                Add(EntityTypes.OnlineId, ip.Start, ip.Length);
            }
        }

        foreach (Match m in Phone.Matches(text))
        {
            var digits = Digits(m.Value);
            if (digits.StartsWith("44") ? digits.Length is 12 or 13 : digits.Length is 10 or 11)
            {
                Add(EntityTypes.Phone, m.Index, m.Length);
            }
        }

        foreach (Match m in Postcode.Matches(text))
        {
            Add(EntityTypes.Address, m.Index, m.Length);
        }

        foreach (Match m in Nhs.Matches(text))
        {
            if (NhsValid(Digits(m.Value)))
            {
                Add(EntityTypes.IdNumber, m.Index, m.Length);
            }
        }

        foreach (Match m in Nino.Matches(text))
        {
            Add(EntityTypes.IdNumber, m.Index, m.Length);
        }

        foreach (Match m in Iban.Matches(text))
        {
            if (IbanValid(m.Value))
            {
                Add(EntityTypes.IdNumber, m.Index, m.Length);
            }
        }

        foreach (var rx in new[] { SortCode, Account })
        {
            foreach (Match m in rx.Matches(text))
            {
                Add(EntityTypes.IdNumber, m.Groups[1].Index, m.Groups[1].Length);
            }
        }

        foreach (Match m in Card.Matches(text))
        {
            var digits = Digits(m.Value);
            if (digits.Length is >= 13 and <= 19 && LuhnValid(digits))
            {
                Add(EntityTypes.IdNumber, m.Index, m.Length);
            }
        }

        foreach (Match m in Ipv4.Matches(text))
        {
            if (m.Value.Split('.').All(p => int.Parse(p) <= 255))
            {
                Add(EntityTypes.OnlineId, m.Index, m.Length);
            }
        }

        foreach (var (start, length) in Organisations(text, options.Rules.OrganisationSuffixes))
        {
            Add(EntityTypes.Company, start, length);
        }

        foreach (Match m in GenderWords.Matches(text))
        {
            Add(EntityTypes.Gender, m.Index, m.Length);
        }

        if (options.Entities.TryGetValue(EntityTypes.Gender, out var g) && g.RedactPronouns)
        {
            foreach (Match m in Pronouns.Matches(text))
            {
                Add(EntityTypes.Gender, m.Index, m.Length);
            }
        }

        return found;
    }

    /// <summary>Words that start a sentence or a phrase rather than a name, dropped from the front of a match ("The Council", "Dear Fernleigh Surgery").</summary>
    static readonly HashSet<string> Leading = new(StringComparer.Ordinal)
    {
        "The", "A", "An", "Our", "Your", "Their", "His", "Her", "Its", "This", "That", "These", "Those", "Dear", "At", "From", "To", "For", "With", "By", "Of", "In", "On", "And", "Or", "Contact", "Please",
    };

    /// <summary>One to five capitalised words (letters, digits, &amp;, ' and -; a single line break between them is allowed, as PDFs wrap names) followed by an organisation suffix, such as "Fernleigh Surgery" or
    /// "Corvid Logistics PLC". Leading words like "The" or "Dear" are dropped, and a match left with no name before the suffix is ignored, so
    /// "the Council" or "a Surgery" on its own is not redacted.</summary>
    public static IEnumerable<(int Start, int Length)> Organisations(string text, IEnumerable<string> suffixes)
    {
        var list = suffixes.Where(x => !string.IsNullOrWhiteSpace(x)).OrderByDescending(x => x.Length).Select(x => string.Join(@"\s+", x.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(Regex.Escape))).ToList();
        if (list.Count == 0)
        {
            yield break;
        }

        var word = @"(?:\p{Lu}[\p{L}\p{N}'&-]*|&)";
        var rx = new Regex($@"(?<![\p{{L}}\p{{N}}])((?:{word}(?:[ \t]+|[ \t]*\n[ \t]*)){{1,5}})({string.Join("|", list)})\.?(?![\p{{L}}\p{{N}}])", Opt);
        foreach (Match m in rx.Matches(text))
        {
            var start = m.Index;
            var words = m.Groups[1].Value.Split([' ', '\t', '\n'], StringSplitOptions.RemoveEmptyEntries).ToList();
            while (words.Count > 0 && Leading.Contains(words[0]))
            {
                start = text.IndexOf(words[1 < words.Count ? 1 : 0], start + words[0].Length, StringComparison.Ordinal);
                words.RemoveAt(0);
            }

            if (words.Count == 0 || words.All(w => w == "&"))
            {
                continue;
            }

            var end = m.Groups[2].Index + m.Groups[2].Length;
            yield return (start, end - start);
        }
    }

    static string Digits(string s) => new(s.Where(char.IsDigit).ToArray());

    /// <summary>NHS numbers end in a modulus-11 check digit.</summary>
    static bool NhsValid(string d)
    {
        if (d.Length != 10)
        {
            return false;
        }

        var sum = 0;
        for (var i = 0; i < 9; i++)
        {
            sum += (d[i] - '0') * (10 - i);
        }

        var check = 11 - sum % 11;
        check = check == 11 ? 0 : check;
        return check != 10 && check == d[9] - '0';
    }

    /// <summary>IBANs pass a modulus-97 check.</summary>
    static bool IbanValid(string raw)
    {
        var s = new string(raw.Where(char.IsLetterOrDigit).ToArray());
        if (s.Length < 15 || s.Length > 34)
        {
            return false;
        }

        var moved = s[4..] + s[..4];
        var rem = 0;
        foreach (var c in moved)
        {
            var v = char.IsDigit(c) ? (c - '0').ToString() : (c - 'A' + 10).ToString();
            foreach (var ch in v)
            {
                rem = (rem * 10 + (ch - '0')) % 97;
            }
        }

        return rem == 1;
    }

    /// <summary>Card numbers pass the Luhn check.</summary>
    static bool LuhnValid(string d)
    {
        var sum = 0;
        var dbl = false;
        for (var i = d.Length - 1; i >= 0; i--)
        {
            var n = d[i] - '0';
            if (dbl)
            {
                n *= 2;
                if (n > 9)
                {
                    n -= 9;
                }
            }

            sum += n;
            dbl = !dbl;
        }

        return sum % 10 == 0;
    }
}
