using System;
using System.IO;

namespace AiDocumentRedactor.Cli
{
    public static class ConfigurationLoader
    {
        public static RedactorOptions LoadConfiguration(string[] args)
        {
            var configPath = GetArg(args, "--config") ?? "redactor.config.json";
            try
            {
                return RedactorOptions.Load(configPath);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Config error: {ex.Message}");
                return null;
            }
        }

        private static string? GetArg(string[] args, string name)
        {
            var i = Array.IndexOf(args, name);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
        }
    }
}