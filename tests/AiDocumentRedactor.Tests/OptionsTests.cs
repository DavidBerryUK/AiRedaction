using AiDocumentRedactor.Core;
using Xunit;

namespace AiDocumentRedactor.Tests;

/// <summary>Tests for loading the JSON config.</summary>
public class OptionsTests
{
    /// <summary>A misspelt or unknown setting is an error, not silently ignored.</summary>
    [Fact]
    public void Unknown_config_keys_are_rejected()
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, "{ \"nonsense\": 1 }");
        Assert.ThrowsAny<Exception>(() => RedactorOptions.Load(path));
    }

    /// <summary>The shipped redactor.config.json loads with the expected defaults.</summary>
    [Fact]
    public void Default_config_file_loads()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "redactor.config.json")))
        {
            dir = Path.GetDirectoryName(dir);
        }

        var o = RedactorOptions.Load(Path.Combine(dir!, "redactor.config.json"));
        Assert.Equal("[REDACTED:{type}]", o.Redaction.PlaceholderTemplate);
        Assert.Equal("-redacted", o.Output.Suffix);
    }
}
