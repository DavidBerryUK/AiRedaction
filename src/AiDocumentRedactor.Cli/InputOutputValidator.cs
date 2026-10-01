using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.Cli;

/// <summary>Works out the input and output folders and refuses unsafe combinations.</summary>
public static class InputOutputValidator
{
    /// <summary>Resolves <c>--input</c> and <c>--output</c> (falling back to the config). Fails if the input folder is missing or the
    /// output folder is the input folder or inside it.</summary>
    public static (string Input, string Output, bool IsValid) ValidatePaths(string[] args, RedactorOptions options)
    {
        var input = Path.GetFullPath(CommandLine.Arg(args, "--input") ?? options.Input.Directory);
        var output = Path.GetFullPath(CommandLine.Arg(args, "--output") ?? options.Output.Directory);

        if (!Directory.Exists(input))
        {
            Console.Error.WriteLine($"Input directory not found: {input}");
            return (string.Empty, string.Empty, false);
        }

        if (output == input || output.StartsWith(input + Path.DirectorySeparatorChar))
        {
            Console.Error.WriteLine("Output directory must not be the input directory or inside it.");
            return (string.Empty, string.Empty, false);
        }

        return (input, output, true);
    }
}
