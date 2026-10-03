using AiDocumentRedactor.App.ViewModels;
using AiDocumentRedactor.Core;
using AiDocumentRedactor.Detection;
using AiDocumentRedactor.Documents;
using Xunit;

namespace AiDocumentRedactor.Tests;

/// <summary>Tests for the clean-up rules: each takes away only what it should, leaves alone what looks similar, never touches the fixed rules or a person's edits, and is off by default.</summary>
public class CleanUpRuleTests
{
    static DetectedEntity Span(string text, string find, string type, string source = "llm") => new(type, text.IndexOf(find, StringComparison.Ordinal), find.Length, 0.9, source);

    static (List<DetectedEntity> Kept, List<SuppressedItem> Suppressed) Run(string text, CleanUpOptions o, params DetectedEntity[] spans) => CleanUpRules.Apply(text, spans, o);

    /// <summary>With every rule off, nothing is taken away: the default changes nothing.</summary>
    [Fact]
    public void Everything_is_off_by_default()
    {
        var text = "Buyer pays [Company Name] XXXX-XXXX on 1 January 2023.";
        var (kept, suppressed) = Run(text, new CleanUpOptions(), Span(text, "Buyer", "COMPANY"), Span(text, "Company Name", "COMPANY"), Span(text, "XXXX-XXXX", "SECRET"), Span(text, "1 January 2023", "DATE_OF_BIRTH"));
        Assert.Equal(4, kept.Count);
        Assert.Empty(suppressed);
        Assert.False(new CleanUpOptions().AnyOn);
        Assert.False(new RedactorOptions().Rules.CleanUp.AnyOn);
    }

    /// <summary>A template placeholder is left alone whether the model's span includes the brackets or not, and so is a date template; a real bracketed word with other content is not a placeholder pattern.</summary>
    [Theory]
    [InlineData("Dear [Company Name], thanks.", "[Company Name]")]
    [InlineData("Dear [Company Name], thanks.", "Company Name")]
    [InlineData("Click {{ customer_name }} here.", "{{ customer_name }}")]
    [InlineData("Date: MM/DD/YYYY", "MM/DD/YYYY")]
    [InlineData("See <Insert Link> now.", "<Insert Link>")]
    public void Placeholders_are_suppressed(string text, string find)
    {
        var (kept, suppressed) = Run(text, new CleanUpOptions { BracketedPlaceholders = true }, Span(text, find, "COMPANY"));
        Assert.Empty(kept);
        Assert.Equal(CleanUpRules.BracketedRule, Assert.Single(suppressed).Rule);
    }

    /// <summary>A real email, name, address or number that merely sits in brackets is never taken for a placeholder: only template wording is.</summary>
    [Theory]
    [InlineData("Mail <jo@example.com> or ask.", "<jo@example.com>", "EMAIL")]
    [InlineData("Contact: [john.doe@email.com] today.", "john.doe@email.com", "EMAIL")]
    [InlineData("Signed [Howard-Lopez] on behalf.", "Howard-Lopez", "PERSON")]
    [InlineData("Seen at [394 Martha Ramp] today.", "394 Martha Ramp", "ADDRESS")]
    [InlineData("Ask [Kenneth Pearson-Hart] about it.", "Kenneth Pearson-Hart", "PERSON")]
    [InlineData("Not bracketed: Priya Natarajan.", "Priya Natarajan", "PERSON")]
    public void Placeholder_rule_leaves_real_things_alone(string text, string find, string type)
    {
        var (kept, suppressed) = Run(text, new CleanUpOptions { BracketedPlaceholders = true }, Span(text, find, type));
        Assert.Single(kept);
        Assert.Empty(suppressed);
    }

    /// <summary>Masked values, repeated zeros and a row of dashes are left alone; a partly masked card number and a real word are not.</summary>
    [Fact]
    public void Masked_values_are_suppressed_but_partial_masks_are_not()
    {
        var text = "Card XXXXXXXXXXXXXXXX, ID 0000000000, line -------, card XXXX-XXXX-XXXX-1234, key aaaaaaaa, sort code 11-11-11.";
        var (kept, suppressed) = Run(text, new CleanUpOptions { MaskedValues = true },
            Span(text, "XXXXXXXXXXXXXXXX", "SECRET"), Span(text, "0000000000", "ID_NUMBER"), Span(text, "-------", "SECRET"), Span(text, "XXXX-XXXX-XXXX-1234", "ID_NUMBER"), Span(text, "aaaaaaaa", "SECRET"), Span(text, "11-11-11", "ID_NUMBER"));
        Assert.Equal(3, suppressed.Count);
        Assert.All(suppressed, x => Assert.Equal(CleanUpRules.MaskedRule, x.Rule));
        Assert.Equal(["XXXX-XXXX-XXXX-1234", "aaaaaaaa", "11-11-11"], kept.Select(k => text.Substring(k.Start, k.Length)));   // a repeated digit such as a sort code is a real number
    }

    /// <summary>Generic roles are left alone as companies or people (with or without "the"), but a real company that merely contains one is kept, and other categories are untouched.</summary>
    [Fact]
    public void Generic_terms_are_suppressed_only_for_companies_and_people()
    {
        var text = "The Borrower owes Borrower Capital Ltd; the Company agrees. Buyer, Priya.";
        var (kept, suppressed) = Run(text, new CleanUpOptions { GenericTerms = true },
            Span(text, "The Borrower", "COMPANY"), Span(text, "Borrower Capital Ltd", "COMPANY"), Span(text, "the Company", "COMPANY"), Span(text, "Buyer", "ADDRESS"), Span(text, "Priya", "PERSON"));
        Assert.Equal(2, suppressed.Count);
        Assert.Equal(["Borrower Capital Ltd", "Buyer", "Priya"], kept.Select(k => text.Substring(k.Start, k.Length)));
    }

    /// <summary>A custom list of generic terms replaces the default.</summary>
    [Fact]
    public void Generic_term_list_is_configurable()
    {
        var text = "Acme and Buyer.";
        var (kept, suppressed) = Run(text, new CleanUpOptions { GenericTerms = true, GenericTermList = ["Acme"] }, Span(text, "Acme", "COMPANY"), Span(text, "Buyer", "COMPANY"));
        Assert.Equal("Acme", text.Substring(Assert.Single(suppressed).Start, 4));
        Assert.Single(kept);
    }

    /// <summary>A date is kept as a date of birth when birth wording is near it or the document starts with it, and left alone otherwise; other categories are untouched.</summary>
    [Fact]
    public void Dates_need_birth_wording()
    {
        var o = new CleanUpOptions { BirthDateContext = true };
        var contract = "This agreement is dated 1st day of January, 2023 and the renewal falls due on 14 March 2024.";
        Assert.Equal(2, Run(contract, o, Span(contract, "1st day of January, 2023", "DATE_OF_BIRTH"), Span(contract, "14 March 2024", "DATE_OF_BIRTH")).Suppressed.Count);

        var letter = "Mr Jones was born in a small town in Wales on 4 May 1971 and joined later.";
        Assert.Empty(Run(letter, o, Span(letter, "4 May 1971", "DATE_OF_BIRTH")).Suppressed);

        var table = "name,date_of_birth,city\n" + string.Concat(Enumerable.Repeat("filler row of text that is long enough to push things along,xx,yy\n", 3)) + "Jo,1990-01-02,Leeds";
        Assert.Empty(Run(table, o, Span(table, "1990-01-02", "DATE_OF_BIRTH")).Suppressed);   // the heading names the column

        var other = "Order placed on 4 May 2023.";
        var (kept, _) = Run(other, o, Span(other, "4 May 2023", "DATE"), Span(other, "4 May 2023", "AGE"));
        Assert.Equal(2, kept.Count);   // only a date of birth is judged
    }

    /// <summary>What the fixed rules, a custom list or a person found is never taken away, whatever it looks like; only model and GLiNER spans are.</summary>
    [Theory]
    [InlineData("rule", false)]
    [InlineData("custom-list", false)]
    [InlineData("human", false)]
    [InlineData("llm", true)]
    [InlineData("llm-variant", true)]
    [InlineData("gliner", true)]
    [InlineData("llm+gliner", true)]
    [InlineData("gliner-only", true)]
    public void Only_model_spans_are_touched(string source, bool suppressed)
    {
        var text = "Value XXXXXXXX here.";
        var (_, result) = Run(text, new CleanUpOptions { MaskedValues = true }, Span(text, "XXXXXXXX", "SECRET", source));
        Assert.Equal(suppressed, result.Count == 1);
    }

    /// <summary>Spans outside the text are kept untouched and do not crash a rule.</summary>
    [Fact]
    public void Out_of_range_spans_are_ignored()
    {
        var (kept, suppressed) = CleanUpRules.Apply("short", [new DetectedEntity("COMPANY", 3, 50, 1, "llm")], new CleanUpOptions { GenericTerms = true, MaskedValues = true });
        Assert.Single(kept);
        Assert.Empty(suppressed);
    }

    /// <summary>IPv6 addresses are found when the rule is on, in full and short form, and times, MAC addresses and ordinary text are not.</summary>
    [Fact]
    public void Ipv6_rule_finds_addresses_and_not_lookalikes()
    {
        var text = "Hosts a741:45da:c53e:0001:0002:0003:0004:0005 and 2001:db8::1 and ::1; at 10:30:45 from 00:1A:2B:3C:4D:5E ratio 3:2.";
        var found = RuleDetector.FindIpv6(text).Select(s => text.Substring(s.Start, s.Length)).ToList();
        Assert.Equal(["a741:45da:c53e:0001:0002:0003:0004:0005", "2001:db8::1", "::1"], found);

        var on = new RedactorOptions { Rules = { CleanUp = { Ipv6 = true } } };
        Assert.Contains(RuleDetector.Find(text, on), s => s.Type == EntityTypes.OnlineId && text.Substring(s.Start, s.Length) == "2001:db8::1");
        Assert.DoesNotContain(RuleDetector.Find(text, new RedactorOptions()), s => text.Substring(s.Start, s.Length) == "2001:db8::1");
    }

    /// <summary>The switches load from the config file's JSON under rules.cleanUp, all off when the block is missing.</summary>
    [Fact]
    public void Switches_load_from_the_config()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".json");
        try
        {
            File.WriteAllText(path, "{ \"rules\": { \"cleanUp\": { \"maskedValues\": true, \"genericTermList\": [\"Acme\"] } } }");
            var o = RedactorOptions.Load(path);
            Assert.True(o.Rules.CleanUp.MaskedValues);
            Assert.False(o.Rules.CleanUp.BracketedPlaceholders);
            Assert.Equal(["Acme"], o.Rules.CleanUp.GenericTermList);
            File.WriteAllText(path, "{ }");
            Assert.False(RedactorOptions.Load(path).Rules.CleanUp.AnyOn);
        }
        finally
        {
            File.Delete(path);
        }
    }
}

/// <summary>Tests for the clean-up rules in the session: each is a session-only switch that Reset puts back, and what a rule leaves out is listed so it can be redacted after all.</summary>
public class CleanUpSessionTests : IDisposable
{
    const string Text = "Dear [Company Name], Sarah Jones will call you. Card XXXXXXXXXXXXXXXX.";
    readonly string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
    string In => Path.Combine(root, "in");

    public CleanUpSessionTests()
    {
        Directory.CreateDirectory(In);
        File.WriteAllText(Path.Combine(In, "a.txt"), Text);
    }

    public void Dispose() => Directory.Delete(root, true);

    /// <summary>A detector that, like the real one, applies the clean-up rules to what it found and reports what they left out.</summary>
    sealed class RuleApplyingModel(RedactorOptions options) : IEntityDetector, IDetectorSuppression
    {
        public IReadOnlyList<SuppressedItem> LastSuppressed { get; private set; } = [];

        public Task<IReadOnlyList<DetectedEntity>> DetectAsync(string text, IProgress<RedactionProgress>? p, CancellationToken ct)
        {
            DetectedEntity At(string find, string type) => new(type, text.IndexOf(find, StringComparison.Ordinal), find.Length, 0.9, "llm");
            var (kept, suppressed) = CleanUpRules.Apply(text, [At("Company Name", "COMPANY"), At("Sarah Jones", "PERSON"), At("XXXXXXXXXXXXXXXX", "SECRET")], options.Rules.CleanUp);
            LastSuppressed = suppressed;
            return Task.FromResult<IReadOnlyList<DetectedEntity>>(kept);
        }
    }

    RedactionSession Make() => new(new RedactorOptions { Input = { Include = ["*.txt"] }, Output = { Directory = Path.Combine(root, "out") }, Llm = { Model = "phi4", CandidateModels = ["phi4"] } },
        In, [new TextDocumentReader()], [new TextDocumentWriter()], new SessionTests.FakeCatalog("phi4"), o => new RuleApplyingModel(o));

    /// <summary>Every rule starts off, switching one on counts as a change from the config, and Reset puts them all back.</summary>
    [Fact]
    public void Rules_are_off_switchable_and_reset()
    {
        var s = Make();
        Assert.Equal(5, s.CleanUpRules.Count);
        Assert.All(s.CleanUpRules, r => Assert.False(r.On));
        Assert.False(s.CategoriesChanged);
        s.SetCleanUp("maskedValues", true);
        s.SetCleanUp("ipv6", true);
        Assert.True(s.CleanUpRules.Single(r => r.Key == "maskedValues").On);
        Assert.True(s.CategoriesChanged);
        s.ResetCategories();
        Assert.All(s.CleanUpRules, r => Assert.False(r.On));
        Assert.False(s.CategoriesChanged);
        s.SetCleanUp("no-such-rule", true);   // an unknown key changes nothing
        Assert.False(s.CategoriesChanged);
    }

    /// <summary>With the rules on, the placeholder and the masked value are left in the text and listed with their rule and line; with them off, they are redacted. Redact anyway adds one.</summary>
    [Fact]
    public async Task Left_out_items_are_listed_and_can_be_redacted_after_all()
    {
        var off = Make();
        await off.LoadModelsAsync();
        off.Refresh();
        await off.SelectAsync(off.Documents.Items.Single());
        await off.RunAsync();
        Assert.Equal(3, off.ActiveResult!.EditCount);
        Assert.Empty(off.SuppressedForActive());

        var on = Make();
        await on.LoadModelsAsync();
        on.Refresh();
        on.SetCleanUp("bracketedPlaceholders", true);
        on.SetCleanUp("maskedValues", true);
        await on.SelectAsync(on.Documents.Items.Single());
        await on.RunAsync();
        Assert.Equal(1, on.ActiveResult!.EditCount);   // only Sarah Jones
        var left = on.SuppressedForActive();
        Assert.Equal(["Company Name", "XXXXXXXXXXXXXXXX"], left.Select(l => l.Text));
        Assert.All(left, l => Assert.Equal(1, l.Line));
        Assert.Contains(left, l => l.RuleLabel.Contains("placeholder", StringComparison.Ordinal));

        Assert.Equal(1, on.AddManual(left[1].Start, left[1].Length, left[1].Type, false));   // redact it after all
        Assert.Equal(2, on.ActiveResult!.EditCount);
    }
}
