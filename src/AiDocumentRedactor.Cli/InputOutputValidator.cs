using System;
using System.IO;

namespace AiDocumentRedactor.Cli
{
    public static class InputOutputValidator
    {
        public static (string input, string output, bool isValid) ValidatePaths(string[] args, RedactorOptions options)
        {
            var input = Path.GetFullPath(GetArg(args, "--input") ?? options.Input.Directory);
            var output = Path.GetFullPath(GetArg(args, "--output") ?? options.Output.Directory);

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

        private static string? GetArg(string[] args, string name)
        {
            var i = Array.IndexOf(args, name);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
        }
    }
}