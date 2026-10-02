// Spike: how well does a GLiNER PII model do on our corpus, running in .NET through ONNX Runtime, before any change to the application?
// Scores it with the evaluation's own scoring (strict recall, overlap precision, must-keep check) so it compares directly with the chat models.
// Usage (from the repo root): dotnet tools/AiDocumentRedactor.GlinerSpike/bin/Debug/net10.0/AiDocumentRedactor.GlinerSpike.dll
//        [--model models/gliner-pii-edge] [--corpus tests/TestCorpus] [--only text/] [--thresholds 0.3,0.5,0.7] [--with-rules] [--try "hello text"]
using System.Diagnostics;
using AiDocumentRedactor.Core;
using AiDocumentRedactor.Detection;
using AiDocumentRedactor.Documents;
using AiDocumentRedactor.Eval;
using AiDocumentRedactor.GlinerSpike;

string? Arg(string name)
{
    var i = Array.IndexOf(args, name);
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
}

// Our categories as labels for the model. Several labels may stand for one category. Label wording can change results a lot, so there are
// named sets to compare: A = short everyday words, B = the model's own training vocabulary, C = B plus extra wording for companies and context.
var labelSets = new Dictionary<string, Dictionary<string, string[]>>
{
    ["A"] = new()
    {
        [EntityTypes.Person] = ["person"], [EntityTypes.Phone] = ["phone number"], [EntityTypes.Email] = ["email"], [EntityTypes.Address] = ["address"],
        [EntityTypes.IdNumber] = ["identification number"], [EntityTypes.OnlineId] = ["ip address"], [EntityTypes.Age] = ["age"],
        [EntityTypes.DateOfBirth] = ["date of birth"], [EntityTypes.Gender] = ["gender"], [EntityTypes.Company] = ["organization"],
        [EntityTypes.CompanyId] = ["company registration number"], [EntityTypes.Domain] = ["website"], [EntityTypes.Contextual] = ["job title"],
        [EntityTypes.Secret] = ["password"],
    },
    ["B"] = new()
    {
        [EntityTypes.Person] = ["name", "first name", "last name"], [EntityTypes.Phone] = ["phone number"], [EntityTypes.Email] = ["email address"],
        [EntityTypes.Address] = ["location address", "location street", "location zip"],
        [EntityTypes.IdNumber] = ["account number", "bank account", "credit card", "ssn", "passport number", "driver license", "healthcare number"],
        [EntityTypes.OnlineId] = ["ip address", "username"], [EntityTypes.Age] = ["age"], [EntityTypes.DateOfBirth] = ["dob"], [EntityTypes.Gender] = ["gender"],
        [EntityTypes.Company] = ["organization"], [EntityTypes.CompanyId] = ["company registration number"], [EntityTypes.Domain] = ["url"],
        [EntityTypes.Contextual] = ["occupation"], [EntityTypes.Secret] = ["password"],
    },
};
labelSets["C"] = labelSets["B"].ToDictionary(kv => kv.Key, kv => kv.Key switch
{
    EntityTypes.Company => new[] { "organization", "company name" },
    EntityTypes.Contextual => ["occupation", "job title", "project name"],
    _ => kv.Value,
});
var labelSet = labelSets[Arg("--labelset") ?? "A"];
var typeOf = labelSet.SelectMany(kv => kv.Value.Select(l => (Label: l, Type: kv.Key))).ToDictionary(x => x.Label, x => x.Type);
var labels = typeOf.Keys.ToList();

using var model = new GlinerModel(Arg("--model") ?? "models/gliner-pii-edge");
if (args.Contains("--inspect"))
{
    Console.WriteLine(model.Describe());
    Environment.Exit(0);
}


if (Arg("--try") is { } sample)
{
    var p = model.Run(sample, labels);
    foreach (var s in GlinerModel.Decode(p, 0.3))
    {
        Console.WriteLine($"{s.Score:0.00}  {s.Label,-28} [{sample.Substring(s.Start, s.Length)}]");
    }

    Environment.Exit(0);
}

var corpus = Path.GetFullPath(Arg("--corpus") ?? Path.Combine("tests", "TestCorpus"));
var only = Arg("--only") ?? "text/";
var thresholds = (Arg("--thresholds") ?? "0.3,0.4,0.5,0.6,0.7,0.8").Split(',').Select(double.Parse).ToArray();
var withRules = args.Contains("--with-rules");
var options = RedactorOptions.Load("redactor.config.json");
var truth = GroundTruthStore.Load(corpus);
IDocumentReader[] readers = [new TextDocumentReader(), new DocxDocumentReader(), new PdfDocumentReader()];

var docs = new List<(string Rel, string Group, GroundTruth Gt, string Text, Prepared Scores)>();
var clock = Stopwatch.StartNew();
foreach (var f in Directory.EnumerateFiles(corpus, "*", SearchOption.AllDirectories).Where(f => !f.Contains("ground-truth") && !f.EndsWith("README.md")).Order())
{
    var rel = Path.GetRelativePath(corpus, f);
    var gt = GroundTruthStore.For(Path.GetFileName(f), truth);
    var reader = readers.FirstOrDefault(r => r.CanRead(f));
    if (!rel.Contains(only, StringComparison.OrdinalIgnoreCase) || gt is null || reader is null)
    {
        continue;
    }

    try
    {
        var doc = await reader.ReadAsync(f, CancellationToken.None);
        docs.Add((rel, GroundTruthStore.FormatGroup(f), gt, doc.Text, model.Run(doc.Text, labels)));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"skipped {rel}: {ex.Message}");
    }
}

Console.WriteLine($"{docs.Count} documents scored in {clock.Elapsed.TotalSeconds:0.0} s ({clock.Elapsed.TotalSeconds / Math.Max(1, docs.Count):0.00} s per document, including reading the files){(withRules ? "; rules layer added" : "")}\n");
Console.WriteLine("threshold  recall   precision  F1      missed        over-redactions  must-keep damaged");
Totals? at05 = null;
List<DocScore>? scores05 = null;
foreach (var th in thresholds)
{
    var scores = new List<DocScore>();
    foreach (var (rel, group, gt, text, prepared) in docs)
    {
        var spans = GlinerModel.Decode(prepared, th)
            .Select(s => new DetectedEntity(typeOf[s.Label], s.Start, s.Length, s.Score, "gliner")).ToList();
        if (withRules)
        {
            spans.AddRange(RuleDetector.Find(text, options));
        }

        var result = Redactor.Apply(text, spans, options.Redaction.PlaceholderTemplate);
        var score = Scoring.Score(text, result, gt, group);
        score.File = rel;
        score.Model = "gliner";
        scores.Add(score);
    }

    var t = Totals.Of(scores);
    Console.WriteLine($"{th,-10:0.0#} {t.Recall,7:P1}  {t.Precision,8:P1}  {t.F1,6:P1}  {t.Leaked,4} of {t.Present,-4}  {t.Edits - t.TruePositives,4} of {t.Edits,-4}      {t.PreserveBroken} of {t.PreserveTotal}");
    if (Math.Abs(th - 0.5) < 1e-9 || at05 is null && th == thresholds[thresholds.Length / 2])
    {
        at05 = t;
        scores05 = scores;
    }
}

if (scores05 is not null)
{
    Console.WriteLine("\nRecall by category at the middle threshold");
    foreach (var g in scores05.SelectMany(s => s.ByCategory).GroupBy(kv => kv.Key).OrderBy(g => g.Key))
    {
        Console.WriteLine($"  {g.Key,-14} {g.Sum(x => x.Value.Caught)}/{g.Sum(x => x.Value.Present)}");
    }

    Console.WriteLine("\nMissed at the middle threshold");
    foreach (var s in scores05)
    {
        foreach (var l in s.Leaks)
        {
            Console.WriteLine($"  {s.File} {l.Type}: {l.Text} ({l.Count})");
        }
    }

    Console.WriteLine("\nOver-redacted at the middle threshold (counts)");
    foreach (var g in scores05.SelectMany(s => s.FalsePositives).GroupBy(f => (f.Type, f.Text)).OrderByDescending(g => g.Count()).Take(15))
    {
        Console.WriteLine($"  {g.Key.Type}: {g.Key.Text} ({g.Count()})");
    }
}

Environment.Exit(0);
