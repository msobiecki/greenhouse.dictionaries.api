using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Data.Sqlite;

namespace Greenhouse.Dictionaries.Infrastructure.Context;

internal sealed class DatabaseInitializer(IServiceScopeFactory scopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        if (dbContext.Database.IsSqlite())
        {
            await BaselineEnsureCreatedDatabaseAsync(dbContext, cancellationToken);
        }

        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task BaselineEnsureCreatedDatabaseAsync(
        DatabaseContext dbContext,
        CancellationToken cancellationToken)
    {
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            var connection = dbContext.Database.GetDbConnection();
            if (await TableExistsAsync(connection, "__EFMigrationsHistory", cancellationToken) ||
                !await TableExistsAsync(connection, "Plants", cancellationToken))
            {
                return;
            }

            var plantColumns = await GetPlantColumnsAsync(connection, cancellationToken);
            var expectedColumns = new Dictionary<string, SqliteColumnRequirements>(StringComparer.OrdinalIgnoreCase)
            {
                ["Id"] = new(["INTEGER"], IsNotNull: true, IsPrimaryKey: true),
                ["ScientificName"] = new(["TEXT"], IsNotNull: true, IsPrimaryKey: false),
                ["CommonName"] = new(["TEXT"], IsNotNull: true, IsPrimaryKey: false),
                ["Family"] = new(["TEXT"], IsNotNull: true, IsPrimaryKey: false),
                ["Category"] = new(["TEXT"], IsNotNull: true, IsPrimaryKey: false),
                ["Description"] = new(["TEXT"], IsNotNull: true, IsPrimaryKey: false),
                ["Sunlight"] = new(["TEXT"], IsNotNull: true, IsPrimaryKey: false),
                ["Watering"] = new(["TEXT"], IsNotNull: true, IsPrimaryKey: false),
                ["FlowerColor"] = new(["TEXT"], IsNotNull: false, IsPrimaryKey: false),
                ["CreatedAt"] = new(["TEXT", "datetime2(0)"], IsNotNull: true, IsPrimaryKey: false),
                ["UpdatedAt"] = new(["TEXT", "datetime2(0)"], IsNotNull: true, IsPrimaryKey: false)
            };
            var schemaMismatches = expectedColumns
                .Where(expected =>
                    !plantColumns.TryGetValue(expected.Key, out var actual) ||
                    !expected.Value.Types.Contains(actual.Type, StringComparer.OrdinalIgnoreCase) ||
                    actual.IsNotNull != expected.Value.IsNotNull ||
                    actual.IsPrimaryKey != expected.Value.IsPrimaryKey)
                .Select(expected =>
                {
                    plantColumns.TryGetValue(expected.Key, out var actual);
                    return $"{expected.Key}: expected {expected.Value}, actual {actual?.ToString() ?? "missing"}";
                })
                .Concat(plantColumns.Keys
                    .Except(expectedColumns.Keys, StringComparer.OrdinalIgnoreCase)
                    .Select(columnName => $"{columnName}: unexpected column"))
                .ToArray();
            if (schemaMismatches.Length > 0)
            {
                throw new InvalidOperationException(
                    "The existing SQLite Plants table has no migration history and does not match the initial schema. " +
                    $"Mismatches: {string.Join("; ", schemaMismatches)}. Back up and migrate the database manually.");
            }

            await using var command = connection.CreateCommand();
            command.CommandText = """
                CREATE TABLE "__EFMigrationsHistory" (
                    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
                    "ProductVersion" TEXT NOT NULL
                );
                INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                VALUES ($migrationId, $productVersion);
                """;
            command.Parameters.Add(new SqliteParameter(
                "$migrationId", GetInitialMigrationId(dbContext)));
            var version = typeof(DbContext).Assembly.GetName().Version!;
            command.Parameters.Add(new SqliteParameter(
                "$productVersion", $"{version.Major}.{version.Minor}.{version.Build}"));
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            await dbContext.Database.CloseConnectionAsync();
        }
    }

    private static string GetInitialMigrationId(DatabaseContext dbContext) =>
        dbContext.GetService<IMigrationsAssembly>().Migrations.Keys
            .OrderBy(migrationId => migrationId, StringComparer.Ordinal)
            .FirstOrDefault()
        ?? throw new InvalidOperationException(
            "Cannot baseline the existing SQLite database because no migrations are registered.");

    private static async Task<bool> TableExistsAsync(
        System.Data.Common.DbConnection connection,
        string tableName,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $tableName";
        command.Parameters.Add(new SqliteParameter("$tableName", tableName));
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0;
    }

    private static async Task<Dictionary<string, SqliteColumnDefinition>> GetPlantColumnsAsync(
        System.Data.Common.DbConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA table_info(\"Plants\")";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var columns = new Dictionary<string, SqliteColumnDefinition>(StringComparer.OrdinalIgnoreCase);
        while (await reader.ReadAsync(cancellationToken))
        {
            columns[reader.GetString(1)] = new SqliteColumnDefinition(
                reader.GetString(2),
                reader.GetInt64(3) != 0,
                reader.GetInt64(5) != 0);
        }

        return columns;
    }

    private sealed record SqliteColumnRequirements(
        string[] Types,
        bool IsNotNull,
        bool IsPrimaryKey);

    private sealed record SqliteColumnDefinition(string Type, bool IsNotNull, bool IsPrimaryKey);
}
