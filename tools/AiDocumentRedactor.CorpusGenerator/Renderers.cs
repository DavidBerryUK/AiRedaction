using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SkiaSharp;

namespace CorpusGenerator;

/// <summary>Turns a document definition into each output format.</summary>
public static class Renderers
{
    /// <summary>Plain text version (also used for csv and json).</summary>
    public static string PlainText(DocDef d)
    {
        if (d.RawText is { } raw)
        {
            return Markup.Strip(raw);
        }

        var parts = new List<string>();
        foreach (var b in d.Blocks)
        {
            parts.Add(b.Kind == BlockKind.Table ? string.Join("\n", b.Rows!.Select(r => string.Join(" | ", r.Select(Markup.Strip)))) : Markup.Strip(b.Text));
        }

        return string.Join("\n\n", parts) + "\n";
    }

    /// <summary>Markdown version with headings and tables.</summary>
    public static string Markdown(DocDef d)
    {
        var parts = new List<string>();
        foreach (var b in d.Blocks)
        {
            parts.Add(b.Kind switch {
                BlockKind.Heading => "## " + Markup.Strip(b.Text),
                BlockKind.Table => string.Join("\n", b.Rows!.Select((r, i) => "| " + string.Join(" | ", r.Select(Markup.Strip)) + " |" + (i == 0 ? "\n|" + string.Concat(r.Select(_ => " --- |")) : ""))),
                _ => Markup.Strip(b.Text).Replace("\n", "  \n"),
            });
        }

        return "# " + d.Title + "\n\n" + string.Join("\n\n", parts) + "\n";
    }

    /// <summary>PDF with a real text layer (text can be selected and extracted).</summary>
    public static void TextPdf(DocDef d, string path)
    {
        var lay = new Layout(d);
        using var stream = File.Create(path);
        using var doc = SKDocument.CreatePdf(stream);
        for (var p = 1; p <= lay.Pages; p++)
        {
            using var c = doc.BeginPage(Layout.W, Layout.H);
            lay.DrawPage(c, p);
            doc.EndPage();
        }
        doc.Close();
    }

    /// <summary>Draws a page to an image at the given DPI.</summary>
    public static SKBitmap RenderBitmap(Layout lay, int page, float dpi)
    {
        var s = dpi / 72f;
        var bmp = new SKBitmap((int)(Layout.W * s), (int)(Layout.H * s));
        using var c = new SKCanvas(bmp);
        c.Scale(s);
        lay.DrawPage(c, page);
        return bmp;
    }

    /// <summary>Photocopy-like degradation: rotation, blur, noise, JPEG artefacts.</summary>
    public static SKBitmap Degrade(SKBitmap src, int seed)
    {
        var rnd = new Random(seed);
        var bmp = new SKBitmap(src.Width, src.Height);
        using (var c = new SKCanvas(bmp))
        {
            c.Clear(new SKColor(250, 249, 245));
            c.RotateDegrees(0.9f, src.Width / 2f, src.Height / 2f);
            using var paint = new SKPaint { ImageFilter = SKImageFilter.CreateBlur(0.7f, 0.7f) };
            c.DrawBitmap(src, 0, 0, paint);
        }
        var px = bmp.Pixels;
        for (var i = 0; i < px.Length; i++)
        {
            var n = rnd.Next(-22, 23);
            var o = px[i];
            px[i] = new SKColor((byte)Math.Clamp(o.Red + n, 0, 255), (byte)Math.Clamp(o.Green + n, 0, 255), (byte)Math.Clamp(o.Blue + n, 0, 255));
        }
        bmp.Pixels = px;
        using var img = SKImage.FromBitmap(bmp);
        using var jpg = img.Encode(SKEncodedImageFormat.Jpeg, 45);
        return SKBitmap.Decode(jpg);
    }

    /// <summary>Writes an image to disk as PNG or JPEG.</summary>
    public static void Save(SKBitmap bmp, string path, SKEncodedImageFormat fmt, int q)
    {
        using var img = SKImage.FromBitmap(bmp);
        using var data = img.Encode(fmt, q);
        File.WriteAllBytes(path, data.ToArray());
    }

    /// <summary>PDF containing only a page image, with no text layer (a scanned PDF).</summary>
    public static void ImageOnlyPdf(SKBitmap bmp, string path)
    {
        using var stream = File.Create(path);
        using var doc = SKDocument.CreatePdf(stream);
        using var c = doc.BeginPage(Layout.W, Layout.H);
        using var img = SKImage.FromBitmap(bmp);
        c.DrawImage(img, new SKRect(0, 0, Layout.W, Layout.H));
        doc.EndPage();
        doc.Close();
    }

    // ---------- DOCX ----------
    /// <summary>Builds Word runs for marked-up text, optionally split into tiny runs.</summary>
    static IEnumerable<Run> RunsFor(string markedUp, bool bold, bool split)
    {
        foreach (var (text, _) in Markup.Segments(markedUp))
        {
            var chunks = split ? Enumerable.Range(0, (text.Length + 5) / 6).Select(i => text.Substring(i * 6, Math.Min(6, text.Length - i * 6))) : [text];
            foreach (var ch in chunks)
            {
                var parts = ch.Split('\n');
                var run = new Run();
                if (bold)
                {
                    run.Append(new RunProperties(new Bold(), new FontSize { Val = "30" }));
                }

                for (var i = 0; i < parts.Length; i++)
                {
                    if (i > 0)
                    {
                        run.Append(new Break());
                    }

                    run.Append(new Text(parts[i]) { Space = SpaceProcessingModeValues.Preserve });
                }
                yield return run;
            }
        }
    }

    /// <summary>Word document with body, table, header/footer, comment, tracked deletion and author metadata as the definition asks.</summary>
    public static void Docx(DocDef d, string path)
    {
        using var doc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var main = doc.AddMainDocumentPart();
        main.Document = new Document();
        var body = main.Document.AppendChild(new Body());
        var first = true;
        foreach (var b in d.Blocks)
        {
            if (b.Kind == BlockKind.Table)
            {
                var t = new Table(new TableProperties(new TableBorders(
                    new TopBorder { Val = BorderValues.Single, Size = 4 }, new BottomBorder { Val = BorderValues.Single, Size = 4 },
                    new LeftBorder { Val = BorderValues.Single, Size = 4 }, new RightBorder { Val = BorderValues.Single, Size = 4 },
                    new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 }, new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 })));
                foreach (var r in b.Rows!)
                {
                    var tr = new TableRow();
                    foreach (var cell in r)
                    {
                        tr.Append(new TableCell(new Paragraph(RunsFor(cell, false, d.SplitRuns))));
                    }

                    t.Append(tr);
                }
                body.Append(t);
                body.Append(new Paragraph());
                continue;
            }
            var para = new Paragraph();
            if (first && d.Comment != null)
            {
                para.Append(new CommentRangeStart { Id = "0" });
            }

            para.Append(RunsFor(b.Text, b.Kind == BlockKind.Heading, d.SplitRuns));
            if (first && d.Comment != null)
            {
                para.Append(new CommentRangeEnd { Id = "0" });
                para.Append(new Run(new CommentReference { Id = "0" }));
            }
            if (first && d.TrackedDeletion != null)
            {
                para.Append(new DeletedRun(new Run(new DeletedText(Markup.Strip(d.TrackedDeletion)) { Space = SpaceProcessingModeValues.Preserve })) {
                    Author = "Reviewer",
                    Date = new DateTime(2025, 10, 1),
                    Id = "1"
                });
            }

            body.Append(para);
            first = false;
        }
        if (d.Header != null)
        {
            var hp = main.AddNewPart<HeaderPart>();
            hp.Header = new Header(new Paragraph(RunsFor(d.Header, false, false)));
            hp.Header.Save();
            var fp = d.Footer != null ? main.AddNewPart<FooterPart>() : null;
            if (fp != null)
            {
                fp.Footer = new Footer(new Paragraph(RunsFor(d.Footer!, false, false)));
                fp.Footer.Save();
            }
            var sect = new SectionProperties(new HeaderReference { Type = HeaderFooterValues.Default, Id = main.GetIdOfPart(hp) });
            if (fp != null)
            {
                sect.Append(new FooterReference { Type = HeaderFooterValues.Default, Id = main.GetIdOfPart(fp) });
            }

            body.Append(sect);
        }
        if (d.Comment != null)
        {
            var cp = main.AddNewPart<WordprocessingCommentsPart>();
            cp.Comments = new Comments(new Comment(new Paragraph(RunsFor(d.Comment, false, false))) { Id = "0", Author = "Reviewer", Initials = "R", Date = new DateTime(2025, 10, 1) });
            cp.Comments.Save();
        }
        main.Document.Save();
        if (d.MetadataAuthor != null)
        {
            doc.PackageProperties.Creator = Markup.Strip(d.MetadataAuthor);
            doc.PackageProperties.LastModifiedBy = Markup.Strip(d.MetadataAuthor);
            doc.PackageProperties.Title = d.Title;
        }
    }
}
