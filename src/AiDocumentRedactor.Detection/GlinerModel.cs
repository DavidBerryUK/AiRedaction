using System.Text.RegularExpressions;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace AiDocumentRedactor.Detection;

/// <summary>One stretch of the document that GLiNER marked: where it is, which label matched, and how sure the model was.</summary>
public record GlinerSpan(int Start, int Length, string Label, double Score);

/// <summary>The model's raw scores for one document, kept so several thresholds can be tried without running the model again.</summary>
public sealed class Prepared
{
    public List<(int Start, int End)> Words = [];
    public List<string> Labels = [];
    /// <summary>One entry per window: the index of its first word, its word count, and the sigmoid scores [word, label, start/end/inside].</summary>
    public List<(int FirstWord, int WordCount, float[] Scores)> Windows = [];
}

/// <summary>Runs a token-level GLiNER model (ONNX) over text: splits it into words, builds the label prompt, reads long text in
/// overlapping windows, and turns the scores into spans. Everything runs in this process; nothing leaves the machine.</summary>
public sealed class GlinerModel : IDisposable
{
    static readonly Regex WordPattern = new(@"\w+(?:[-_]\w+)*|\S", RegexOptions.Compiled);
    const int OverlapWords = 30, MaxSpanWords = 30;

    readonly InferenceSession session;
    readonly IWordTokenizer tokenizer;
    readonly int maxTokens;
    readonly int entToken, sepToken;

    public GlinerModel(string dir, string onnxFile = "model_quint8.onnx")
    {
        session = new InferenceSession(Path.Combine(dir, onnxFile));
        var tokenizerPath = Path.Combine(dir, "tokenizer.json");
        using (var doc = System.Text.Json.JsonDocument.Parse(File.ReadAllText(tokenizerPath)))
        {
            tokenizer = doc.RootElement.GetProperty("model").GetProperty("type").GetString() == "Unigram" ? UnigramTokenizer.Load(tokenizerPath) : BpeTokenizer.Load(tokenizerPath);
        }

        using (var cfg = System.Text.Json.JsonDocument.Parse(File.ReadAllText(Path.Combine(dir, "gliner_config.json"))))
        {
            maxTokens = cfg.RootElement.GetProperty("max_len").GetInt32() - 8;   // the model's own length limit, with a little room
        }

        entToken = tokenizer.Special("<<ENT>>") ?? throw new InvalidDataException("No <<ENT>> token");
        sepToken = tokenizer.Special("<<SEP>>") ?? throw new InvalidDataException("No <<SEP>> token");
    }

    /// <summary>The model's input and output names, for checking it is the kind this runner expects.</summary>
    public string Describe() => "inputs: " + string.Join(", ", session.InputMetadata.Keys) + "; outputs: " + string.Join(", ", session.OutputMetadata.Select(o => $"{o.Key} [{string.Join(",", o.Value.Dimensions)}]"));

    /// <summary>Runs the model over the text for the given labels.</summary>
    public Prepared Run(string text, IReadOnlyList<string> labels)
    {
        var p = new Prepared { Labels = [.. labels] };
        foreach (Match m in WordPattern.Matches(text))
        {
            p.Words.Add((m.Index, m.Index + m.Length));
        }

        var prompt = new List<int>();
        foreach (var l in labels)
        {
            prompt.Add(entToken);
            prompt.AddRange(tokenizer.EncodeWord(l));
        }

        prompt.Add(sepToken);
        var wordTokens = p.Words.Select(w => tokenizer.EncodeWord(text[w.Start..w.End])).ToList();
        var budget = maxTokens - prompt.Count - 2;
        for (var first = 0; first < p.Words.Count;)
        {
            var count = 0;
            var used = 0;
            while (first + count < p.Words.Count && (count == 0 || used + wordTokens[first + count].Length <= budget))
            {
                used += wordTokens[first + count].Length;
                count++;
            }

            p.Windows.Add((first, count, Infer(prompt, wordTokens.Skip(first).Take(count).ToList(), labels.Count)));
            if (first + count >= p.Words.Count)
            {
                break;
            }

            first += Math.Max(1, count - OverlapWords);
        }

        return p;
    }

    float[] Infer(List<int> prompt, List<int[]> words, int labelCount)
    {
        var ids = new List<long> { tokenizer.Cls };
        var wordsMask = new List<long> { 0 };
        ids.AddRange(prompt.Select(x => (long)x));
        wordsMask.AddRange(prompt.Select(_ => 0L));
        for (var w = 0; w < words.Count; w++)
        {
            for (var t = 0; t < words[w].Length; t++)
            {
                ids.Add(words[w][t]);
                wordsMask.Add(t == 0 ? w + 1 : 0);
            }
        }

        ids.Add(tokenizer.Sep);
        wordsMask.Add(0);
        var n = ids.Count;
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", new DenseTensor<long>(ids.ToArray(), new[] { 1, n })),
            NamedOnnxValue.CreateFromTensor("attention_mask", new DenseTensor<long>(Enumerable.Repeat(1L, n).ToArray(), new[] { 1, n })),
            NamedOnnxValue.CreateFromTensor("words_mask", new DenseTensor<long>(wordsMask.ToArray(), new[] { 1, n })),
            NamedOnnxValue.CreateFromTensor("text_lengths", new DenseTensor<long>(new long[] { words.Count }, new[] { 1, 1 })),
        };
        using var result = session.Run(inputs);
        var logits = result.First().AsTensor<float>();
        var dims = logits.Dimensions.ToArray();
        var wordsOut = dims[1];
        var scores = new float[words.Count * labelCount * 3];
        for (var w = 0; w < Math.Min(words.Count, wordsOut); w++)
        {
            for (var c = 0; c < labelCount; c++)
            {
                for (var k = 0; k < 3; k++)
                {
                    scores[(w * labelCount + c) * 3 + k] = 1f / (1f + MathF.Exp(-logits[0, w, c, k]));
                }
            }
        }

        return scores;
    }

    /// <summary>Turns the scores into spans at a threshold: a span runs from a word whose start score is above it to a word whose end score is above it,
    /// with every word between them scored as inside. Overlaps are resolved by keeping the more confident span.</summary>
    public static List<GlinerSpan> Decode(Prepared p, double threshold)
    {
        var candidates = new List<(int From, int To, int Label, double Score)>();
        var labelCount = p.Labels.Count;
        foreach (var (first, count, scores) in p.Windows)
        {
            float S(int w, int c, int k) => scores[(w * labelCount + c) * 3 + k];
            for (var c = 0; c < labelCount; c++)
            {
                for (var s = 0; s < count; s++)
                {
                    if (S(s, c, 0) <= threshold)
                    {
                        continue;
                    }

                    var minInside = float.MaxValue;
                    for (var e = s; e < Math.Min(count, s + MaxSpanWords); e++)
                    {
                        minInside = Math.Min(minInside, S(e, c, 2));
                        if (minInside <= threshold)
                        {
                            break;
                        }

                        if (S(e, c, 1) > threshold)
                        {
                            candidates.Add((first + s, first + e, c, (S(s, c, 0) + S(e, c, 1)) / 2.0));
                        }
                    }
                }
            }
        }

        var kept = new List<(int From, int To, int Label, double Score)>();
        foreach (var c in candidates.OrderByDescending(x => x.Score).ThenByDescending(x => x.To - x.From))
        {
            if (!kept.Any(k => c.From <= k.To && k.From <= c.To))
            {
                kept.Add(c);
            }
        }

        return kept.OrderBy(k => k.From).Select(k => new GlinerSpan(p.Words[k.From].Start, p.Words[k.To].End - p.Words[k.From].Start, p.Labels[k.Label], k.Score)).ToList();
    }

    public void Dispose() => session.Dispose();
}
