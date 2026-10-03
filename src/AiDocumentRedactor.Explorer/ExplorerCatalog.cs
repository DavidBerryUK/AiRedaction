using System.Collections.Concurrent;
using System.Text.Json;
using Markdig;

namespace AiDocumentRedactor.Explorer;

/// <summary>A dataset folder found under the datasets directory.</summary>
public record DatasetListing(string Id, string Status, string CreatedAt);

/// <summary>Finds the datasets in a folder and opens them for the explorer, building each one's database the first time it is needed.</summary>
public class ExplorerCatalog(string datasetsDirectory, string? methodologyPath = null)
{
    readonly ConcurrentDictionary<string, ExplorerService> services = new();
    readonly SemaphoreSlim building = new(1, 1);

    /// <summary>The folder the datasets are in.</summary>
    public string DatasetsDirectory => datasetsDirectory;

    /// <summary>The methodology document, if the file exists.</summary>
    public string? MethodologyPath => methodologyPath is not null && File.Exists(methodologyPath) ? methodologyPath : null;

    string? methodologyHtml;
    DateTime methodologyStamp;

    /// <summary>The methodology document as HTML (null if the file is missing). Raw HTML in the file is not passed through, and links to other documents become plain text because
    /// the explorer cannot open them. Cached until the file changes.</summary>
    public string? MethodologyHtml()
    {
        var path = MethodologyPath;
        if (path is null)
        {
            return null;
        }

        var stamp = File.GetLastWriteTimeUtc(path);
        if (methodologyHtml is null || stamp != methodologyStamp)
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().DisableHtml().Build();
            var html = Markdown.ToHtml(File.ReadAllText(path), pipeline);
            methodologyHtml = System.Text.RegularExpressions.Regex.Replace(html, "<a href=\"(?!https?://|#)[^\"]*\">(.*?)</a>", "<span class=\"doclink\">$1</span>", System.Text.RegularExpressions.RegexOptions.Singleline);
            methodologyStamp = stamp;
        }

        return methodologyHtml;
    }

    /// <summary>The datasets (folders with a run.json), final ones first, then the newest.</summary>
    public async Task<List<DatasetListing>> ListAsync()
    {
        var list = new List<DatasetListing>();
        if (!Directory.Exists(datasetsDirectory))
        {
            return list;
        }

        foreach (var dir in Directory.GetDirectories(datasetsDirectory))
        {
            var run = Path.Combine(dir, "run.json");
            if (!File.Exists(run))
            {
                continue;
            }

            try
            {
                var o = JsonDocument.Parse(await File.ReadAllTextAsync(run)).RootElement;
                list.Add(new DatasetListing(Path.GetFileName(dir), o.TryGetProperty("status", out var s) ? s.GetString() ?? "?" : "?", o.TryGetProperty("createdAt", out var c) ? c.GetString() ?? string.Empty : string.Empty));
            }
            catch (JsonException)
            {
                // A run.json that cannot be read is not a dataset the explorer can show.
            }
        }

        return [.. list.OrderBy(d => d.Status == "final" ? 0 : 1).ThenByDescending(d => d.CreatedAt, StringComparer.Ordinal)];
    }

    /// <summary>The service for a dataset, built (database and all) on first use. Null if there is no such dataset. Throws if the dataset is not valid.</summary>
    public async Task<ExplorerService?> OpenAsync(string id)
    {
        if (id.Contains('/') || id.Contains('\\') || id.Contains(".."))
        {
            return null;
        }

        if (services.TryGetValue(id, out var existing))
        {
            return existing;
        }

        var dir = Path.Combine(datasetsDirectory, id);
        if (!File.Exists(Path.Combine(dir, "run.json")))
        {
            return null;
        }

        await building.WaitAsync();
        try
        {
            if (services.TryGetValue(id, out existing))
            {
                return existing;
            }

            var service = new ExplorerService(await ResultsDatabase.EnsureAsync(dir));
            services[id] = service;
            return service;
        }
        finally
        {
            building.Release();
        }
    }
}
