using SkiaSharp;

namespace CorpusGenerator;

/// <summary>One line of laid-out text and where it sits on its page.</summary>
public record Line(int Page, float Y, string Text, bool Heading);

/// <summary>Simple A4 text layout shared by text-PDF and scan rendering.</summary>
public class Layout
{
    public const float W = 595, H = 842, M = 56;
    public List<Line> Lines { get; } = [];
    public int Pages { get; private set; } = 1;
    readonly SKFont body = new(SKTypeface.FromFamilyName("Helvetica"), 11);
    readonly SKFont head = new(SKTypeface.FromFamilyName("Helvetica", SKFontStyle.Bold), 15);
    public SKFont Body => body; public SKFont Head => head;

    /// <summary>Lays out all blocks of a document into lines across pages.</summary>
    public Layout(DocDef d)
    {
        float y = M + 14;
        var page = 1;
        /// <summary>Wraps one block's text into lines, starting a new page when the current one is full.</summary>
        void Add(string text, bool heading)
        {
            var f = heading ? head : body;
            var lh = heading ? 22f : 15f;
            foreach (var raw in text.Split('\n'))
            {
                foreach (var l in Wrap(raw, f, W - 2 * M))
                {
                    if (y > H - M)
                    {
                        page++;
                        y = M + 14;
                    }
                    Lines.Add(new Line(page, y, l, heading));
                    y += lh;
                }
            }

            y += heading ? 4 : 8;
        }
        foreach (var b in d.Blocks)
        {
            if (b.Kind == BlockKind.Table)
            {
                foreach (var r in b.Rows!)
                {
                    Add(string.Join("   |   ", r.Select(Markup.Strip)), false);
                }
            }
            else
            {
                Add(Markup.Strip(b.Text), b.Kind == BlockKind.Heading);
            }
        }
        Pages = page;
    }

    /// <summary>Breaks a line into pieces that fit the page width.</summary>
    static IEnumerable<string> Wrap(string s, SKFont f, float max)
    {
        var cur = "";
        foreach (var w in s.Split(' '))
        {
            var t = cur.Length == 0 ? w : cur + " " + w;
            if (f.MeasureText(t) > max && cur.Length > 0)
            {
                yield return cur;
                cur = w;
            }
            else
            {
                cur = t;
            }
        }
        yield return cur;
    }

    /// <summary>Draws one page's lines onto a canvas.</summary>
    public void DrawPage(SKCanvas c, int page)
    {
        c.Clear(SKColors.White);
        using var p = new SKPaint { Color = SKColors.Black, IsAntialias = true };
        foreach (var l in Lines.Where(l => l.Page == page))
        {
            c.DrawText(l.Text, M, l.Y, l.Heading ? head : body, p);
        }
    }
}
