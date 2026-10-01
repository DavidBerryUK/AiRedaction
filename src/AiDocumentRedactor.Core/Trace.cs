namespace AiDocumentRedactor.Core;

/// <summary>What happened to one item the model returned: kept, or why it was dropped.</summary>
public record CallItem(string Type, string Text, string Outcome, bool Kept);

/// <summary>One request to the model for one chunk of the document, with its raw reply and what was done with it.
/// Contains document text, so it is held in memory only and never saved (FR9).</summary>
public record ModelCall(int Number, int ChunkStart, int ChunkLength, string UserMessage, string RawReply,
    IReadOnlyList<CallItem> Items, long PromptTokens, long OutputTokens, TimeSpan Elapsed, int Attempts);

/// <summary>Optional: detectors that record each model call, for the prompt inspector in the UI.</summary>
public interface IDetectorTrace
{
    /// <summary>The calls made so far, in order.</summary>
    IReadOnlyList<ModelCall> Calls
    {
        get;
    }
}
