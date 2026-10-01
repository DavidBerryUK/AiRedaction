using AiDocumentRedactor.Core;

namespace AiDocumentRedactor.App.ViewModels;

/// <summary>One row in the document list: the file, its status and any error.</summary>
public record DocumentItem(string FullPath, string RelativePath, long SizeBytes, DocumentStatus Status, string? ErrorMessage)
{
    /// <summary>The file name without folders.</summary>
    public string Name => Path.GetFileName(RelativePath);
    /// <summary>Upper-case extension without the dot, e.g. "PDF". Empty for no extension.</summary>
    public string Type => Path.GetExtension(RelativePath).TrimStart('.').ToUpperInvariant();
    /// <summary>True for PDF files, which the viewer shows with the browser's built-in PDF viewer.</summary>
    public bool IsPdf => Type == "PDF";
    /// <summary>True for scanned-page images the viewer can show (png, jpg).</summary>
    public bool IsImage => Type is "PNG" or "JPG" or "JPEG";
    /// <summary>True if the viewer can display this file (it may still not be redactable yet).</summary>
    public bool CanPreview => IsPdf || IsImage;

    /// <summary>The status in plain words for display and search.</summary>
    public string StatusText => Status switch
    {
        DocumentStatus.NotProcessed => "Not processed",
        DocumentStatus.Processing => "Processing…",
        DocumentStatus.Processed => "Processed",
        DocumentStatus.NeedsReview => "Needs review",
        DocumentStatus.Error => "Error",
        DocumentStatus.Cancelled => "Cancelled",
        _ => Status.ToString(),
    };
}
