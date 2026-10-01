namespace AiDocumentRedactor.Core;

/// <summary>Safely maps a requested relative path to a file inside the input folder, for the in-app viewer.</summary>
public static class DocumentFiles
{
    static readonly Dictionary<string, string> ViewableTypes = new(StringComparer.OrdinalIgnoreCase) {
        [".pdf"] = "application/pdf",
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
    };

    /// <summary>The viewer's content type for a file extension, or null if that type is not viewable.</summary>
    public static string? ContentType(string path) => ViewableTypes.GetValueOrDefault(Path.GetExtension(path));

    /// <summary>Returns the full path if <c>relative</c> names an existing, viewable file inside <c>root</c>; otherwise null.
    /// Rejects anything that escapes the root (../, absolute paths) and any file type that is not viewable.</summary>
    public static string? Resolve(string root, string relative)
    {
        if (string.IsNullOrWhiteSpace(relative) || relative.Contains('\0') || Path.IsPathRooted(relative))
        {
            return null;
        }

        var rootFull = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var full = Path.GetFullPath(Path.Combine(rootFull, relative));
        if (!full.StartsWith(rootFull, StringComparison.Ordinal))
        {
            return null;
        }

        return File.Exists(full) && ContentType(full) is not null ? full : null;
    }
}
