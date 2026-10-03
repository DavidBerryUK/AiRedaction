using System.Globalization;
using System.Text;

namespace AiDocumentRedactor.Eval.Dataset;

/// <summary>Reads and writes the dataset's CSV files: RFC 4180, UTF-8 without a byte-order mark, LF line endings, an empty cell for "unknown".</summary>
public static class Csv
{
    /// <summary>One cell, quoted when it contains a comma, a quote or a line break.</summary>
    public static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.AsSpan().IndexOfAny(",\"\r\n") >= 0 ? "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"" : value;
    }

    /// <summary>A number in invariant culture, or an empty cell when there is none.</summary>
    public static string? Num(double? value) => value?.ToString("0.######", CultureInfo.InvariantCulture);

    /// <summary>A whole number in invariant culture, or an empty cell when there is none.</summary>
    public static string? Num(long? value) => value?.ToString(CultureInfo.InvariantCulture);

    /// <summary>A boolean as true or false, or an empty cell when there is none.</summary>
    public static string? Bool(bool? value) => value is null ? null : value.Value ? "true" : "false";

    /// <summary>Writes a header row and the rows.</summary>
    public static async Task WriteAsync(string path, IReadOnlyList<Column> columns, IEnumerable<string?[]> rows)
    {
        await using var writer = new StreamWriter(path, false, new UTF8Encoding(false)) { NewLine = "\n" };
        await writer.WriteLineAsync(string.Join(',', columns.Select(c => c.Name)));
        foreach (var row in rows)
        {
            if (row.Length != columns.Count)
            {
                throw new InvalidOperationException($"A row for {Path.GetFileName(path)} has {row.Length} cells but there are {columns.Count} columns.");
            }

            await writer.WriteLineAsync(string.Join(',', row.Select(Escape)));
        }
    }

    /// <summary>Reads a file written by <see cref="WriteAsync"/>: the column names, and each row as column name to cell.</summary>
    public static async Task<(List<string> Columns, List<Dictionary<string, string>> Rows)> ReadAsync(string path) => Parse(await File.ReadAllTextAsync(path, Encoding.UTF8));

    /// <summary>Splits CSV text into a header and rows, handling quoted cells that hold commas, quotes and line breaks.</summary>
    public static (List<string> Columns, List<Dictionary<string, string>> Rows) Parse(string text)
    {
        var records = new List<List<string>>();
        var row = new List<string>();
        var cell = new StringBuilder();
        var quoted = false;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (quoted)
            {
                if (c == '"' && i + 1 < text.Length && text[i + 1] == '"')
                {
                    cell.Append('"');
                    i++;
                }
                else if (c == '"')
                {
                    quoted = false;
                }
                else
                {
                    cell.Append(c);
                }

                continue;
            }

            switch (c)
            {
                case '"':
                    quoted = true;
                    break;
                case ',':
                    row.Add(cell.ToString());
                    cell.Clear();
                    break;
                case '\r':
                    break;
                case '\n':
                    row.Add(cell.ToString());
                    cell.Clear();
                    records.Add(row);
                    row = [];
                    break;
                default:
                    cell.Append(c);
                    break;
            }
        }

        if (quoted)
        {
            throw new InvalidDataException("A quoted cell is never closed.");
        }

        if (cell.Length > 0 || row.Count > 0)
        {
            row.Add(cell.ToString());
            records.Add(row);
        }

        if (records.Count == 0)
        {
            return ([], []);
        }

        var columns = records[0];
        var rows = new List<Dictionary<string, string>>();
        for (var r = 1; r < records.Count; r++)
        {
            if (records[r].Count != columns.Count)
            {
                throw new InvalidDataException($"Row {r + 1} has {records[r].Count} cells but the header has {columns.Count}.");
            }

            rows.Add(columns.Select((name, i) => (name, i)).ToDictionary(x => x.name, x => records[r][x.i]));
        }

        return (columns, rows);
    }
}
