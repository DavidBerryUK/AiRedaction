namespace AiDocumentRedactor.Core;

/// <summary>An installed model as reported by Ollama.</summary>
public record ModelInfo(string Name, string ParameterSize, string Quantization, string Family, long SizeBytes, string Digest, int ContextLength)
{
    /// <summary>First 8 characters of the model digest, for display.</summary>
    public string ShortDigest => Digest.Length > 8 ? Digest[..8] : Digest;
    /// <summary>Context window size, e.g. "16K ctx".</summary>
    public string ContextLabel => ContextLength >= 1024 ? $"{ContextLength / 1024}K ctx" : $"{ContextLength} ctx";
    /// <summary>One-line description: parameters, quantisation, context size.</summary>
    public string Summary => $"{ParameterSize} · {Quantization} · {ContextLabel}";
    /// <summary>Download size in GB or MB.</summary>
    public string SizeLabel => SizeBytes >= 1L << 30 ? $"{SizeBytes / (double)(1L << 30):0.0} GB" : $"{SizeBytes / (double)(1L << 20):0} MB";
}

/// <summary>A model offered in the picker: installed (Info present) or not yet downloaded.</summary>
public record ModelChoice(string Name, ModelInfo? Info)
{
    /// <summary>True when the model is installed.</summary>
    public bool Available => Info is not null;
    /// <summary>The command that downloads this model.</summary>
    public string PullCommand => $"ollama pull {Name}";
}

/// <summary>Lists the models installed in the local model server.</summary>
public interface IModelCatalog
{
    /// <summary>Returns every installed model that can generate text.</summary>
    Task<IReadOnlyList<ModelInfo>> ListAsync(CancellationToken ct);
}
