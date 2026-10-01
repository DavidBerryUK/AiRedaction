namespace AiDocumentRedactor.Detection;

/// <summary>A piece of the document: its text and where it starts in the whole document.</summary>
public record Chunk(int Start, string Text);

/// <summary>Splits long text into pieces small enough for the model to handle well.</summary>
public static class Chunker
{
    /// <summary>Splits into chunks of roughly maxChars, preferring a blank line, then a line break, then a space, so words and
    /// paragraphs stay whole. Each chunk after the first starts <c>overlapChars</c> before the previous one ended (at a word boundary),
    /// so something that straddles a break is seen whole in at least one chunk.</summary>
    public static List<Chunk> Split(string text, int maxChars, int overlapChars = 0)
    {
        var chunks = new List<Chunk>();
        var i = 0;
        while (i < text.Length)
        {
            var end = Math.Min(i + maxChars, text.Length);
            if (end < text.Length)
            {
                var brk = text.LastIndexOf("\n\n", end - 1, end - i, StringComparison.Ordinal);
                if (brk <= i)
                {
                    brk = text.LastIndexOf('\n', end - 1, end - i);
                }

                if (brk <= i)
                {
                    brk = text.LastIndexOf(' ', end - 1, end - i);
                }

                if (brk > i)
                {
                    end = brk + 1;
                }
            }
            chunks.Add(new Chunk(i, text[i..end]));
            if (end >= text.Length)
            {
                break;
            }

            var next = end - Math.Max(0, overlapChars);
            if (next > i && next < end)
            {
                var ws = text.IndexOfAny([' ', '\n'], next, end - next);
                next = ws >= 0 ? ws + 1 : end;
            }   // start at a word boundary
            i = next > i ? next : end;
        }
        return chunks;
    }
}
