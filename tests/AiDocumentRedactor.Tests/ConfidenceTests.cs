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
}
