namespace AiDocumentRedactor.Core;

/// <summary>How one category of sensitive data is found. A category is always found by the AI model from the prompt; some also have a fixed rule that runs before and alongside it.</summary>
/// <param name="Rule">What the fixed rule matches (null if the category has no rule, so only the model finds it).</param>
/// <param name="ModelAdds">What the AI model finds beyond the rule, or (when there is no rule) a note that it is the only way.</param>
public record CategoryDetectionInfo(string? Rule, string ModelAdds)
{
    /// <summary>True if a fixed rule helps find this category.</summary>
    public bool HasRule => Rule is not null;
}

/// <summary>Says, for each category, how it is found: by a fixed rule, by the AI model from the prompt, or both. The rule descriptions are checked against the rule detector by a test, so
/// this cannot quietly drift from what the detectors really do.</summary>
public static class CategoryDetection
{
    /// <summary>The fixed-rule coverage and the model's part for every category in <see cref="PromptBuilder.DefaultDescriptions"/>.</summary>
    public static readonly IReadOnlyDictionary<string, CategoryDetectionInfo> All = new Dictionary<string, CategoryDetectionInfo>
    {
        [EntityTypes.Person] = new(null, "Names are found only by the AI model, from the prompt."),
        [EntityTypes.Phone] = new("UK-format telephone numbers.", "Numbers in other formats."),
        [EntityTypes.Email] = new("Anything shaped like an email address.", "Anything the pattern misses."),
        [EntityTypes.Address] = new("UK postcodes.", "Full and partial addresses (street, building)."),
        [EntityTypes.IdNumber] = new("NHS numbers (check digit), National Insurance numbers, IBANs (check digit), card numbers (Luhn check), and sort codes and account numbers that follow their label.", "Passport, licence and other identifier numbers."),
        [EntityTypes.OnlineId] = new("IPv4 and IPv6 addresses.", "Usernames, social media handles and URLs identifying a person."),
        [EntityTypes.Age] = new(null, "Ages are found only by the AI model, from the prompt."),
        [EntityTypes.DateOfBirth] = new(null, "Dates of birth are found only by the AI model, from the prompt."),
        [EntityTypes.Gender] = new("Mr, Mrs, Ms, Miss, male, female, man, woman, men and women (and, if switched on, he, she, him, her, his, hers).", "Other explicit statements of gender."),
        [EntityTypes.Company] = new("Names ending in a company word such as Ltd, PLC, Group, Surgery, Trust or Council (the list is in the config).", "Company and trading names without such a word, and abbreviations."),
        [EntityTypes.CompanyId] = new(null, "Registration, VAT, tax and DUNS numbers are found only by the AI model, from the prompt."),
        [EntityTypes.Domain] = new(null, "Domain names and web addresses are found only by the AI model, from the prompt."),
        [EntityTypes.Contextual] = new(null, "Indirect identifiers are found only by the AI model, from the prompt."),
        [EntityTypes.Location] = new(null, "Places are found only by the AI model, from the prompt."),
        [EntityTypes.Secret] = new(null, "Passwords, keys and tokens are found only by the AI model, from the prompt."),
    };

    /// <summary>How a category is found (a category the table does not know is treated as model-only).</summary>
    public static CategoryDetectionInfo For(string type) => All.TryGetValue(type, out var info) ? info : new(null, "Found only by the AI model, from the prompt.");
}
