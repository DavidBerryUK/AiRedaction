namespace AiDocumentRedactor.Detection;

/// <summary>A piece of the document: its text and where it starts in the whole document.</summary>
public record Chunk(int Start, string Text);

/// <summary>Splits long text into pieces small enough for the model to handle well.</summary>
public static class Chunker
{
    /// <summary>Splits on paragraph boundaries into chunks of roughly maxChars (hard-splits oversize paragraphs).</summary>
    public static List<Chunk> Split(string text, int maxChars)
    {
        var chunks = new List<Chunk>();
        var i = 0;
        while (i < text.Length)
        {
            var end = Math.Min(i + maxChars, text.Length);
            if (end < text.Length)
            {
                var brk = text.LastIndexOf("\n\n", end - 1, end - i, StringComparison.Ordinal);
                if (brk <= i) brk = text.LastIndexOf('\n', end - 1, end - i);
                if (brk <= i) brk = text.LastIndexOf(' ', end - 1, end - i);
                if (brk > i) end = brk + 1;
            }
            chunks.Add(new Chunk(i, text[i..end]));
            i = end;
        }
        return chunks;
    }
}
