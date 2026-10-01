using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.Detection;

/// <summary>Phase 0 placeholder: detects nothing. Replaced by the Ollama detector in Phase 1.</summary>
public class NoOpDetector : IEntityDetector
{
    /// <summary>Returns no detections.</summary>
    public Task<IReadOnlyList<DetectedEntity>> DetectAsync(string text, IProgress<RedactionProgress>? progress, CancellationToken ct)
        => Task.FromResult<IReadOnlyList<DetectedEntity>>([]);
}
