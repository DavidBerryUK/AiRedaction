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
    /// <summary>Which models the evaluation command (not the app) compares.</summary>
    public EvaluationOptions Evaluation { get; set; } = new();

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
    /// <summary>ADDRESS only: also redact the names of cities, towns, regions and countries on their own. Off by default: a place name alone
    /// ("a meeting in Paris") does not identify anyone, so only addresses with a street, building or postcode, or a place tied to a person, count.</summary>
    public bool RedactPlaces { get; set; }
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
