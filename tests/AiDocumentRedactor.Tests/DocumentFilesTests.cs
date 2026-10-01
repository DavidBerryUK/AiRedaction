using AiDocumentRedactor.Core;
using Xunit;

namespace AiDocumentRedactor.Tests;

/// <summary>Tests for the safe file lookup used by the PDF/image viewer endpoint.</summary>
public class DocumentFilesTests : IDisposable
{
    readonly string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
    readonly string outside;

    /// <summary>Creates a source folder with a PDF in a sub-folder, plus a PDF and a text file outside it.</summary>
    public DocumentFilesTests()
    {
        Directory.CreateDirectory(Path.Combine(root, "in", "sub"));
        File.WriteAllText(Path.Combine(root, "in", "sub", "a.pdf"), "x");
        File.WriteAllText(Path.Combine(root, "in", "notes.txt"), "x");
        outside = Path.Combine(root, "secret.pdf"); File.WriteAllText(outside, "x");
    }
    /// <summary>Deletes the temporary folders.</summary>
    public void Dispose() => Directory.Delete(root, true);

    string In => Path.Combine(root, "in");

    /// <summary>A viewable file inside the folder resolves, in sub-folders too.</summary>
    [Fact] public void Resolves_a_viewable_file_inside_the_folder() =>
        Assert.Equal(Path.Combine(In, "sub", "a.pdf"), DocumentFiles.Resolve(In, "sub/a.pdf"));

    /// <summary>Paths that climb out of the folder, or are absolute, are refused.</summary>
    [Theory]
    [InlineData("../secret.pdf")]
    [InlineData("sub/../../secret.pdf")]
    [InlineData("..\\secret.pdf")]
    public void Refuses_paths_that_escape_the_folder(string rel) => Assert.Null(DocumentFiles.Resolve(In, rel));

    /// <summary>An absolute path is refused even if the file exists.</summary>
    [Fact] public void Refuses_absolute_paths() => Assert.Null(DocumentFiles.Resolve(In, outside));

    /// <summary>Only viewable types (pdf, png, jpg) are served; text files and missing files are not.</summary>
    [Fact] public void Refuses_non_viewable_types_and_missing_files()
    {
        Assert.Null(DocumentFiles.Resolve(In, "notes.txt"));
        Assert.Null(DocumentFiles.Resolve(In, "missing.pdf"));
        Assert.Null(DocumentFiles.Resolve(In, ""));
    }

    /// <summary>Content types are chosen by extension, case-insensitively.</summary>
    [Fact] public void Content_type_follows_the_extension()
    {
        Assert.Equal("application/pdf", DocumentFiles.ContentType("x.PDF"));
        Assert.Equal("image/jpeg", DocumentFiles.ContentType("x.jpg"));
        Assert.Null(DocumentFiles.ContentType("x.docx"));
    }
}
