using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace StarCorp.Data.Setup;

/// Roda os scripts de banco (schema e seed) numa conexao com o master, ja que o schema cria o proprio database.
/// E o equivalente, em Dapper, ao MigrateAsync do EF: deixa o banco pronto no startup.
public sealed partial class DatabaseBootstrapper(string connectionString, string scriptsDirectory)
{
    public async Task EnsureCreatedAsync(bool seed, CancellationToken ct = default)
    {
        await RunScriptAsync("schema.sql", ct);
        if (seed)
            await RunScriptAsync("seed.sql", ct);
    }

    private async Task RunScriptAsync(string fileName, CancellationToken ct)
    {
        var path = Path.Combine(scriptsDirectory, fileName);
        var script = await File.ReadAllTextAsync(path, ct);

        var masterConnectionString = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master"
        }.ConnectionString;

        await using var conn = new SqlConnection(masterConnectionString);
        await OpenWithRetryAsync(conn, ct);

        // O USE dentro do script muda o database corrente da conexao, entao os lotes seguintes rodam no destino.
        foreach (var batch in GoSeparator().Split(script))
        {
            if (string.IsNullOrWhiteSpace(batch))
                continue;

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = batch;
            cmd.CommandTimeout = 120;
            await cmd.ExecuteNonQueryAsync(ct);
        }
    }

    // O container do SQL Server pode demorar a aceitar conexoes; tenta algumas vezes antes de desistir.
    private static async Task OpenWithRetryAsync(SqlConnection conn, CancellationToken ct)
    {
        const int maxAttempts = 10;
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await conn.OpenAsync(ct);
                return;
            }
            catch (SqlException) when (attempt < maxAttempts)
            {
                await Task.Delay(TimeSpan.FromSeconds(3), ct);
            }
        }
    }

    [GeneratedRegex(@"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)]
    private static partial Regex GoSeparator();
}
