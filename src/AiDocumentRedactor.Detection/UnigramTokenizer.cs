using System.Text;
using System.Text.Json;

namespace AiDocumentRedactor.Detection;

/// <summary>What the model runner needs from a tokeniser: encode a word, and know the start, end and special tokens.</summary>
public interface IWordTokenizer
{
    int Cls { get; }
    int Sep { get; }
    int? Special(string content);
    int[] EncodeWord(string word);
}

/// <summary>The SentencePiece (Unigram) tokeniser used by DeBERTa-based GLiNER models, read from a Hugging Face <c>tokenizer.json</c>.
/// Each word is normalised (compatibility forms folded), given the "▁" word-start marker, and split into the most probable pieces.
/// The model file's own normalisation table is approximated by standard Unicode compatibility normalisation, which is the same for ordinary text.</summary>
public sealed class UnigramTokenizer : IWordTokenizer
{
    readonly Dictionary<string, (int Id, double Score)> pieces = new();
    readonly Dictionary<string, int> added = new();
    readonly Dictionary<string, int[]> cache = new();
    readonly int unk;
    readonly int maxPiece;

    public int Cls { get; }
    public int Sep { get; }

    UnigramTokenizer(JsonElement root)
    {
        var model = root.GetProperty("model");
        unk = model.GetProperty("unk_id").GetInt32();
        var id = 0;
        foreach (var entry in model.GetProperty("vocab").EnumerateArray())
        {
            pieces[entry[0].GetString()!] = (id++, entry[1].GetDouble());
        }

        maxPiece = pieces.Keys.Max(k => k.Length);
        foreach (var a in root.GetProperty("added_tokens").EnumerateArray())
        {
            added[a.GetProperty("content").GetString()!] = a.GetProperty("id").GetInt32();
        }

        Cls = added["[CLS]"];
        Sep = added["[SEP]"];
    }

    /// <summary>Reads the tokeniser from a <c>tokenizer.json</c> file.</summary>
    public static UnigramTokenizer Load(string path)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        return new UnigramTokenizer(doc.RootElement);
    }

    public int? Special(string content) => added.TryGetValue(content, out var id) ? id : null;

    public int[] EncodeWord(string word)
    {
        if (cache.TryGetValue(word, out var hit))
        {
            return hit;
        }

        var ids = new List<int>();
        foreach (var part in TextSafe.Clean(word).Normalize(NormalizationForm.FormKC).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        {
            ids.AddRange(Segment("▁" + part));
        }

        return cache[word] = ids.ToArray();
    }

    /// <summary>The most probable split of the text into known pieces (Viterbi); characters no piece covers become the unknown token.</summary>
    List<int> Segment(string s)
    {
        var n = s.Length;
        var best = new double[n + 1];
        var from = new int[n + 1];
        var id = new int[n + 1];
        Array.Fill(best, double.NegativeInfinity);
        best[0] = 0;
        for (var i = 0; i < n; i++)
        {
            if (double.IsNegativeInfinity(best[i]))
            {
                continue;
            }

            var found = false;
            for (var len = 1; len <= Math.Min(maxPiece, n - i); len++)
            {
                if (pieces.TryGetValue(s.Substring(i, len), out var p) && best[i] + p.Score > best[i + len])
                {
                    best[i + len] = best[i] + p.Score;
                    from[i + len] = i;
                    id[i + len] = p.Id;
                    found = true;
                }
            }

            if (!found && best[i] - 20 > best[i + 1])
            {
                best[i + 1] = best[i] - 20;   // a character no piece covers: the unknown token, at a heavy penalty
                from[i + 1] = i;
                id[i + 1] = unk;
            }
        }

        var result = new List<int>();
        for (var at = n; at > 0; at = from[at])
        {
            result.Add(id[at]);
        }

        result.Reverse();
        return result;
    }
}
