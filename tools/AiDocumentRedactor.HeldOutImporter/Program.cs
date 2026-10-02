// Builds a held-out test corpus from the English test split of gretelai/synthetic_pii_finance_multilingual (Apache 2.0, fully synthetic,
// no real people), through the dataset's public data API. Each document comes with labelled PII positions, which are converted to this
// project's answer-key format. None of these documents was used to write a rule, choose a threshold or tune a prompt, so they make an
// honest test of how the system does on documents it has not been shaped around.
// Usage (from the repo root): dotnet run --project tools/AiDocumentRedactor.HeldOutImporter -- [--per-type 5] [--out tests/HeldOutCorpus] [--copy-to in/finance-sample]
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

string? Arg(string name)
{
    var i = Array.IndexOf(args, name);
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
}

var perType = int.Parse(Arg("--per-type") ?? "5");
var outDir = Path.GetFullPath(Arg("--out") ?? Path.Combine("tests", "HeldOutCorpus"));
var copyTo = Arg("--copy-to") is { } c ? Path.GetFullPath(c) : null;
const string Dataset = "gretelai/synthetic_pii_finance_multilingual";

// The dataset's labels, mapped to our categories. Labels that are not sensitive under our policy (generic dates and times, coordinates, bank
// identifier codes) are left off the answer key, so a model that redacts them is correctly counted as over-redacting.
var map = new Dictionary<string, string>
{
    ["name"] = "PERSON", ["first_name"] = "PERSON", ["last_name"] = "PERSON",
    ["company"] = "COMPANY", ["street_address"] = "ADDRESS", ["email"] = "EMAIL", ["phone_number"] = "PHONE", ["date_of_birth"] = "DATE_OF_BIRTH",
    ["ssn"] = "ID_NUMBER", ["passport_number"] = "ID_NUMBER", ["driver_license_number"] = "ID_NUMBER", ["iban"] = "ID_NUMBER", ["bban"] = "ID_NUMBER",
    ["bank_routing_number"] = "ID_NUMBER", ["credit_card_number"] = "ID_NUMBER", ["credit_card_security_code"] = "ID_NUMBER", ["account_pin"] = "ID_NUMBER",
    ["employee_id"] = "ID_NUMBER", ["customer_id"] = "ID_NUMBER",
    ["ipv4"] = "ONLINE_ID", ["ipv6"] = "ONLINE_ID", ["user_name"] = "ONLINE_ID", ["password"] = "SECRET", ["api_key"] = "SECRET",
};

using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
http.DefaultRequestHeaders.UserAgent.ParseAdd("AiDocumentRedactor-HeldOutImporter/1.0");
async Task<string> GetWithRetryAsync(string url)
{
    // The public data service limits how fast it can be asked. Wait as long as it says (Retry-After), or longer and longer, and ask again.
    for (var attempt = 1; ; attempt++)
    {
        using var response = await http.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }

        var retryable = (int)response.StatusCode is 429 or 502 or 503 or 504;
        if (!retryable || attempt >= 12)
        {
            response.EnsureSuccessStatusCode();
        }

        var wait = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(Math.Min(60, 5 * attempt));
        Console.WriteLine($"  the service said {(int)response.StatusCode}; waiting {wait.TotalSeconds:0} s ...");
        await Task.Delay(wait);
    }
}

var rows = new List<JsonElement>();
Console.WriteLine($"Reading the English test documents of {Dataset} ...");
for (var offset = 0; ; offset += 100)
{
    var url = $"https://datasets-server.huggingface.co/rows?dataset={Dataset}&config=default&split=test&offset={offset}&length=100";
    using var doc = JsonDocument.Parse(await GetWithRetryAsync(url));
    var batch = doc.RootElement.GetProperty("rows");
    foreach (var r in batch.EnumerateArray())
    {
        var row = r.GetProperty("row");
        if (row.GetProperty("language").GetString() == "English")
        {
            rows.Add(row.Clone());
        }
    }

    if (offset + 100 >= doc.RootElement.GetProperty("num_rows_total").GetInt32())
    {
        break;
    }

    await Task.Delay(1200);   // one request a second or so, to be polite to a free public service
}

Console.WriteLine($"{rows.Count} English documents. Choosing up to {perType} per document type (short, high quality first).");
var chosen = rows
    .Where(r => r.GetProperty("generated_text").GetString()!.Length is >= 150 and <= 5000 && r.GetProperty("quality_score").GetInt32() >= 80)
    .GroupBy(r => r.GetProperty("document_type").GetString()!)
    .OrderBy(g => g.Key)
    .SelectMany(g => g.OrderByDescending(r => r.GetProperty("quality_score").GetInt32()).ThenBy(r => r.GetProperty("index").GetInt32()).Take(perType))
    .ToList();

var textDir = Path.Combine(outDir, "text");
var truthDir = Path.Combine(outDir, "ground-truth");
Directory.CreateDirectory(textDir);
Directory.CreateDirectory(truthDir);
foreach (var old in Directory.GetFiles(textDir).Concat(Directory.GetFiles(truthDir)))
{
    File.Delete(old);
}

var n = 0;
var byType = new Dictionary<string, int>();
var totalEntities = 0;
var written = new List<(string Id, string Title)>();
foreach (var r in chosen)
{
    n++;
    var text = r.GetProperty("generated_text").GetString()!;
    var title = r.GetProperty("document_type").GetString()!;
    var slug = Regex.Replace(title.ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
    var id = $"heldout-{n:000}-{slug}";
    var spansRaw = r.GetProperty("pii_spans");
    using var spans = JsonDocument.Parse(spansRaw.ValueKind == JsonValueKind.String ? spansRaw.GetString()! : spansRaw.GetRawText());
    var entities = spans.RootElement.EnumerateArray()
        .Select(s => (Label: s.GetProperty("label").GetString()!, Start: s.GetProperty("start").GetInt32(), End: s.GetProperty("end").GetInt32()))
        .Where(s => map.ContainsKey(s.Label) && s.Start >= 0 && s.End <= text.Length && s.End - s.Start >= 2)
        .Select(s => (Type: map[s.Label], Text: text[s.Start..s.End].Trim()))
        .Where(s => s.Text.Length >= 2)
        .GroupBy(s => (s.Type, s.Text))
        .Select(g => new { where = "body", type = g.Key.Type, text = g.Key.Text, occurrences = g.Count() })
        .ToList();
    File.WriteAllText(Path.Combine(textDir, id + ".txt"), text, new UTF8Encoding(false));
    File.WriteAllText(Path.Combine(truthDir, id + ".json"), JsonSerializer.Serialize(
        new { id, title = $"{title} (synthetic finance document)", formats = new[] { "txt" }, scanKinds = Array.Empty<string>(), entities, mustPreserve = Array.Empty<string>() },
        new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
    byType[title] = byType.GetValueOrDefault(title) + 1;
    totalEntities += entities.Count;
    written.Add((id, title));
}

File.WriteAllText(Path.Combine(outDir, "README.md"), $"""
    # Held-out corpus (finance documents)

    {n} English documents from the **test split** of [{Dataset}](https://huggingface.co/datasets/{Dataset}) (Apache 2.0, fully synthetic: no real
    individuals are represented), covering {byType.Count} document types, with {totalEntities} labelled sensitive items (distinct text per document).
    Imported by `tools/AiDocumentRedactor.HeldOutImporter`; to rebuild, run it from the repository root.

    **Why it exists.** No rule, threshold, prompt or answer key in this project was written after looking at these documents, so scores on them
    show how the system does on documents it has not been shaped around. Do not tune against it: if you do, build a fresh one.

    **How the answer key was made.** The dataset labels each sensitive span. They are mapped to this project's categories: names to PERSON,
    company to COMPANY, street addresses to ADDRESS, emails to EMAIL, phone numbers to PHONE, dates of birth to DATE_OF_BIRTH, national, passport,
    licence, bank, card, employee and customer numbers to ID_NUMBER, IP addresses and user names to ONLINE_ID, passwords and API keys to SECRET.
    Generic dates and times, coordinates and bank identifier codes are **not** on the key (they are not sensitive under this project's policy), so a
    model that redacts them counts as over-redacting.

    **Limits.** These are generated finance documents (loan applications, policies, contracts, emails, support tickets, statements and machine
    formats such as SWIFT, FIX and XBRL), not letters, medical notes or scans. The labels come from the generator and were not checked by a
    person, so a few may be missing or wrong; treat small differences between models with caution. Text only (no Word, PDF or image versions).

    Documents by type: {string.Join("; ", byType.OrderBy(k => k.Key).Select(k => $"{k.Key} ({k.Value})"))}.
    """.Replace("\n    ", "\n").TrimStart(), new UTF8Encoding(false));

if (copyTo is not null)
{
    Directory.CreateDirectory(copyTo);
    foreach (var f in Directory.GetFiles(textDir))
    {
        File.Copy(f, Path.Combine(copyTo, Path.GetFileName(f)), true);
    }
}

Console.WriteLine($"Wrote {n} documents ({byType.Count} types, {totalEntities} labelled items) to {outDir}{(copyTo is null ? "" : $" and copied them to {copyTo}")}.");
