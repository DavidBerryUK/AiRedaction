using System.Text.Json;
using System.Text.Json.Serialization;

namespace AiDocumentRedactor.Core;

/// <summary>The whole JSON config file (redactor.config.json) as a typed object. Unknown keys are rejected.</summary>
public class RedactorOptions
{
    /// <summary>Where the source documents are and which file types to pick up.</summary>
    public InputOptions Input { get; set; } = new();
    /// <summary>Where redacted files go and how they are named.</summary>
    public OutputOptions Output { get; set; } = new();
    /// <summary>Which sensitive-data categories (PERSON, EMAIL, ...) are switched on.</summary>
    public Dictionary<string, EntityOptions> Entities { get; set; } = new();
    /// <summary>How a redaction looks in the output.</summary>
    public RedactionOptions Redaction { get; set; } = new();
    /// <summary>The local language model: server, model name, settings.</summary>
    public LlmOptions Llm { get; set; } = new();
    /// <summary>Where reports and status.json are written.</summary>
    public ReportOptions Report { get; set; } = new();
    /// <summary>Terms to always redact or never redact.</summary>
    public CustomTermsOptions CustomTerms { get; set; } = new();
    /// <summary>Settings for the web UI.</summary>
    public UiOptions Ui { get; set; } = new();
    /// <summary>Which models vote when grading confidence.</summary>
    public ConfidenceOptions Confidence { get; set; } = new();
    /// <summary>How redacted PDFs are produced.</summary>
    public PdfOptions Pdf { get; set; } = new();
    /// <summary>Reading scanned PDFs and images with OCR.</summary>
    public OcrOptions Ocr { get; set; } = new();
    /// <summary>Fixed rules for predictable items (emails, phone numbers, postcodes, ID numbers, gender words), run alongside the model.</summary>
    public RulesOptions Rules { get; set; } = new();
    /// <summary>A small second detector (GLiNER) whose agreement with the main model decides what is redacted and what is flagged for review.</summary>
    public GlinerOptions Gliner { get; set; } = new();
    /// <summary>Which models the evaluation command (not the app) compares.</summary>
    public EvaluationOptions Evaluation { get; set; } = new();

    /// <summary>The mode of a category: what the config says, otherwise "flag" for LOCATION (place names are identified but left in the text,
    /// because a city alone does not identify anyone) and "redact" for everything else.</summary>
    public string ModeOf(string type) => Entities.TryGetValue(type, out var e) ? e.Mode : (type == EntityTypes.Location ? "flag" : "redact");

    /// <summary>Where reports and status.json go: report.directory if set, otherwise &lt;output&gt;/_report.</summary>
    [JsonIgnore]
    public string ReportDirectory => string.IsNullOrWhiteSpace(Report.Directory) ? Path.Combine(Output.Directory, "_report") : Report.Directory;

    /// <summary>Reads and validates the config file; throws with a clear message if it is wrong.</summary>
    public static RedactorOptions Load(string path)
    {
        var json = JsonSerializer.Deserialize<RedactorOptions>(File.ReadAllText(path), JsonOptions)
                   ?? throw new InvalidDataException("Config file is empty.");
        json.Validate();
        return json;
    }

    /// <summary>JSON settings used for config and status files: comments allowed, enums as names, unknown keys an error.</summary>
    public static readonly JsonSerializerOptions JsonOptions = new() {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow, // unknown keys are an error
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Checks the settings make sense and reports every problem at once.</summary>
    public void Validate()
    {
        var errors = new List<string>();
        if (!Redaction.PlaceholderTemplate.Contains("{type}"))
        {
            errors.Add("redaction.placeholderTemplate must contain {type}.");
        }

        if (string.IsNullOrWhiteSpace(Output.Suffix))
        {
            errors.Add("output.suffix must not be empty.");
        }

        if (Llm.Provider is not ("none" or "ollama"))
        {
            errors.Add("llm.provider must be 'none' or 'ollama'.");
        }

        if (Evaluation.Models.Any(m => string.IsNullOrWhiteSpace(m.Name)))
        {
            errors.Add("evaluation.models: every entry needs a name.");
        }

        if (errors.Count > 0)
        {
            throw new InvalidDataException(string.Join(Environment.NewLine, errors));
        }
    }
}

/// <summary>Settings for the evaluation command.</summary>
public class EvaluationOptions
{
    /// <summary>Models that may be compared, each with a switch. Only those with include = true are run (and only if installed).</summary>
    public List<EvaluationModel> Models { get; set; } = [];
}

/// <summary>One model the evaluation command could run.</summary>
public class EvaluationModel
{
    /// <summary>The Ollama model name, e.g. "phi4" or "gemma4:e4b".</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>True to run it in the next evaluation; false to keep it listed but skip it.</summary>
    public bool Include { get; set; } = true;
}

/// <summary>Input folder settings.</summary>
public class InputOptions
{
    /// <summary>Folder containing the source documents.</summary>
    public string Directory { get; set; } = "./in";
    public bool Recursive { get; set; } = true;
    /// <summary>File patterns to pick up, e.g. *.txt.</summary>
    public string[] Include { get; set; } = ["*.txt", "*.md"];
}

/// <summary>Output folder settings.</summary>
public class OutputOptions
{
    /// <summary>Folder the redacted files are written to.</summary>
    public string Directory { get; set; } = "./out";
    /// <summary>Replace an existing output file instead of skipping it.</summary>
    public bool Overwrite
    {
        get; set;
    }
    /// <summary>Keep the input's sub-folder structure in the output.</summary>
    public bool MirrorFolders { get; set; } = true;
    /// <summary>Added to the file name before the extension, e.g. letter-redacted.txt.</summary>
    public string Suffix { get; set; } = "-redacted";
    /// <summary>Remove document properties and hidden content from outputs (used by the Word/PDF writers, not built yet).</summary>
    public bool ScrubMetadata { get; set; } = true;
}

/// <summary>Settings for one sensitive-data category.</summary>
public class GlinerOptions
{
    /// <summary>Run the GLiNER detector alongside the main model and combine the two by agreement. Off by default; needs the model files.</summary>
    public bool Enabled { get; set; }
    /// <summary>Folder holding the ONNX file, tokenizer.json and gliner_config.json. Nothing is downloaded at run time.</summary>
    public string ModelDirectory { get; set; } = "models/gliner-pii-edge";
    /// <summary>The ONNX file inside the folder.</summary>
    public string OnnxFile { get; set; } = "model_quint8.onnx";
    /// <summary>Lowest confidence at which GLiNER marks something. Lower finds more and adds more noise; its scores are low, so 0.3 is already a firm mark.</summary>
    public double Threshold { get; set; } = 0.3;
    /// <summary>What to do with something only GLiNER found (the main model and rules did not): "flag" lists it for review and leaves it in the text, "redact" removes it, "ignore" drops it.</summary>
    public string SoloAction { get; set; } = "flag";
    /// <summary>With SoloAction "flag", a solo find at or above this confidence is redacted anyway (1.1 means never).</summary>
    public double SoloRedactMinScore { get; set; } = 1.1;
    /// <summary>The words GLiNER is asked for, per category. Short everyday words work best. A category left out here, or switched off, is not asked for.</summary>
    public Dictionary<string, string[]> Labels { get; set; } = new()
    {
        ["PERSON"] = ["person"], ["PHONE"] = ["phone number"], ["EMAIL"] = ["email"], ["ADDRESS"] = ["address"],
        ["ID_NUMBER"] = ["identification number"], ["ONLINE_ID"] = ["ip address"], ["AGE"] = ["age"], ["DATE_OF_BIRTH"] = ["date of birth"],
        ["GENDER"] = ["gender"], ["COMPANY"] = ["organization"], ["COMPANY_ID"] = ["company registration number"], ["DOMAIN"] = ["website"],
        ["CONTEXTUAL"] = ["job title"], ["SECRET"] = ["password"],
    };
}

public class RulesOptions
{
    /// <summary>Find predictable items with fixed rules as well as the model. Each rule runs only when its category is switched on.</summary>
    public bool Enabled { get; set; } = true;
    /// <summary>Words that end an organisation's name. One to five capitalised words followed by one of these ("Fernleigh Surgery",
    /// "Brightwater Analytics Ltd") are marked as COMPANY by rule, so the name is found even when the model leaves it out.</summary>
    public string[] OrganisationSuffixes { get; set; } =
    [
        "Ltd", "Limited", "PLC", "LLP", "LLC", "Inc", "Corp", "Corporation", "Group", "Holdings", "Partners", "Associates",
        "Surgery", "Practice", "Clinic", "Hospital", "Trust", "School", "Academy", "College", "University",
        "Council", "Bank", "Building Society", "Credit Union", "Charity", "Foundation", "Society", "Association",
    ];
}

public class EntityOptions
{
    public bool Enabled { get; set; } = true;
    /// <summary>"redact" (default) removes what is found; "flag" lists it for review but leaves it in the text.</summary>
    public string Mode { get; set; } = "redact";
    /// <summary>GENDER only: also redact gendered pronouns (he, she, him, her, his, hers). Off by default because it makes text hard to read.</summary>
    public bool RedactPronouns
    {
        get; set;
    }
    /// <summary>Overrides the category's definition in the model prompt.</summary>
    public string? Description
    {
        get; set;
    }
}

/// <summary>Redaction appearance.</summary>
public class RedactionOptions
{
    /// <summary>Replacement text; {type} becomes the category, e.g. [REDACTED:EMAIL].</summary>
    public string PlaceholderTemplate { get; set; } = "[REDACTED:{type}]";
}

/// <summary>Settings for the language model that finds sensitive data.</summary>
public class LlmOptions
{
    /// <summary>"ollama" to use the local model, "none" to detect nothing.</summary>
    public string Provider { get; set; } = "none";
    /// <summary>Address of the local model server (must be this machine unless overridden).</summary>
    public string Endpoint { get; set; } = "http://localhost:11434";
    /// <summary>Allow a model server on another machine. Off by default to keep documents local.</summary>
    public bool AllowRemoteEndpoint
    {
        get; set;
    }
    /// <summary>The model used for the next run.</summary>
    public string Model { get; set; } = "phi4";
    /// <summary>Models offered in the UI picker, in order. Each is shown as available (installed) or unavailable.</summary>
    public string[] CandidateModels { get; set; } = [];
    /// <summary>0 gives repeatable answers.</summary>
    public double Temperature
    {
        get; set;
    }
    /// <summary>Fixed random seed so repeat runs match.</summary>
    public int Seed { get; set; } = 42;
    /// <summary>Context window the model is asked to use; prompts longer than this are silently truncated.</summary>
    public int NumCtx { get; set; } = 8192;
    /// <summary>How long the model stays loaded in memory after use.</summary>
    public string KeepAlive { get; set; } = "60m";
    /// <summary>A second model asked when the main one returns nothing for text that plainly has names or identifiers in it. Empty for none.</summary>
    public string? FallbackModel
    {
        get; set;
    }
    /// <summary>Switch a thinking model's reasoning on or off. false skips the long chain of thought (much faster, and the answer is not cut short); null leaves the model's own default.</summary>
    public bool? Think { get; set; }
    /// <summary>Approximate size of each piece of text sent to the model.</summary>
    public int ChunkChars { get; set; } = 4800;
    /// <summary>How much of the end of one piece is repeated at the start of the next, so a name split across a boundary is still seen whole.</summary>
    public int ChunkOverlapChars { get; set; } = 400;
    /// <summary>Give up on a model call after this long.</summary>
    public int TimeoutSeconds { get; set; } = 300;
}

/// <summary>Confidence grading settings.</summary>
public class OcrOptions
{
    /// <summary>Read scanned PDFs and images with OCR. Off means scans are shown view-only.</summary>
    public bool Enabled { get; set; } = true;
    /// <summary>Resolution scanned PDF pages are rendered at before OCR (higher is more accurate but slower).</summary>
    public int RenderDpi { get; set; } = 300;
    /// <summary>Resolution assumed for PNG/JPG scans, used to turn pixels into page points.</summary>
    public int ImageDpi { get; set; } = 300;
    /// <summary>Below this average word confidence (0 to 1) the document is flagged for review, because the model only sees what OCR read.</summary>
    public double MinConfidence { get; set; } = 0.6;
    /// <summary>After redacting, OCR the output again and fail if redacted words are still readable.</summary>
    public bool VerifyOutput { get; set; } = true;
}

public class PdfOptions
{
    /// <summary>Resolution pages are rendered at before black boxes are drawn (higher = sharper, bigger file).</summary>
    public int RenderDpi { get; set; } = 200;
    /// <summary>Extra margin around each redacted word, in PDF points, so no edge of a letter peeks out.</summary>
    public double BoxPaddingPoints { get; set; } = 1.5;
    /// <summary>Extra room below each box, as a fraction of the word's height, to cover descenders (g, y, p, q, j), which extend below the box the PDF reports.</summary>
    public double DescenderFactor { get; set; } = 0.35;
    /// <summary>JPEG quality of the page images in the output PDF.</summary>
    public int JpegQuality { get; set; } = 85;
}

public class ConfidenceOptions
{
    /// <summary>Models whose results are compared with the primary model to grade each edit's confidence.</summary>
    public string[] Models { get; set; } = [];
    /// <summary>Highest level a category can reach, for categories the models are known to be less reliable on (e.g. CONTEXTUAL: Medium).</summary>
    public Dictionary<string, string> CategoryCaps { get; set; } = new() { ["CONTEXTUAL"] = "Medium" };
}

/// <summary>Web UI settings.</summary>
public class UiOptions
{
    /// <summary>How many per-model results to keep for each document.</summary>
    public int MaxResultsPerDocument { get; set; } = 8;
    /// <summary>Show the Prompt button (what is sent to the model and what came back). Turn off for client sessions.</summary>
    public bool PromptInspector { get; set; } = true;
    /// <summary>Warn before redacting a document with more pages than this.</summary>
    public int WarnPages { get; set; } = 50;
    /// <summary>Warn before redacting a document with more characters of text than this.</summary>
    public int WarnChars { get; set; } = 200_000;
}

/// <summary>User-supplied terms that override the model.</summary>
public class CustomTermsOptions
{
    /// <summary>Always redact these, even if the model misses them.</summary>
    public string[] Redact { get; set; } = [];
    /// <summary>Never redact these (also stated in the model prompt).</summary>
    public string[] Allow { get; set; } = [];
}

/// <summary>Report folder settings.</summary>
public class ReportOptions
{
    /// <summary>Empty means <output>/_report.</summary>
    public string Directory { get; set; } = string.Empty;
}
