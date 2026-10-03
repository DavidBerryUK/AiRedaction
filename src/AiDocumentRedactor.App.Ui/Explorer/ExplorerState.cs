using AiDocumentRedactor.Explorer;

namespace AiDocumentRedactor.App.Ui;

/// <summary>What the explorer's tabs share: the open dataset, the filters, and the setup shown in the document view. Changing a filter calls <see cref="Touch"/>, which makes each tab reload.</summary>
public class ExplorerState
{
    public ExplorerService? Service;
    public string DatasetId = string.Empty;
    public DatasetInfo? Info;
    public string? Corpus, DocType, Format, Search, Status, Setup;

    /// <summary>The kinds of setup shown: "plain" (a model on its own, or a baseline) and the three with GLiNER.</summary>
    public HashSet<string> Variants = ["plain"];
    public bool Baselines = true;

    /// <summary>Whether results made live from the explorer are counted in summaries. Off by default, so the leaderboard stays the batch evaluation.</summary>
    public bool IncludeLive;
    public List<string> Corpora = [], DocTypes = [], Formats = [], Setups = [], Types = [];

    /// <summary>The setup the document view shows, kept while moving between documents.</summary>
    public string? SelectedConfig;

    /// <summary>Counts up whenever a filter changes, so a tab can tell it needs to load again.</summary>
    public int Version { get; private set; }

    public void Touch() => Version++;

    /// <summary>The help topic currently open in the dialog (null when closed), and what the page calls to redraw when it changes.</summary>
    public string? HelpTopic { get; private set; }
    public Action? HelpChanged;

    public void ShowHelp(string? topic)
    {
        HelpTopic = topic;
        HelpChanged?.Invoke();
    }

    /// <summary>The filter for queries. <paramref name="withSetup"/> adds the single setup picked in the grid.</summary>
    public Filter ToFilter(bool withSetup = false) => new(Corpus, DocType, Format, Search, [.. Variants], withSetup && Setup is not null ? [Setup] : null, Status, IncludeLive ? null : "batch", Baselines);

    /// <summary>The link to a document's page. Each part of the path is escaped on its own so the slashes stay.</summary>
    public static string DocumentLink(string docId) => "/explorer/doc/" + string.Join('/', docId.Split('/').Select(Uri.EscapeDataString));
}

/// <summary>Number formats used across the explorer.</summary>
public static class Fmt
{
    public static string Pct(double? v) => v is null ? "–" : (v.Value * 100).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "%";
    public static string Range((double Low, double High) r) => $"{r.Low * 100:0.0}–{r.High * 100:0.0}";
    public static string Secs(double? v) => v is null ? "–" : v.Value.ToString(v.Value < 1 ? "0.00" : "0.0", System.Globalization.CultureInfo.InvariantCulture) + " s";
    public static string N(int? v) => v is null ? "–" : v.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>A colour from red (0) to green (1) that works on a light or dark page: translucent, so the text colour still reads.</summary>
    public static string Heat(double? v) => v is null ? "transparent" : $"hsl({(int)(Math.Clamp(v.Value, 0, 1) * 120)} 70% 45% / 0.38)";

    /// <summary>A colour of one hue whose strength grows with <paramref name="v"/> out of <paramref name="max"/>.</summary>
    public static string Scale(int v, int max, int hue) => v == 0 || max == 0 ? "transparent" : $"hsl({hue} 75% 50% / {Math.Round(0.12 + 0.55 * v / max, 2).ToString(System.Globalization.CultureInfo.InvariantCulture)})";
}
