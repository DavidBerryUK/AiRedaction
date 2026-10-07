using AiDocumentRedactor.Core;
using AiDocumentRedactor.Detection;
using Xunit;

namespace AiDocumentRedactor.Tests;

/// <summary>Tests that what the Categories dialog says about how each category is found matches what the detectors and the prompt really do.</summary>
public class CategoryDetectionTests
{
    /// <summary>A sentence the fixed rules should find something of the category in, for each category that says it has a rule.</summary>
    static readonly Dictionary<string, string> RuleSamples = new()
    {
        [EntityTypes.Phone] = "Ring 020 7946 0958 today.",
        [EntityTypes.Email] = "Write to jo.bloggs@example.com please.",
        [EntityTypes.Address] = "She lives at LS1 4AP.",
        [EntityTypes.IdNumber] = "National Insurance QQ 12 34 56 C is on file.",
        [EntityTypes.OnlineId] = "The server is 192.168.1.10 here.",
        [EntityTypes.Gender] = "Mrs Smith is a woman.",
        [EntityTypes.Company] = "She works at Brightwater Analytics Ltd today.",
    };

    /// <summary>Every category the model is asked about has an entry saying how it is found.</summary>
    [Fact]
    public void Every_category_is_described()
    {
        Assert.All(PromptBuilder.DefaultDescriptions.Keys, t => Assert.True(CategoryDetection.All.ContainsKey(t), $"{t} has no entry"));
        Assert.All(CategoryDetection.All.Keys, t => Assert.True(PromptBuilder.DefaultDescriptions.ContainsKey(t), $"{t} is not a category"));
    }

    /// <summary>A category is said to have a fixed rule exactly when the rule detector finds something of that category, so the screen cannot claim a rule that does not exist (or hide one).</summary>
    [Fact]
    public void A_category_claims_a_rule_exactly_when_the_rule_detector_finds_it()
    {
        var options = new RedactorOptions();

        foreach (var (type, info) in CategoryDetection.All)
        {
            Assert.Equal(info.HasRule, RuleSamples.ContainsKey(type));
            if (RuleSamples.TryGetValue(type, out var sample))
            {
                Assert.Contains(RuleDetector.Find(sample, options), e => e.Type == type);
            }
        }
    }

    /// <summary>The categories with no rule never come out of the rule detector, even on text built to contain every kind of thing.</summary>
    [Fact]
    public void The_rule_detector_finds_nothing_for_a_category_without_a_rule()
    {
        var everything = string.Join(" ", RuleSamples.Values) + " Alice Smith, aged 42, born 1 May 1980, lives in Paris. Registered in England no. 01234567. See www.example.org. The password is hunter2.";

        var found = RuleDetector.Find(everything, new RedactorOptions()).Select(e => e.Type).Distinct().ToList();

        Assert.All(found, t => Assert.True(CategoryDetection.For(t).HasRule, $"{t} was found by a rule but is described as AI only"));
    }

    /// <summary>The prompt lines the dialog shows for a category are the very lines the system prompt contains, and the extra rules appear exactly when the category is on.</summary>
    [Fact]
    public void The_prompt_lines_shown_are_the_lines_in_the_system_prompt()
    {
        var options = new RedactorOptions();
        var system = PromptBuilder.System(options);

        foreach (var type in PromptBuilder.DefaultDescriptions.Keys)
        {
            Assert.Contains(PromptBuilder.CategoryLine(options, type), system);
            Assert.All(PromptBuilder.RulesFor(options, type), rule => Assert.Contains(rule, system));
        }

        options.Entities[EntityTypes.Location] = new EntityOptions { Enabled = false };
        var withoutLocation = PromptBuilder.System(options);
        Assert.DoesNotContain(PromptBuilder.CategoryLine(options, EntityTypes.Location), withoutLocation);
        Assert.DoesNotContain(PromptBuilder.RulesFor(options, EntityTypes.Location)[0], withoutLocation);
        Assert.Contains(PromptBuilder.RulesFor(options, EntityTypes.Address)[0], withoutLocation);   // the address rule changes wording when LOCATION is off
    }

    /// <summary>Only GENDER, ADDRESS and LOCATION have rules of their own in the prompt, and GENDER's pronoun rule goes when pronouns are switched on.</summary>
    [Fact]
    public void Only_three_categories_have_prompt_rules()
    {
        var options = new RedactorOptions();

        var withRules = PromptBuilder.DefaultDescriptions.Keys.Where(t => PromptBuilder.RulesFor(options, t).Count > 0).ToList();

        Assert.Equal([EntityTypes.Address, EntityTypes.Gender, EntityTypes.Location], withRules.Order());
        options.Entities[EntityTypes.Gender] = new EntityOptions { RedactPronouns = true };
        Assert.Empty(PromptBuilder.RulesFor(options, EntityTypes.Gender));
        Assert.Contains("also gendered pronouns", PromptBuilder.CategoryLine(options, EntityTypes.Gender));
    }
}
