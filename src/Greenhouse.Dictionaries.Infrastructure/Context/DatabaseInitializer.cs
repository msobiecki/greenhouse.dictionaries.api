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
            string[] expectedColumns =
            [
                "Id", "ScientificName", "CommonName", "Family", "Category", "Description",
                "Sunlight", "Watering", "FlowerColor", "CreatedAt", "UpdatedAt"
            ];
            if (!expectedColumns.All(plantColumns.Contains))
            {
                throw new InvalidOperationException(
                    "The existing SQLite Plants table has no migration history and does not match the initial schema. Back up and migrate the database manually.");
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

    private static async Task<HashSet<string>> GetPlantColumnsAsync(
        System.Data.Common.DbConnection connection,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA table_info(\"Plants\")";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var columns = new HashSet<string>(StringComparer.Ordinal);
        while (await reader.ReadAsync(cancellationToken))
        {
            columns.Add(reader.GetString(1));
        }

        return columns;
    }
}
