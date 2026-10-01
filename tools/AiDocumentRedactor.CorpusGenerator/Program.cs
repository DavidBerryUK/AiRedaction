// Generates the synthetic test corpus: every document in every format, plus the ground-truth answer files.
// Usage: dotnet run --project tools/AiDocumentRedactor.CorpusGenerator -- tests/TestCorpus
using System.Text.Json;
using CorpusGenerator;
using SkiaSharp;

var root = Path.GetFullPath(args.Length > 0 ? args[0] : "TestCorpus");
if (Directory.Exists(root)) Directory.Delete(root, true);
foreach (var f in new[] { "text", "markdown", "csv", "json", "docx", "pdf", "scans", "ground-truth" }) Directory.CreateDirectory(Path.Combine(root, f));
var jo = new JsonSerializerOptions { WriteIndented = true };
int files = 0;
// Builds an output path inside the corpus folder and counts the file.
string Out(string folder, string name) { files++; return Path.Combine(root, folder, name); }

// For each document: render its formats, then write its ground truth (what must be absent / preserved in a redacted copy).
foreach (var d in Corpus.All())
{
    var text = d.RawText != null ? Markup.Strip(d.RawText) : Renderers.PlainText(d);
    foreach (var fmt in d.Formats)
        switch (fmt)
        {
            case "txt": File.WriteAllText(Out("text", d.Id + ".txt"), text); break;
            case "md": File.WriteAllText(Out("markdown", d.Id + ".md"), Renderers.Markdown(d)); break;
            case "csv": File.WriteAllText(Out("csv", d.Id + ".csv"), text); break;
            case "json": File.WriteAllText(Out("json", d.Id + ".json"), text); break;
            case "docx": Renderers.Docx(d, Out("docx", d.Id + ".docx")); break;
            case "pdf": Renderers.TextPdf(d, Out("pdf", d.Id + ".pdf")); break;
            case "scan":
                var lay = new Layout(d);
                if (lay.Pages > 1) throw new InvalidOperationException($"{d.Id} must fit on one page for scans");
                foreach (var kind in d.ScanKinds)
                {
                    using var clean = Renderers.RenderBitmap(lay, 1, kind == "degraded" ? 150 : 300);
                    switch (kind)
                    {
                        case "clean": Renderers.Save(clean, Out("scans", d.Id + "-scan-clean.png"), SKEncodedImageFormat.Png, 100); break;
                        case "degraded":
                            using (var deg = Renderers.Degrade(clean, d.Id.GetHashCode() & 0xffff))
                                Renderers.Save(deg, Out("scans", d.Id + "-scan-degraded.jpg"), SKEncodedImageFormat.Jpeg, 60);
                            break;
                        case "pdf": Renderers.ImageOnlyPdf(clean, Out("scans", d.Id + "-scan.pdf")); break;
                    }
                }
                break;
        }

    // Ground truth: sensitive strings that must be absent from redacted output, and strings that must survive.
    var all = new List<(string where, Entity e)>();
    foreach (var b in d.Blocks)
        foreach (var s in b.Kind == BlockKind.Table ? b.Rows!.SelectMany(r => r) : [b.Text])
            all.AddRange(Markup.Entities(s).Select(e => ("body", e)));
    if (d.RawText != null) all.AddRange(Markup.Entities(d.RawText).Select(e => ("body", e)));
    foreach (var (where, s) in new[] { ("header", d.Header), ("footer", d.Footer), ("comment", d.Comment), ("tracked-deletion", d.TrackedDeletion), ("metadata", d.MetadataAuthor) })
        if (s != null) all.AddRange(Markup.Entities(s).Select(e => (where, e)));
    var truth = new
    {
        id = d.Id, title = d.Title, formats = d.Formats, scanKinds = d.ScanKinds,
        entities = all.GroupBy(x => (x.where, x.e.Type, x.e.Text)).Select(g => new { g.Key.where, type = g.Key.Type, text = g.Key.Text, occurrences = g.Count() }),
        mustPreserve = d.MustPreserve,
    };
    File.WriteAllText(Out("ground-truth", d.Id + ".json"), JsonSerializer.Serialize(truth, jo));
}
Console.WriteLine($"Wrote {files} files to {root}");
