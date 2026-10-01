using AiDocumentRedactor.App.ViewModels;
using AiDocumentRedactor.Core;
using Xunit;

namespace AiDocumentRedactor.Tests;

/// <summary>Tests for the document list: sorting, search, status and refresh, using a temporary folder of files.</summary>
public class DocumentListTests : IDisposable
{
    readonly string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
    readonly RedactorOptions options;
    string In => Path.Combine(root, "in");

    /// <summary>Creates a temporary input folder with five sample files.</summary>
    public DocumentListTests()
    {
        Directory.CreateDirectory(In);
        options = new RedactorOptions {
            Input = { Include = ["*.txt", "*.pdf", "*.docx", "*.md"] },
            Output = { Directory = Path.Combine(root, "out") },
            Report = { Directory = Path.Combine(root, "out", "_report") },
        };
        foreach (var n in new[] { "beta.pdf", "Alpha letter.txt", "gamma.docx", "delta.txt", "notes.md" })
        {
            File.WriteAllText(Path.Combine(In, n), "x");
        }
    }
    /// <summary>Deletes the temporary folder after each test.</summary>
    public void Dispose() => Directory.Delete(root, true);

    /// <summary>A fresh document list over the temporary folder.</summary>
    DocumentListViewModel Vm() => new(options, In);
    /// <summary>File names currently shown, in order.</summary>
    static string[] Names(DocumentListViewModel v) => v.Items.Select(i => i.Name).ToArray();

    /// <summary>Sorted by name ignoring case, and the order can be reversed.</summary>
    [Fact]
    public void Lists_documents_sorted_by_name_ignoring_case_and_can_reverse()
    {
        var vm = Vm();
        Assert.Equal(["Alpha letter.txt", "beta.pdf", "delta.txt", "gamma.docx", "notes.md"], Names(vm));
        vm.SortDescending = true;
        Assert.Equal("notes.md", vm.Items[0].Name);
    }

    /// <summary>Sorted by file type, then by name.</summary>
    [Fact]
    public void Sorts_by_type_then_name()
    {
        var vm = Vm();
        vm.SortBy = DocumentSort.Type;
        Assert.Equal(["gamma.docx", "notes.md", "beta.pdf", "Alpha letter.txt", "delta.txt"], Names(vm));
    }

    [Theory]
    [InlineData("alpha", new[] { "Alpha letter.txt" })]
    [InlineData("TXT", new[] { "Alpha letter.txt", "delta.txt" })]
    [InlineData("a txt", new[] { "Alpha letter.txt", "delta.txt" })]      // every word must match; 'a' matches both names
    [InlineData("letter txt", new[] { "Alpha letter.txt" })]
    /// <summary>Every search word must match the name, type or status, ignoring case.</summary>
    [InlineData("nomatch", new string[0])]
    public void Free_text_search_matches_name_and_type_case_insensitively(string text, string[] expected)
    {
        var vm = Vm();
        vm.SearchText = text;
        Assert.Equal(expected, Names(vm));
    }

    /// <summary>New files start as Not processed and can be found by that status.</summary>
    [Fact]
    public void New_documents_are_not_processed_and_search_finds_by_status()
    {
        var vm = Vm();
        Assert.All(vm.Items, i => Assert.Equal(DocumentStatus.NotProcessed, i.Status));
        vm.SearchText = "not processed";
        Assert.Equal(5, vm.Items.Count);
    }

    /// <summary>Processed and Error statuses (with the error message) survive reloading.</summary>
    [Fact]
    public void Status_processed_and_error_are_shown_and_persist_across_reloads()
    {
        var vm = Vm();
        vm.SetStatus(Path.Combine(In, "beta.pdf"), DocumentStatus.Processed, edits: 7, model: "phi4");
        vm.SetStatus(Path.Combine(In, "delta.txt"), DocumentStatus.Error, "Unsupported file type");

        var reloaded = Vm();   // fresh view-model reads status.json
        Assert.Equal(DocumentStatus.Processed, reloaded.Items.Single(i => i.Name == "beta.pdf").Status);
        var err = reloaded.Items.Single(i => i.Name == "delta.txt");
        Assert.Equal(DocumentStatus.Error, err.Status);
        Assert.Equal("Unsupported file type", err.ErrorMessage);

        reloaded.SearchText = "error";
        Assert.Equal(["delta.txt"], Names(reloaded));
    }

    /// <summary>A file that already has an output but no record counts as Processed.</summary>
    [Fact]
    public void Document_with_an_output_file_but_no_record_counts_as_processed()
    {
        Directory.CreateDirectory(options.Output.Directory);
        File.WriteAllText(Path.Combine(options.Output.Directory, "delta-redacted.txt"), "x");
        Assert.Equal(DocumentStatus.Processed, Vm().Items.Single(i => i.Name == "delta.txt").Status);
    }

    /// <summary>Processing is not saved to disk, and Refresh picks up newly added files.</summary>
    [Fact]
    public void Processing_status_is_in_session_only_and_refresh_picks_up_new_files()
    {
        var vm = Vm();
        vm.SetStatus(Path.Combine(In, "beta.pdf"), DocumentStatus.Processing);
        Assert.Equal(DocumentStatus.Processing, vm.Items.Single(i => i.Name == "beta.pdf").Status);
        Assert.Equal(DocumentStatus.NotProcessed, Vm().Items.Single(i => i.Name == "beta.pdf").Status); // not persisted

        File.WriteAllText(Path.Combine(In, "new.txt"), "x");
        vm.Refresh();
        Assert.Contains("new.txt", Names(vm));
    }

    /// <summary>A damaged status file is ignored instead of breaking the list.</summary>
    [Fact]
    public void Corrupt_status_file_does_not_break_the_list()
    {
        Directory.CreateDirectory(options.Report.Directory);
        File.WriteAllText(Path.Combine(options.Report.Directory, "status.json"), "{ not json");
        Assert.Equal(5, Vm().Items.Count);
    }

    /// <summary>The selected file stays selected after Refresh, and the summary reflects filtering.</summary>
    [Fact]
    public void Selection_survives_refresh_and_filtering_updates_summary()
    {
        var vm = Vm();
        vm.Selected = vm.Items.Single(i => i.Name == "gamma.docx");
        vm.Refresh();
        Assert.Equal("gamma.docx", vm.Selected?.Name);
        vm.SearchText = "pdf";
        Assert.StartsWith("1 of 5 documents", vm.Summary);
    }

    /// <summary>The type filter shows only that type, lists the types with counts, combines with search, and resets if the type disappears.</summary>
    [Fact]
    public void Type_filter_shows_one_type_and_combines_with_search()
    {
        var vm = Vm();
        Assert.Equal([("DOCX", 1), ("MD", 1), ("PDF", 1), ("TXT", 2)], vm.AvailableTypes);
        Assert.Equal(5, vm.TotalCount);

        vm.TypeFilter = "TXT";
        Assert.Equal(["Alpha letter.txt", "delta.txt"], Names(vm));
        vm.SearchText = "alpha";
        Assert.Equal(["Alpha letter.txt"], Names(vm));
        Assert.StartsWith("1 of 5 documents", vm.Summary);

        vm.SearchText = string.Empty;
        vm.TypeFilter = string.Empty;
        Assert.Equal(5, vm.Items.Count);

        vm.TypeFilter = "PDF";
        File.Delete(Path.Combine(In, "beta.pdf"));
        vm.Refresh();                       // there are no PDFs left, so the filter must not leave the list empty
        Assert.Equal(string.Empty, vm.TypeFilter);
        Assert.Equal(4, vm.Items.Count);
    }
}
