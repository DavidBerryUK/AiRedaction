using System.Net.Http.Json;
using System.Text.Json;
using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.Detection;

/// <summary>Lists models installed in the local Ollama (name, size, quantisation, context, digest).</summary>
public class OllamaModelCatalog(HttpClient http) : IModelCatalog
{
    /// <summary>Asks Ollama for its installed models and returns those that can generate text, sorted by name.</summary>
    public async Task<IReadOnlyList<ModelInfo>> ListAsync(CancellationToken ct)
    {
        var tags = await http.GetFromJsonAsync<JsonElement>("/api/tags", ct);
        var list = new List<ModelInfo>();
        foreach (var m in tags.GetProperty("models").EnumerateArray())
        {
            // Skip embedding-only models: they cannot chat.
            if (m.TryGetProperty("capabilities", out var caps) &&
                !caps.EnumerateArray().Any(c => c.GetString() == "completion"))
            {
                continue;
            }

            var d = m.GetProperty("details");
            /// <summary>Reads a text field from the model's details, or empty if absent.</summary>
            string Str(string n) => d.TryGetProperty(n, out var v) ? v.GetString() ?? string.Empty : "";
            var name = m.GetProperty("name").GetString()!;
            list.Add(new ModelInfo(name.EndsWith(":latest") ? name[..^7] : name, Str("parameter_size"), Str("quantization_level"), Str("family"),
                m.GetProperty("size").GetInt64(), m.GetProperty("digest").GetString() ?? string.Empty,
                d.TryGetProperty("context_length", out var cl) ? cl.GetInt32() : 0));
        }
        return list.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }
}
