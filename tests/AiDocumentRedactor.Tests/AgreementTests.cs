using AiDocumentRedactor.Core;
using AiDocumentRedactor.Detection;
using Xunit;

namespace AiDocumentRedactor.Tests;

/// <summary>Tests for combining the main detector with GLiNER by agreement (no model needed: the spans are made by hand).</summary>
public class AgreementTests
{
    static DetectedEntity Span(int start, int length, string type = "PERSON", string source = "llm", double conf = 1.0) => new(type, start, length, conf, source);

    static RedactorOptions Options(string soloAction = "flag", double redactMin = 1.1) =>
        new() { Gliner = new GlinerOptions { Enabled = true, SoloAction = soloAction, SoloRedactMinScore = redactMin } };

    /// <summary>Where both find the same thing, the main span stands, is marked as agreed and takes the higher confidence; GLiNER's own span is not added.</summary>
    [Fact]
    public void Agreement_marks_the_main_span_and_adds_nothing()
    {
        var result = AgreementCombiner.Combine([Span(10, 12, conf: 0.7)], [Span(11, 10, source: "gliner", conf: 0.9)], Options());
        var only = Assert.Single(result);
        Assert.Equal((10, 12), (only.Start, only.Length));
        Assert.Equal("llm+gliner", only.Source);
        Assert.Equal(0.9, only.Confidence);
        Assert.False(only.Flag);
    }

    /// <summary>Something only GLiNER found is flagged for review and left in the text by default.</summary>
    [Fact]
    public void Solo_find_is_flagged_by_default()
    {
        var result = AgreementCombiner.Combine([Span(0, 5)], [Span(50, 8, "COMPANY", "gliner", 0.6)], Options());
        var solo = Assert.Single(result, s => s.Start == 50);
        Assert.True(solo.Flag);
        Assert.Equal("gliner-only", solo.Source);
        Assert.Equal("COMPANY", solo.Type);
    }

    /// <summary>A solo find can be redacted outright, or only above a confidence, or ignored, by configuration.</summary>
    [Fact]
    public void Solo_action_is_configurable()
    {
        var solo = Span(50, 8, "COMPANY", "gliner", 0.6);
        Assert.False(AgreementCombiner.Combine([], [solo], Options("redact")).Single().Flag);
        Assert.False(AgreementCombiner.Combine([], [solo], Options("flag", 0.5)).Single().Flag);
        Assert.True(AgreementCombiner.Combine([], [solo], Options("flag", 0.7)).Single().Flag);
        Assert.Empty(AgreementCombiner.Combine([], [solo], Options("ignore")));
    }

    /// <summary>Adding GLiNER never removes or changes what the main detector redacts, apart from marking it agreed.</summary>
    [Fact]
    public void Main_detector_spans_are_never_lost()
    {
        DetectedEntity[] primary = [Span(0, 5), Span(20, 6, "EMAIL", "rule"), Span(40, 9, "COMPANY", "llm-variant", 0.7)];
        var result = AgreementCombiner.Combine(primary, [Span(21, 3, "EMAIL", "gliner", 0.5)], Options());
        foreach (var p in primary)
        {
            Assert.Contains(result, r => r.Start == p.Start && r.Length == p.Length && r.Type == p.Type && !r.Flag);
        }
    }

    /// <summary>Very short solo finds (noise such as "Tel") are not raised.</summary>
    [Fact]
    public void Very_short_solo_finds_are_dropped()
    {
        Assert.Empty(AgreementCombiner.Combine([], [Span(5, 2, "SECRET", "gliner", 0.8)], Options()));
    }

    /// <summary>A switched-on GLiNER with no model file stops with a clear message; switched off, there is no detector.</summary>
    [Fact]
    public void Missing_model_is_an_error_and_off_means_none()
    {
        Assert.Null(GlinerDetector.Create(new RedactorOptions()));
        var o = new RedactorOptions { Gliner = new GlinerOptions { Enabled = true, ModelDirectory = "does-not-exist" } };
        var ex = Assert.Throws<FileNotFoundException>(() => GlinerDetector.Create(o));
        Assert.Contains("GLiNER is switched on but its model is missing", ex.Message);
    }
}

/// <summary>Tests for text that is awkward for the GLiNER tokeniser: emoji and invalid UTF-16.</summary>
public class GlinerTextTests
{
    /// <summary>An emoji stays whole as one symbol when a text is split into words, instead of being cut into two invalid halves.</summary>
    [Fact]
    public void Emoji_is_one_word()
    {
        const string text = "Hi 🚀 team, call Eleanor 👉 now";
        var words = GlinerModel.Words(text).Select(w => text[w.Start..w.End]).ToList();
        Assert.Contains("🚀", words);
        Assert.Contains("👉", words);
        Assert.Contains("Eleanor", words);
        Assert.All(words, w => Assert.Equal(w, TextSafe.Clean(w)));   // no word contains a half-emoji
    }

    /// <summary>An unpaired surrogate is replaced one for one so the text can be normalised; valid text is untouched.</summary>
    [Fact]
    public void Lone_surrogates_are_replaced_without_moving_positions()
    {
        var bad = "ab\uD83Dcd\uDE80e";
        var clean = TextSafe.Clean(bad);
        Assert.Equal(bad.Length, clean.Length);
        Assert.Equal("ab�cd�e", clean);
        _ = clean.Normalize(System.Text.NormalizationForm.FormC);   // does not throw
        Assert.Equal("plain 🚀 text", TextSafe.Clean("plain 🚀 text"));
    }
}
