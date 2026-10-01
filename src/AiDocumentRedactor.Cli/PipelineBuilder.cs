using AiDocumentRedactor.Core;
using AiDocumentRedactor.Detection;
using AiDocumentRedactor.Documents;

namespace AiDocumentRedactor.Cli;

/// <summary>Builds the read, detect, redact, write pipeline from the config.</summary>
public static class PipelineBuilder
{
    /// <summary>Creates the detector (the local Ollama model if configured and reachable, otherwise one that finds nothing) and the pipeline.
    /// The OCR engine is owned by the caller and must stay alive for as long as the pipeline is used. Returns null after printing the problem
    /// if the model is not available.</summary>
    public static async Task<(IEntityDetector Detector, RedactionPipeline Pipeline)?> BuildPipelineAsync(RedactorOptions options, IOcrEngine? ocr)
    {
        IEntityDetector detector = new NoOpDetector();
        if (options.Llm.Provider == "ollama")
        {
            try
            {
                var ollama = new OllamaDetector(OllamaDetector.CreateClient(options.Llm), options);
                await ollama.CheckAvailableAsync(CancellationToken.None);
                detector = ollama;
                Console.WriteLine($"Model: {options.Llm.Model} at {options.Llm.Endpoint}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return null;
            }
        }

        var pipeline = new RedactionPipeline(DocumentFormats.Readers(options, ocr), DocumentFormats.Writers(options, ocr), detector, options);
        return (detector, pipeline);
    }
}
