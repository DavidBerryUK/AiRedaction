using System.Collections.Concurrent;
using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.Detection;

/// <summary>Finds sensitive spans with a small GLiNER model running in this process (ONNX Runtime, no network). It marks exact words in the text, never
/// writes any, and gives each find a confidence. Used as a second opinion next to the main model: see <see cref="AgreementCombiner"/>.</summary>
public sealed class GlinerDetector
{
    static readonly ConcurrentDictionary<string, Lazy<GlinerModel>> Models = new();

    readonly GlinerModel model;
    readonly GlinerOptions settings;
    readonly Dictionary<string, string> typeOf;
    readonly List<string> labels;

    GlinerDetector(GlinerModel model, RedactorOptions options)
    {
        this.model = model;
        settings = options.Gliner;
        var on = PromptBuilder.EnabledTypes(options).ToHashSet();
        typeOf = settings.Labels.Where(kv => on.Contains(kv.Key)).SelectMany(kv => kv.Value.Select(l => (Label: l, Type: kv.Key))).DistinctBy(x => x.Label).ToDictionary(x => x.Label, x => x.Type);
        labels = [.. typeOf.Keys];
    }

    /// <summary>Creates the detector if GLiNER is switched on. The model is loaded once and shared; a missing model file is an error, not a silent skip, so a
    /// run never quietly goes without the second opinion it was configured to have. Returns null when GLiNER is off.</summary>
    public static GlinerDetector? Create(RedactorOptions options)
    {
        if (!options.Gliner.Enabled)
        {
            return null;
        }

        var dir = Path.GetFullPath(options.Gliner.ModelDirectory);
        var file = Path.Combine(dir, options.Gliner.OnnxFile);
        if (!File.Exists(file))
        {
            throw new FileNotFoundException($"GLiNER is switched on but its model is missing: {file}. See the phase 2 plan for the files to place there, or set gliner.enabled to false.");
        }

        var model = Models.GetOrAdd(file, _ => new Lazy<GlinerModel>(() => new GlinerModel(dir, options.Gliner.OnnxFile))).Value;
        return new GlinerDetector(model, options);
    }

    /// <summary>The spans GLiNER marks in the text at the configured threshold.</summary>
    public IReadOnlyList<DetectedEntity> Detect(string text)
    {
        if (labels.Count == 0 || string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        var prepared = model.Run(text, labels);
        return GlinerModel.Decode(prepared, settings.Threshold)
            .Select(s => new DetectedEntity(typeOf[s.Label], s.Start, s.Length, Math.Round(s.Score, 3), "gliner")).ToList();
    }
}
