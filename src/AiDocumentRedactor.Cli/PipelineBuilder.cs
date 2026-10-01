using System;
using AiDocumentRedactor.Core;
using AiDocumentRedactor.Detection;
using AiDocumentRedactor.Documents;
using AiDocumentRedactor.Ocr;

namespace AiDocumentRedactor.Cli
{
    public static class PipelineBuilder
    {
        public static (IEntityDetector detector, RedactionPipeline pipeline) BuildPipeline(RedactorOptions options)
        {
            // Choose the detector: the local Ollama model if configured (checking it is reachable), else one that finds nothing.
            IEntityDetector detector = new NoOpDetector();

            if (options.Llm.Provider == "ollama")
            {
                try
                {
                    var od = new OllamaDetector(OllamaDetector.CreateClient(options.Llm), options);
                    od.CheckAvailableAsync(CancellationToken.None).Wait(); // Synchronous for CLI use
                    detector = od;
                    Console.WriteLine($"Model: {options.Llm.Model} at {options.Llm.Endpoint}");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    return (null, null);
                }
            }

            // Build the pipeline (read -> detect -> redact -> write)
            using var ocr = options.Ocr.Enabled ? new RapidOcrEngine() : null;
            var pipeline = new RedactionPipeline(
                DocumentFormats.Readers(options, ocr),
                DocumentFormats.Writers(options, ocr),
                detector,
                options);

            return (detector, pipeline);
        }
    }
}