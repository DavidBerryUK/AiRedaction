using AiDocumentRedactor.App.ViewModels;
using AiDocumentRedactor.Core;
using Xunit;

namespace AiDocumentRedactor.Tests;

/// <summary>Tests for grading edits by agreement between models, and for saving the chosen models to the config file.</summary>
public class ConfidenceTests
{
    const string Text = "Sarah Jones met Acme Ltd on Monday and Bob Smith left.";
    /// <summary>Builds a model result that found the given spans in the sample text.</summary>
    static ModelResult R(string model, params (int s, int l, string t)[] spans) =>
        new(Guid.NewGuid(), model, null,
            Redactor.Apply(Text, spans.Select(x => new DetectedEntity(x.t, x.s, x.l, 1, "t")), "[REDACTED:{type}]"),
            TimeSpan.FromSeconds(1), 0, 0, 0, DateTime.UtcNow);

    static readonly (int, int, string) Sarah = (0, 11, "PERSON"), Acme = (16, 8, "COMPANY"), Bob = (40, 9, "PERSON");

    /// <summary>With one model nothing can be compared, so all edits are Medium and the reason says why.</summary>
    [Fact]
    public void Single_model_cannot_be_graded_so_everything_is_medium_with_an_explanation()
    {
        var a = R("phi4", Sarah, Acme);
        var g = ConfidenceGrader.Grade(a, [a]);
        Assert.All(g.Values, c => Assert.Equal(ConfidenceLevel.Medium, c.Level));
        Assert.Contains("Only one model", g.Values.First().Reason);
    }

    /// <summary>Found by all models = High; found by fewer than half = Low; otherwise Medium.</summary>
    [Fact]
    public void Edit_found_by_every_model_is_high_and_by_fewer_than_half_is_low()
    {
        var a = R("a", Sarah, Acme, Bob); var b = R("b", Sarah, Acme); var c = R("c", Sarah);
        var g = ConfidenceGrader.Grade(a, [a, b, c]);
        var byStart = a.Result.Edits.ToDictionary(e => e.OriginalStart, e => g[e.Id]);
        Assert.Equal(ConfidenceLevel.High, byStart[0].Level);      // all 3
        Assert.Equal(ConfidenceLevel.Medium, byStart[16].Level);   // 2 of 3
        Assert.Equal(ConfidenceLevel.Low, byStart[40].Level);      // 1 of 3
        Assert.Contains("missed by b, c", byStart[40].Reason);
    }

    /// <summary>Two models marking slightly different spans over the same text still count as agreeing.</summary>
    [Fact]
    public void Overlapping_but_not_identical_spans_count_as_agreement()
    {
        var a = R("a", Sarah); var b = R("b", (0, 5, "PERSON"));   // "Sarah" only
        Assert.Equal(ConfidenceLevel.High, ConfidenceGrader.Grade(a, [a, b]).Values.Single().Level);
    }

    /// <summary>With two models, one disagreement gives Medium, not Low.</summary>
    [Fact]
    public void Two_models_one_disagreement_is_medium_not_low()
    {
        var a = R("a", Sarah, Acme); var b = R("b", Sarah);
        var g = ConfidenceGrader.Grade(a, [a, b]);
        Assert.Equal(ConfidenceLevel.Medium, g[a.Result.Edits.Single(e => e.OriginalStart == 16).Id].Level);
    }

    /// <summary>Saving the voting models changes only that key; other settings are untouched.</summary>
    [Fact]
    public void Saving_confidence_models_changes_only_that_key_in_the_config_file()
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, "{ \"llm\": { \"model\": \"phi4\", \"seed\": 7 }, \"output\": { \"suffix\": \"-x\" } }");
        ConfigFile.SaveConfidenceModels(path, ["gemma4:e4b", "phi4"]);
        var o = RedactorOptions.Load(path);
        Assert.Equal(["gemma4:e4b", "phi4"], o.Confidence.Models);
        Assert.Equal(7, o.Llm.Seed); Assert.Equal("-x", o.Output.Suffix);
    }

    /// <summary>An edit that is only a shorter form of something longer is graded one step lower, and a person's own edit is always High.</summary>
    [Fact]
    public void Variant_only_edits_are_lowered_and_human_edits_are_high()
    {
        ModelResult WithSource(string src) => new(Guid.NewGuid(), "a", null,
            Redactor.Apply(Text, [new DetectedEntity("PERSON", 0, 5, 1, src)], "[REDACTED:{type}]"), TimeSpan.FromSeconds(1), 0, 0, 0, DateTime.UtcNow);
        var llm = WithSource("llm"); var variant = WithSource("llm-variant"); var human = WithSource("human");
        Assert.Equal(ConfidenceLevel.Medium, ConfidenceGrader.Grade(llm, [llm]).Values.Single().Level);
        var v = ConfidenceGrader.Grade(variant, [variant]).Values.Single();
        Assert.Equal(ConfidenceLevel.Low, v.Level);
        Assert.Contains("shorter form", v.Reason);
        Assert.Equal(ConfidenceLevel.High, ConfidenceGrader.Grade(human, [human]).Values.Single().Level);
    }

    /// <summary>Words OCR was unsure of cap the edit at Low, and a category ceiling stops High going above it.</summary>
    [Fact]
    public void Low_ocr_confidence_and_category_caps_limit_the_grade()
    {
        var a = R("a", Sarah); var b = R("b", Sarah);                     // both models agree: would be High
        var plain = ConfidenceGrader.Grade(a, [a, b]).Values.Single();
        Assert.Equal(ConfidenceLevel.High, plain.Level);

        var lowOcr = new ConfidenceContext(_ => 0.42, 0.6, new Dictionary<string, string>());
        var g = ConfidenceGrader.Grade(a, [a, b], lowOcr).Values.Single();
        Assert.Equal(ConfidenceLevel.Low, g.Level);
        Assert.Contains("42%", g.Reason);

        var goodOcr = new ConfidenceContext(_ => 0.95, 0.6, new Dictionary<string, string> { ["PERSON"] = "Medium" });
        var capped = ConfidenceGrader.Grade(a, [a, b], goodOcr).Values.Single();
        Assert.Equal(ConfidenceLevel.Medium, capped.Level);
        Assert.Contains("limited to Medium", capped.Reason);
    }
}
