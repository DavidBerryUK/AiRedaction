using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AiDocumentRedactor.Explorer.Dataset;
using Microsoft.Data.Sqlite;

namespace AiDocumentRedactor.Explorer;

/// <summary>Builds the SQLite database the explorer reads from a dataset folder. The database is a cache: it is made from the CSV files and the text file, rebuilt when they
/// change, and can be deleted at any time. The dataset files stay the record.</summary>
public static class ResultsDatabase
{
    public const string DatabaseFile = "explorer.sqlite";

    /// <summary>The path of the database for a dataset folder, built (or rebuilt, when the dataset's files changed) if needed.</summary>
    public static async Task<string> EnsureAsync(string datasetDir)
    {
        var path = Path.Combine(datasetDir, DatabaseFile);
        var signature = Signature(datasetDir);
        if (File.Exists(path) && await StoredSignatureAsync(path) == signature)
        {
            return path;
        }

        await ImportAsync(datasetDir, path, signature);
        return path;
    }

    /// <summary>A fingerprint of the dataset's files (names, sizes and times), used to tell when the database is out of date.</summary>
    static string Signature(string datasetDir)
    {
        var sb = new StringBuilder("v1;");
        foreach (var name in new[] { Schema.RunFile, Schema.TextFile }.Concat(Schema.Files.Keys))
        {
            var info = new FileInfo(Path.Combine(datasetDir, name));
            sb.Append(name).Append('|').Append(info.Exists ? info.Length : -1).Append('|').Append(info.Exists ? info.LastWriteTimeUtc.Ticks : 0).Append(';');
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()))).ToLowerInvariant();
    }

    static async Task<string?> StoredSignatureAsync(string path)
    {
        try
        {
            await using var c = new SqliteConnection($"Data Source={path};Mode=ReadOnly;Pooling=False");
            await c.OpenAsync();
            await using var cmd = c.CreateCommand();
            cmd.CommandText = "SELECT value FROM meta WHERE key = 'signature'";
            return await cmd.ExecuteScalarAsync() as string;
        }
        catch (SqliteException)
        {
            return null;
        }
    }

    /// <summary>Reads the dataset and writes a fresh database at <paramref name="dbPath"/>.</summary>
    public static async Task ImportAsync(string datasetDir, string dbPath, string? signature = null)
    {
        var problems = await DatasetValidator.ValidateAsync(datasetDir);
        if (problems.Count > 0)
        {
            throw new InvalidDataException($"The dataset in {datasetDir} is not valid: {problems[0]}{(problems.Count > 1 ? $" (and {problems.Count - 1} more)" : string.Empty)}");
        }

        var temp = dbPath + ".tmp";
        File.Delete(temp);
        await using (var c = new SqliteConnection($"Data Source={temp};Pooling=False"))
        {
            await c.OpenAsync();
            await Exec(c, "PRAGMA journal_mode = OFF; PRAGMA synchronous = OFF;");
            await using var tx = (SqliteTransaction)await c.BeginTransactionAsync();
            foreach (var (file, columns) in Schema.Files)
            {
                var table = Path.GetFileNameWithoutExtension(file);
                await Exec(c, $"CREATE TABLE {table} ({string.Join(", ", columns.Select(col => $"\"{col.Name}\" {SqlType(col.Kind)}"))})", tx);
                var (names, rows) = await Csv.ReadAsync(Path.Combine(datasetDir, file));
                await using var insert = c.CreateCommand();
                insert.Transaction = tx;
                insert.CommandText = $"INSERT INTO {table} VALUES ({string.Join(", ", columns.Select((_, i) => "@p" + i))})";
                var parameters = columns.Select((_, i) => insert.Parameters.Add("@p" + i, SqliteType.Text)).ToList();
                foreach (var row in rows)
                {
                    for (var i = 0; i < columns.Length; i++)
                    {
                        parameters[i].Value = Cell(columns[i], row.GetValueOrDefault(columns[i].Name, string.Empty));
                    }

                    await insert.ExecuteNonQueryAsync();
                }
            }

            await Exec(c, "CREATE TABLE texts (doc_id TEXT PRIMARY KEY, text TEXT NOT NULL)", tx);
            await using (var insert = c.CreateCommand())
            {
                insert.Transaction = tx;
                insert.CommandText = "INSERT INTO texts VALUES (@id, @text)";
                var id = insert.Parameters.Add("@id", SqliteType.Text);
                var text = insert.Parameters.Add("@text", SqliteType.Text);
                foreach (var line in await File.ReadAllLinesAsync(Path.Combine(datasetDir, Schema.TextFile)))
                {
                    if (line.Length == 0)
                    {
                        continue;
                    }

                    var o = JsonDocument.Parse(line).RootElement;
                    id.Value = o.GetProperty("doc_id").GetString();
                    text.Value = o.GetProperty("text").GetString();
                    await insert.ExecuteNonQueryAsync();
                }
            }

            await Exec(c, "CREATE TABLE meta (key TEXT PRIMARY KEY, value TEXT)", tx);
            await using (var insert = c.CreateCommand())
            {
                insert.Transaction = tx;
                insert.CommandText = "INSERT INTO meta VALUES (@k, @v)";
                var k = insert.Parameters.Add("@k", SqliteType.Text);
                var v = insert.Parameters.Add("@v", SqliteType.Text);
                foreach (var (key, value) in new[] { ("run", await File.ReadAllTextAsync(Path.Combine(datasetDir, Schema.RunFile))), ("signature", signature ?? Signature(datasetDir)) })
                {
                    k.Value = key;
                    v.Value = value;
                    await insert.ExecuteNonQueryAsync();
                }
            }

            foreach (var index in new[]
            {
                "CREATE UNIQUE INDEX ix_documents_id ON documents (doc_id)",
                "CREATE INDEX ix_entities_key ON entities (key_id)",
                "CREATE UNIQUE INDEX ix_results_id ON results (result_id)",
                "CREATE INDEX ix_results_doc ON results (doc_id)",
                "CREATE INDEX ix_results_config ON results (config, variant, model)",
                "CREATE INDEX ix_spans_result ON spans (result_id)",
                "CREATE INDEX ix_outcomes_result ON outcomes (result_id)",
                "CREATE INDEX ix_outcomes_doc_kind ON outcomes (doc_id, kind)",
                "CREATE INDEX ix_outcomes_kind_type ON outcomes (kind, type)",
            })
            {
                await Exec(c, index, tx);
            }

            await tx.CommitAsync();
            await Exec(c, "ANALYZE");
        }

        SqliteConnection.ClearAllPools();
        File.Delete(dbPath);
        File.Move(temp, dbPath);
    }

    static string SqlType(ColumnKind kind) => kind switch
    {
        ColumnKind.Int or ColumnKind.Bool => "INTEGER",
        ColumnKind.Number => "REAL",
        _ => "TEXT",
    };

    /// <summary>A cell as a database value: text stays text (empty when empty), and an empty number or boolean is null.</summary>
    static object Cell(Column column, string value)
    {
        if (column.Kind == ColumnKind.Text)
        {
            return value;
        }

        if (value.Length == 0)
        {
            return DBNull.Value;
        }

        return column.Kind switch
        {
            ColumnKind.Int => long.Parse(value, CultureInfo.InvariantCulture),
            ColumnKind.Number => double.Parse(value, CultureInfo.InvariantCulture),
            _ => value == "true" ? 1L : 0L,
        };
    }

    static async Task Exec(SqliteConnection c, string sql, SqliteTransaction? tx = null)
    {
        await using var cmd = c.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        await cmd.ExecuteNonQueryAsync();
    }
}
