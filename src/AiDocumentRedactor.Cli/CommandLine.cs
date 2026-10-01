namespace AiDocumentRedactor.Cli;

/// <summary>Reads command-line flags.</summary>
public static class CommandLine
{
    /// <summary>The value after a flag, e.g. <c>--config path</c>, or null if the flag is absent.</summary>
    public static string? Arg(string[] args, string name)
    {
        var i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
    }
}
