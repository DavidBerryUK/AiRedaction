using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace AiDocumentRedactor.Eval;

/// <summary>What the audit decides about a disagreement between the answer key and the models.</summary>
public enum AuditDecision
{
    /// <summary>The models redacted something sensitive that the key left out: add it to the key.</summary>
    Add,
    /// <summary>The models redacted something that is not sensitive (a placeholder, a public body, a reference code): a real over-redaction, left as it is.</summary>
    Reject,
    /// <summary>Cannot be decided by a rule: neither counted right nor wrong.</summary>
    Ignore,
    /// <summary>A key item most models missed that is real: keep it.</summary>
    Keep,
    /// <summary>A key item that is not sensitive (a placeholder, a generic word, a row of dashes, a city on its own): take it out of the key.</summary>
    Remove,
}

/// <summary>Audits an answer key made by a data generator against what several models agree on. The generator labels only some of the sensitive text, and a
/// few of its labels are not sensitive at all, so scores against it understate the models. Where at least 3 of 5 models agree that something differs from the key,
/// simple, written rules decide whether the key or the models are right, and anything the rules cannot decide is left out of scoring. The rules are listed in
/// the audit log and were checked against a hand reading of 108 of these disagreements. The key is changed only by an explicit <c>--apply</c>.</summary>
public static partial class KeyAudit
{
    static readonly string[] Judged = ["PERSON", "COMPANY", "ADDRESS", "EMAIL", "PHONE", "ID_NUMBER", "DATE_OF_BIRTH", "ONLINE_ID", "SECRET"];
    static readonly HashSet<string> Unjudged = ["GENDER", "AGE", "CONTEXTUAL", "DOMAIN", "COMPANY_ID", "LOCATION"];

    [GeneratedRegex(@"^(HMRC|HM Revenue( and|&| &) Customs|FRC|FCA|PRA|SEC|IRS|ICO|NHS|GAAP|IFRS|SWIFT|ISDA|FpML|XBRL|Companies House|United States Internal Revenue Service|Internal Revenue Service|Financial Reporting Council|Financial Conduct Authority)$", RegexOptions.IgnoreCase)]
    private static partial Regex PublicBody();
    [GeneratedRegex(@"^(Company|Corporation|Firm|Bank|Employer|Employee|Borrower|Lender|Customer|Client|Seller|Buyer|Insurer|Insured|Party|Parties|Financial|Agreement|Account|Address|Name)$", RegexOptions.IgnoreCase)]
    private static partial Regex Generic();
    [GeneratedRegex(@"^[A-Z]{4}[A-Z]{2}[A-Z0-9]{2}([A-Z0-9]{3})?$")]
    private static partial Regex Bic();
    [GeneratedRegex(@"^[A-Z]{2}\d{10}$")]
    private static partial Regex Isin();
    [GeneratedRegex(@"^[A-Z]{2,4}\d{3,4}$")]
    private static partial Regex ReferenceCode();
    [GeneratedRegex(@"^[A-Z]{6,}\d{3,4}$")]
    private static partial Regex CodeName();
    [GeneratedRegex(@"\b(Ltd|Limited|PLC|LLC|LLP|Inc|Corp|Corporation|Group|Holdings|Partners|Associates|Enterprises|Solutions|Insurance|Trust|Bank|Union|Society|Association|Surgery|Clinic|Hospital|School|College|University|Council)\b")]
    private static partial Regex CompanySuffix();
    [GeneratedRegex(@"\b(St|Street|Road|Rd|Avenue|Ave|Lane|Ln|Close|Drive|Dr|Court|Ct|Box|Circle|Way|Unit|Apt|Suite|Square|Place|Terrace|Boulevard)\b", RegexOptions.IgnoreCase)]
    private static partial Regex StreetWord();
    [GeneratedRegex(@"^\p{Lu}[\p{L}'.\-]+( \p{Lu}[\p{L}'.\-]+){0,3}$")]
    private static partial Regex NameLike();
    [GeneratedRegex(@"^\p{Lu}[\p{L}.\- ]+, ?\p{Lu}[\p{L}. ]+$")]
    private static partial Regex CityCountry();
    [GeneratedRegex(@"^(\d{1,3}\.){3}\d{1,3}$|^([0-9a-fA-F]{1,4}:){2,7}[0-9a-fA-F]{1,4}$|^0x[0-9a-fA-F]{20,}$|^[13][1-9A-HJ-NP-Za-km-z]{25,34}$")]
    private static partial Regex MachineId();
    [GeneratedRegex(@"\b(1[89]\d\d|20[0-2]\d)\b")]
    private static partial Regex Year();

    /// <summary>Decides what to do with something the models redacted and the key lacks. <paramref name="type"/> is the category the models most often gave it.</summary>
    public static AuditDecision ClassifyGap(string type, string text)
    {
        var t = text.Trim();
        if (t.Length == 0 || t.Contains('[') || t.Contains(']') || t.Contains("___") || !t.Any(char.IsLetterOrDigit) || PublicBody().IsMatch(t) || Generic().IsMatch(t))
        {
            return AuditDecision.Reject;
        }

        if (Unjudged.Contains(type))
        {
            return AuditDecision.Ignore;
        }

        switch (type)
        {
            case "EMAIL" or "PHONE":
                return AuditDecision.Add;
            case "SECRET":
                return t.Length >= 8 ? AuditDecision.Add : AuditDecision.Ignore;
            case "ONLINE_ID":
                return t.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? AuditDecision.Reject : MachineId().IsMatch(t) || t.Length >= 6 ? AuditDecision.Add : AuditDecision.Ignore;
            case "ID_NUMBER":
                if (Bic().IsMatch(t) || Isin().IsMatch(t) || ReferenceCode().IsMatch(t) || t.Contains('*'))
                {
                    return AuditDecision.Reject;
                }

                return t.Count(char.IsDigit) >= 6 || MachineId().IsMatch(t) || Regex.IsMatch(t, @"^\d{2}-\d{2}-\d{2}$") ? AuditDecision.Add : AuditDecision.Reject;
            case "ADDRESS":
                return CityCountry().IsMatch(t) && !t.Any(char.IsDigit) && !StreetWord().IsMatch(t) ? AuditDecision.Ignore : AuditDecision.Add;
            case "PERSON":
                return NameLike().IsMatch(t) || (t.Length >= 4 && t.All(c => char.IsUpper(c) || c is ' ' or '-')) ? AuditDecision.Add : AuditDecision.Reject;
            case "COMPANY":
                return CodeName().IsMatch(t) || CompanySuffix().IsMatch(t) ? AuditDecision.Add : AuditDecision.Ignore;
            case "DATE_OF_BIRTH":
                var y = Year().Match(t);
                return y.Success && int.Parse(y.Value, CultureInfo.InvariantCulture) <= 2008 ? AuditDecision.Add : AuditDecision.Reject;
            default:
                return AuditDecision.Ignore;
        }
    }

    /// <summary>Decides whether an answer-key item that most models missed is real (keep) or noise (remove).</summary>
    public static AuditDecision ClassifyMiss(string type, string text)
    {
        var t = text.Trim();
        if (t.Length == 0 || t.Contains('[') || t.Contains(']') || t.Contains("___") || !t.Any(char.IsLetterOrDigit) || Generic().IsMatch(t) || PublicBody().IsMatch(t))
        {
            return AuditDecision.Remove;
        }

        if (!t.Any(char.IsUpper) && !t.Any(char.IsDigit))
        {
            return AuditDecision.Remove;   // an all-lower-case phrase cannot be a name, address or identifier
        }

        return type switch
        {
            "ADDRESS" when CityCountry().IsMatch(t) && !t.Any(char.IsDigit) => AuditDecision.Remove,   // a city on its own is a place, not an address (this project flags places)
            "ID_NUMBER" when t.Count(char.IsDigit) <= 4 => AuditDecision.Remove,                          // the last digits of a masked card
            "COMPANY" when !CodeName().IsMatch(t) && !CompanySuffix().IsMatch(t) && !t.Any(char.IsDigit) => AuditDecision.Remove,   // a topic, product or brand with no company marker
            _ => AuditDecision.Keep,
        };
    }

    sealed record Item(string File, string Text, string Type, int Models, AuditDecision Decision);

    /// <summary>Audits the answer key against the models of a saved run, writes a log, and with <paramref name="apply"/> rewrites the key files.</summary>
    public static int Run(string savedPath, string[] plainModels, bool apply, string logPath, string decisionsJson)
    {
        var saved = SavedRun.Load(savedPath);
        var corpus = saved.CorpusDir;
        var truth = GroundTruthStore.Load(corpus);
        var needed = (int)Math.Ceiling(plainModels.Length * 0.6);
        var rows = saved.Scores.Where(s => plainModels.Contains(s.Model)).ToList();

        var gaps = rows.SelectMany(s => s.FalsePositives.Select(f => (s.File, f.Text, f.Type, s.Model)))
            .GroupBy(x => (x.File, x.Text)).Where(g => g.Select(x => x.Model).Distinct().Count() >= needed)
            .Select(g => new Item(g.Key.File, g.Key.Text, g.GroupBy(x => x.Type).OrderByDescending(t => t.Count()).First().Key, g.Select(x => x.Model).Distinct().Count(), AuditDecision.Ignore)).ToList()
            .Select(i => i with { Decision = ClassifyGap(i.Type, i.Text) }).ToList();
        var misses = rows.SelectMany(s => s.Leaks.Select(l => (s.File, l.Text, l.Type, s.Model)))
            .GroupBy(x => (x.File, x.Text)).Where(g => g.Select(x => x.Model).Distinct().Count() >= needed)
            .Select(g => new Item(g.Key.File, g.Key.Text, g.First().Type, g.Select(x => x.Model).Distinct().Count(), AuditDecision.Keep)).ToList()
            .Select(i => i with { Decision = ClassifyMiss(i.Type, i.Text) }).ToList();

        var sb = new StringBuilder();
        sb.AppendLine("# Answer-key audit").AppendLine();
        sb.AppendLine($"Built from `{Path.GetFileName(savedPath)}`. Compared the answer key of `{corpus}` with what at least {needed} of {plainModels.Length} models ({string.Join(", ", plainModels)}) agree on.").AppendLine();
        sb.AppendLine("## Redacted by most models but not on the key").AppendLine();
        sb.AppendLine("| Decision | Items | Meaning |").AppendLine("|---|---:|---|");
        foreach (var (d, meaning) in new[] { (AuditDecision.Add, "sensitive and missing from the key: added"), (AuditDecision.Reject, "not sensitive (placeholder, public body, reference code, masked or generic text): a real over-redaction, left as it is"), (AuditDecision.Ignore, "cannot be decided by a rule, or a category the key does not judge: left out of scoring") })
        {
            sb.AppendLine($"| {d} | {gaps.Count(g => g.Decision == d)} | {meaning} |");
        }

        sb.AppendLine().AppendLine("## On the key but missed by most models").AppendLine();
        sb.AppendLine("| Decision | Items | Meaning |").AppendLine("|---|---:|---|");
        foreach (var (d, meaning) in new[] { (AuditDecision.Keep, "a real item: stays on the key, and the models really did miss it"), (AuditDecision.Remove, "not sensitive (placeholder, generic or lower-case phrase, city alone, masked digits): removed from the key") })
        {
            sb.AppendLine($"| {d} | {misses.Count(m => m.Decision == d)} | {meaning} |");
        }

        sb.AppendLine().AppendLine("## Rules used").AppendLine();
        sb.AppendLine("- Placeholders (`[Company Name]`, `____`), text with no letters or digits, generic words (Company, Firm, Borrower…), and public bodies (HMRC, FCA, IRS, GAAP…) are never sensitive.");
        sb.AppendLine("- Categories this key never labels (GENDER, AGE, CONTEXTUAL, DOMAIN, COMPANY_ID) are not judged at all.");
        sb.AppendLine("- Emails and phone numbers are added. Passwords and keys of 8 or more characters are added. IP addresses, wallet addresses and similar machine identifiers are added; web addresses are not.");
        sb.AppendLine("- Identifier numbers are added when they have 6 or more digits (or are a sort code); bank identifier codes (BIC), securities numbers (ISIN), short reference codes (ABC017) and masked numbers are not.");
        sb.AppendLine("- Addresses are added unless they are only a city and country (a place, which this project flags). Names are added when they look like names. Company names are added only with a company marker (Ltd, PLC, Bank, Trust…) or as a coded name; other company-like words are left out of scoring.");
        sb.AppendLine("- Dates are added as a date of birth only when the year is 2008 or earlier. A missed key item is removed when it is a placeholder, a generic word, an all-lower-case phrase, a city alone, the last digits of a masked number, or a company-like phrase with no company marker.");
        sb.AppendLine().AppendLine("## The decisions").AppendLine();
        sb.AppendLine("| Document | Text | Category | Models | Decision |").AppendLine("|---|---|---|---:|---|");
        foreach (var i in gaps.Concat(misses).OrderBy(i => i.Decision).ThenBy(i => i.File).ThenBy(i => i.Text))
        {
            sb.AppendLine($"| {i.File} | `{i.Text.Replace("|", "\\|").Replace("\n", " ")}` | {i.Type} | {i.Models} | {i.Decision} |");
        }

        File.WriteAllText(logPath, sb.ToString());
        File.WriteAllText(decisionsJson, JsonSerializer.Serialize(gaps.Concat(misses).Select(i => new { i.File, i.Text, i.Type, i.Models, Decision = i.Decision.ToString() }), new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Gaps: {string.Join(", ", gaps.GroupBy(g => g.Decision).Select(g => $"{g.Key} {g.Count()}"))}. Misses: {string.Join(", ", misses.GroupBy(m => m.Decision).Select(m => $"{m.Key} {m.Count()}"))}. Log: {logPath}");
        if (!apply)
        {
            Console.WriteLine("Nothing changed. Add --apply to rewrite the answer-key files.");
            return 0;
        }

        var changedDocs = 0;
        foreach (var gt in truth)
        {
            var path = Path.Combine(corpus, "ground-truth", gt.Id + ".json");
            var node = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
            var entities = node["entities"]!.AsArray();
            var files = rows.Select(r => r.File).Where(f => GroundTruthStore.For(Path.GetFileName(f), [gt]) is not null).Distinct().ToList();
            var textFile = files.FirstOrDefault();
            var text = textFile is null ? string.Empty : File.ReadAllText(Path.Combine(corpus, textFile));
            var ignore = new List<string>();
            foreach (var i in gaps.Where(g => files.Contains(g.File)))
            {
                if (i.Decision == AuditDecision.Add)
                {
                    var found = Scoring.Find(text, i.Text).Count;
                    if (found > 0 && !entities.Any(e => e!["text"]!.GetValue<string>() == i.Text))
                    {
                        entities.Add(new JsonObject { ["where"] = "body", ["type"] = i.Type, ["text"] = i.Text, ["occurrences"] = found, ["audit"] = "added" });
                    }
                }
                else if (i.Decision == AuditDecision.Ignore && !Unjudged.Contains(i.Type))
                {
                    ignore.Add(i.Text);
                }
            }

            foreach (var i in misses.Where(m => files.Contains(m.File) && m.Decision == AuditDecision.Remove))
            {
                for (var k = entities.Count - 1; k >= 0; k--)
                {
                    if (entities[k]!["text"]!.GetValue<string>() == i.Text)
                    {
                        entities.RemoveAt(k);
                    }
                }
            }

            node["judgedCategories"] = new JsonArray(Judged.Select(j => (JsonNode)JsonValue.Create(j)!).ToArray());
            node["ignore"] = new JsonArray(ignore.Distinct().Select(j => (JsonNode)JsonValue.Create(j)!).ToArray());
            File.WriteAllText(path, node.ToJsonString(new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
            changedDocs++;
        }

        Console.WriteLine($"Rewrote the answer key of {changedDocs} documents.");
        return 0;
    }
}
