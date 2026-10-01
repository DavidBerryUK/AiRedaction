namespace AiDocumentRedactor.App.ViewModels;

/// <summary>The static parts of what is sent to the model, for the Prompt tab.</summary>
public record PromptPreview(string Model, string SystemPrompt, string UserMessageTemplate, string SchemaJson, string Settings);
