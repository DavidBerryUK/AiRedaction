using System.Text.Json;
using System.Text.Json.Nodes;

namespace AiDocumentRedactor.Core;

/// <summary>How sure we are that an edit is right: Low, Medium or High.</summary>
public enum ConfidenceLevel { Low, Medium, High }

/// <summary>Confidence in one edit, with the reason shown to the reviewer.</summary>
public record EditConfidence(ConfidenceLevel Level, int Votes, int Total, string Reason);

/// <summary>Helpers for editing the JSON config file.</summary>
public static class ConfigFile
{
    /// <summary>Updates only confidence.models in the JSON config file, leaving everything else untouched.</summary>
    public static void SaveConfidenceModels(string path, IEnumerable<string> models)
    {
        var root = JsonNode.Parse(File.ReadAllText(path), documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true })!.AsObject();
        var conf = root["confidence"] as JsonObject ?? new JsonObject();
        conf["models"] = new JsonArray(models.Select(m => (JsonNode?)JsonValue.Create(m)).ToArray());
        root["confidence"] = conf;
        File.WriteAllText(path, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }
}
