using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.Cli;

/// <summary>Loads the JSON config and applies command-line overrides.</summary>
public static class ConfigurationLoader
{
    /// <summary>Reads the config file named by <c>--config</c> (default redactor.config.json) and applies <c>--model</c> (FR15).
    /// Prints the problem and returns null if the file cannot be read or is invalid.</summary>
    public static RedactorOptions? LoadConfiguration(string[] args)
    {
        var configPath = CommandLine.Arg(args, "--config") ?? "redactor.config.json";
        try
        {
            var options = RedactorOptions.Load(configPath);
            if (CommandLine.Arg(args, "--model") is { } modelOverride)
            {
                options.Llm.Model = modelOverride;
            }

            return options;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Config error: {ex.Message}");
            return null;
        }
    }
}
