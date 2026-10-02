using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AiDocumentRedactor.Detection;

/// <summary>The byte-level BPE tokeniser used by the GLiNER model's encoder (GPT-2 style), read from a Hugging Face <c>tokenizer.json</c>.
/// Each word is encoded on its own with a leading space, which is how the model was trained to see words.</summary>
public sealed class BpeTokenizer : IWordTokenizer
{
    static readonly Regex Pre = new(@"'s|'t|'re|'ve|'m|'ll|'d| ?\p{L}+| ?\p{N}+| ?[^\s\p{L}\p{N}]+|\s+(?!\S)|\s+", RegexOptions.Compiled);
    static readonly char[] ByteToChar = BuildByteMap();

    readonly Dictionary<string, int> vocab;
    readonly Dictionary<(string, string), int> ranks = new();
    readonly Dictionary<string, int> added = new();
    readonly Dictionary<string, int[]> cache = new();

    /// <summary>Id of the start-of-text token, the end-of-text token and the unknown token.</summary>
    public int Cls
    {
        get;
    }
    public int Sep
    {
        get;
    }
    public int Unk
    {
        get;
    }

    BpeTokenizer(Dictionary<string, int> vocab, JsonElement merges, Dictionary<string, int> added)
    {
        this.vocab = vocab;
        this.added = added;
        var rank = 0;
        foreach (var m in merges.EnumerateArray())
        {
            var (a, b) = m.ValueKind == JsonValueKind.String ? Split(m.GetString()!) : (m[0].GetString()!, m[1].GetString()!);
            ranks[(a, b)] = rank++;
        }

        Cls = added["[CLS]"];
        Sep = added["[SEP]"];
        Unk = added["[UNK]"];
    }

    static (string, string) Split(string s)
    {
        var i = s.IndexOf(' ');
        return (s[..i], s[(i + 1)..]);
    }

    /// <summary>Reads the tokeniser from a <c>tokenizer.json</c> file.</summary>
    public static BpeTokenizer Load(string path)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var model = doc.RootElement.GetProperty("model");
        var vocab = model.GetProperty("vocab").EnumerateObject().ToDictionary(p => p.Name, p => p.Value.GetInt32());
        var added = doc.RootElement.GetProperty("added_tokens").EnumerateArray().ToDictionary(a => a.GetProperty("content").GetString()!, a => a.GetProperty("id").GetInt32());
        return new BpeTokenizer(vocab, model.GetProperty("merges").Clone(), added);
    }

    /// <summary>The id of a special token such as the entity marker, or null.</summary>
    public int? Special(string content) => added.TryGetValue(content, out var id) ? id : null;

    /// <summary>Encodes one word (or label) the way the model expects: normalised, given a leading space, split, and merged into sub-words.</summary>
    public int[] EncodeWord(string word)
    {
        if (cache.TryGetValue(word, out var hit))
        {
            return hit;
        }

        var text = word.Normalize(NormalizationForm.FormC);
        if (!text.StartsWith(' '))
        {
            text = " " + text;
        }

        var ids = new List<int>();
        foreach (Match piece in Pre.Matches(text))
        {
            var mapped = string.Concat(Encoding.UTF8.GetBytes(piece.Value).Select(b => ByteToChar[b]));
            foreach (var sub in Bpe(mapped))
            {
                ids.Add(vocab.TryGetValue(sub, out var id) ? id : Unk);
            }
        }

        return cache[word] = ids.ToArray();
    }

    List<string> Bpe(string mapped)
    {
        var parts = mapped.Select(c => c.ToString()).ToList();
        while (parts.Count > 1)
        {
            var best = -1;
            var bestRank = int.MaxValue;
            for (var i = 0; i < parts.Count - 1; i++)
            {
                if (ranks.TryGetValue((parts[i], parts[i + 1]), out var r) && r < bestRank)
                {
                    bestRank = r;
                    best = i;
                }
            }

            if (best < 0)
            {
                break;
            }

            parts[best] += parts[best + 1];
            parts.RemoveAt(best + 1);
        }

        return parts;
    }

    /// <summary>The GPT-2 byte-to-character table, so every byte has a printable stand-in.</summary>
    static char[] BuildByteMap()
    {
        var map = new char[256];
        var keep = Enumerable.Range('!', '~' - '!' + 1).Concat(Enumerable.Range('¡', '¬' - '¡' + 1)).Concat(Enumerable.Range('®', 'ÿ' - '®' + 1)).ToHashSet();
        var next = 0;
        for (var b = 0; b < 256; b++)
        {
            map[b] = keep.Contains(b) ? (char)b : (char)(256 + next++);
        }

        return map;
    }
}
