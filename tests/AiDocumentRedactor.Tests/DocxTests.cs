using System.IO.Compression;
using AiDocumentRedactor.Core;
using AiDocumentRedactor.Documents;
using DocumentFormat.OpenXml.Packaging;
using Xunit;

namespace AiDocumentRedactor.Tests;

/// <summary>Tests for reading and redacting Word documents, using the synthetic .docx files in tests/TestCorpus.</summary>
public class DocxTests
{
    /// <summary>Finds a corpus file by walking up from the test folder to the repository root.</summary>
    static string Corpus(string rel)
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "tests", "TestCorpus", rel)))
        {
            dir = Path.GetDirectoryName(dir);
        }

        return Path.Combine(dir ?? throw new FileNotFoundException(rel), "tests", "TestCorpus", rel);
    }

    /// <summary>Spans for every occurrence of each string in the document text.</summary>
    static List<DetectedEntity> Spans(string text, params string[] needles) =>
        needles.SelectMany(n => Enumerable.Range(0, text.Length).Where(i => string.CompareOrdinal(text, i, n, 0, n.Length) == 0)
            .Select(i => new DetectedEntity("PERSON", i, n.Length, 1, "test"))).ToList();

    /// <summary>All the docx files in the corpus.</summary>
    public static IEnumerable<object[]> Files() =>
        Directory.GetFiles(Path.GetDirectoryName(Corpus("docx/01-hr-letter.docx")) ?? string.Empty, "*.docx").Select(f => new object[] { Path.GetFileName(f) });

    /// <summary>Every corpus document reads to non-empty text.</summary>
    [Theory, MemberData(nameof(Files))]
    public async Task Reads_text(string name)
    {
        var doc = await new DocxDocumentReader().ReadAsync(Corpus("docx/" + name), default);
        Assert.Equal("docx", doc.Format);
        Assert.False(string.IsNullOrWhiteSpace(doc.Text));
    }

    /// <summary>Redacting a name removes it from every XML part of the output and the file still opens as Word.</summary>
    [Theory, MemberData(nameof(Files))]
    public async Task Redacted_output_has_no_secret_anywhere_and_still_opens(string name)
    {
        var doc = await new DocxDocumentReader().ReadAsync(Corpus("docx/" + name), default);
        var secret = doc.Text.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries).First(w => w.Length > 5 && char.IsLetter(w[0]));
        var result = Redactor.Apply(doc.Text, Spans(doc.Text, secret), "[REDACTED:{type}]");
        var outPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".docx");
        try
        {
            await new DocxDocumentWriter().WriteAsync(doc, result, outPath, default);
            using (var zip = ZipFile.OpenRead(outPath))
            {
                foreach (var e in zip.Entries.Where(e => e.FullName.EndsWith(".xml") || e.FullName.EndsWith(".rels")))
                {
                    using var r = new StreamReader(e.Open());
                    Assert.DoesNotContain(secret, r.ReadToEnd());
                }
            }

            using var opened = WordprocessingDocument.Open(outPath, false);
            Assert.NotNull(opened.MainDocumentPart);
            var back = await new DocxDocumentReader().ReadAsync(outPath, default);
            Assert.Contains("[REDACTED:PERSON]", back.Text);
            Assert.DoesNotContain(secret, back.Text);
        }
        finally { File.Delete(outPath); }
    }

    /// <summary>Author and last-modified-by metadata is cleared.</summary>
    [Fact]
    public async Task Metadata_is_scrubbed()
    {
        var src = Corpus("docx/01-hr-letter.docx");
        if (!File.Exists(src))
        {
            src = Directory.GetFiles(Path.GetDirectoryName(src)!, "*.docx").First();
        }

        var doc = await new DocxDocumentReader().ReadAsync(src, default);
        var outPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".docx");
        try
        {
            await new DocxDocumentWriter().WriteAsync(doc, Redactor.Apply(doc.Text, [], "[REDACTED:{type}]"), outPath, default);
            using var d = WordprocessingDocument.Open(outPath, false);
            var p = d.PackageProperties;
            Assert.True(string.IsNullOrEmpty(p.Creator));
            Assert.True(string.IsNullOrEmpty(p.LastModifiedBy));
        }
        finally { File.Delete(outPath); }
    }
}
