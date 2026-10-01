using System.Collections.ObjectModel;
using AiDocumentRedactor.Core;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AiDocumentRedactor.App.ViewModels;

/// <summary>How the document list can be ordered.</summary>
public enum DocumentSort
{
    Name, Type
}

/// <summary>Backs the document list: scans the source directory, shows each file's status,
/// and offers sorting (name / type) and free-text search. No UI dependencies.</summary>
public partial class DocumentListViewModel : ObservableObject
{
    readonly RedactorOptions options;
    readonly string inputRoot;
    readonly StatusStore store;
    List<DocumentItem> all = [];
    readonly Dictionary<string, DocumentStatusRecord> live = new();   // in-session updates (e.g. Processing)

    /// <summary>Loads the document list for the given source folder.</summary>
    public DocumentListViewModel(RedactorOptions options, string inputRoot)
    {
        this.options = options;
        this.inputRoot = Path.GetFullPath(inputRoot);
        store = new StatusStore(options.ReportDirectory);
        Refresh();
    }

    /// <summary>The documents currently shown, after search and sorting.</summary>
    public ObservableCollection<DocumentItem> Items { get; } = [];

    /// <summary>Free-text search box contents.</summary>
    [ObservableProperty] string searchText = "";
    /// <summary>Sort field.</summary>
    [ObservableProperty] DocumentSort sortBy = DocumentSort.Name;
    /// <summary>Reverse the sort order.</summary>
    [ObservableProperty] bool sortDescending;
    /// <summary>The document chosen in the list.</summary>
    [ObservableProperty] DocumentItem? selected;
    /// <summary>Show only documents of this type (upper-case extension such as "PDF"); empty shows all types.</summary>
    [ObservableProperty] string typeFilter = "";
    /// <summary>The types present in the source folder with how many documents each has, for the type filter drop-down.</summary>
    public IReadOnlyList<(string Type, int Count)> AvailableTypes => all.GroupBy(i => i.Type).OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase).Select(g => (g.Key, g.Count())).ToList();
    /// <summary>Total number of documents in the folder, ignoring search and filter.</summary>
    public int TotalCount => all.Count;

    /// <summary>Counts line, e.g. "12 of 27 documents · 3 processed · 1 errors".</summary>
    [ObservableProperty] string summary = "";

    /// <summary>Re-applies search and sort when the search text changes.</summary>
    partial void OnSearchTextChanged(string value) => Apply();
    /// <summary>Re-applies the filters when the type filter changes.</summary>
    partial void OnTypeFilterChanged(string value) => Apply();
    /// <summary>Re-applies search and sort when the sort field changes.</summary>
    partial void OnSortByChanged(DocumentSort value) => Apply();
    /// <summary>Re-applies search and sort when the direction changes.</summary>
    partial void OnSortDescendingChanged(bool value) => Apply();

    /// <summary>Rescans the source directory (FR35) and reloads saved statuses.</summary>
    public void Refresh()
    {
        var saved = store.Load();
        var files = options.Input.Include
            .SelectMany(p => Directory.Exists(inputRoot)
                ? Directory.EnumerateFiles(inputRoot, p, options.Input.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                : [])
            .Distinct();
        var selectedPath = Selected?.FullPath;
        all = files.Select(f => Build(f, saved)).ToList();
        if (TypeFilter != "" && all.All(i => i.Type != TypeFilter))
        {
            TypeFilter = "";   // that type no longer exists in the folder
        }

        Apply();
        Selected = selectedPath is null ? null : Items.FirstOrDefault(i => i.FullPath == selectedPath);
    }

    /// <summary>Builds one list row, working out its status: in-session, then saved, then "has an output file", else not processed.</summary>
    DocumentItem Build(string fullPath, Dictionary<string, DocumentStatusRecord> saved)
    {
        var rel = Path.GetRelativePath(inputRoot, fullPath);
        var key = StatusStore.Key(rel);
        var size = new FileInfo(fullPath).Length;
        if (live.TryGetValue(key, out var l))
        {
            return new(fullPath, rel, size, l.Status, l.Error);
        }

        if (saved.TryGetValue(key, out var s))
        {
            return new(fullPath, rel, size, s.Status, s.Error);
        }
        // No record: processed outside the tool or before status tracking? An output file means it was processed.
        var output = Path.Combine(options.Output.Directory, options.Output.MirrorFolders ? Path.GetDirectoryName(rel) ?? "" : "",
            Path.GetFileNameWithoutExtension(rel) + options.Output.Suffix + Path.GetExtension(rel));
        return new(fullPath, rel, size, File.Exists(output) ? DocumentStatus.Processed : DocumentStatus.NotProcessed, null);
    }

    /// <summary>Updates a document's status immediately (used while a run is in progress or after it ends).</summary>
    public void SetStatus(string fullPath, DocumentStatus status, string? error = null, int edits = 0, string? model = null)
    {
        var rel = Path.GetRelativePath(inputRoot, fullPath);
        var rec = new DocumentStatusRecord(status, error, edits, DateTime.UtcNow, model);
        if (status == DocumentStatus.Processing)
        {
            live[StatusStore.Key(rel)] = rec;
        }
        else
        {
            live.Remove(StatusStore.Key(rel));
            store.Set(rel, rec);
        }
        Refresh();
    }

    /// <summary>Filters by the search words, sorts, and refreshes the visible list and summary.</summary>
    void Apply()
    {
        IEnumerable<DocumentItem> q = TypeFilter == "" ? all : all.Where(i => i.Type == TypeFilter);
        var terms = SearchText.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        // Free text: every word must match the name, folder, type or status (case-insensitive)
        foreach (var t in terms)
        {
            q = q.Where(i => i.RelativePath.Contains(t, StringComparison.OrdinalIgnoreCase)
                          || i.Type.Contains(t, StringComparison.OrdinalIgnoreCase)
                          || i.StatusText.Contains(t, StringComparison.OrdinalIgnoreCase));
        }

        var cmp = StringComparer.OrdinalIgnoreCase;
        var sorted = SortBy == DocumentSort.Type
            ? (SortDescending ? q.OrderByDescending(i => i.Type, cmp).ThenByDescending(i => i.Name, cmp)
                              : q.OrderBy(i => i.Type, cmp).ThenBy(i => i.Name, cmp))
            : (SortDescending ? q.OrderByDescending(i => i.Name, cmp) : q.OrderBy(i => i.Name, cmp));

        var list = sorted.ToList();
        Items.Clear();
        foreach (var i in list)
        {
            Items.Add(i);
        }

        Summary = $"{list.Count} of {all.Count} documents · {all.Count(i => i.Status is DocumentStatus.Processed or DocumentStatus.NeedsReview)} processed · {all.Count(i => i.Status == DocumentStatus.Error)} errors";
    }
}
