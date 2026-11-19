using Greenhouse.Dictionaries.Application.Abstractions;
using Greenhouse.Dictionaries.Domain.Entities;
using Greenhouse.Dictionaries.Infrastructure;
using Greenhouse.Dictionaries.Infrastructure.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Greenhouse.Dictionaries.Api.Tests.Infrastructure.Context;

public sealed class SqliteDatabaseTests
{
    [Fact]
    public async Task GetAllAsync_SearchesPolishTextWithoutCaseSensitivity()
    {
        var databasePath = CreateDatabasePath();
        try
        {
            await using var provider = CreateServiceProvider(databasePath);
            await InitializeAsync(provider);

            var context = provider.GetRequiredService<DatabaseContext>();
            context.Plants.Add(CreatePlant("Żółć"));
            await context.SaveChangesAsync();

            var repository = provider.GetRequiredService<IPlantReadRepository>();
            var result = await repository.GetAllAsync(
                "ŻÓŁĆ", new PlantSearchFilters(Category: "FLOWERING"));

            Assert.Equal("Żółć", Assert.Single(result.Items).CommonName);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task Startup_MigratesNewSqliteDatabaseAndGeneratesPlantId()
    {
        var databasePath = CreateDatabasePath();
        try
        {
            await using var provider = CreateServiceProvider(databasePath);
            await InitializeAsync(provider);

            var context = provider.GetRequiredService<DatabaseContext>();
            var plant = CreatePlant("Rose");
            context.Plants.Add(plant);
            await context.SaveChangesAsync();

            Assert.True(plant.Id > 0);
            await AssertAllMigrationsAppliedAsync(context);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public void SqliteMigrationGeneratesRollbackScript()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        using var context = new DatabaseContext(options);
        var latestMigrationId = context.Database.GetMigrations().Last();

        var rollbackScript = context.GetService<IMigrator>()
            .GenerateScript(latestMigrationId, Migration.InitialDatabase);

        Assert.Contains("DROP TABLE \"Plants\"", rollbackScript, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Startup_MigratesExistingInitialSchemaAndPreservesPlantData()
    {
        var databasePath = CreateDatabasePath();
        try
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlite($"Data Source={databasePath}")
                .Options;
            await using (var existingContext = new DatabaseContext(options))
            {
                var initialMigrationId = existingContext.Database.GetMigrations().First();
                await existingContext.GetService<IMigrator>().MigrateAsync(initialMigrationId);
                await existingContext.Database.ExecuteSqlRawAsync("""
                    INSERT INTO "Plants" (
                        "ScientificName", "CommonName", "Family", "Category", "Description",
                        "Sunlight", "Watering", "FlowerColor", "CreatedAt", "UpdatedAt")
                    VALUES (
                        'Rosa canina', 'Existing rose', 'Rosaceae', 'flower', 'A flowering plant.',
                        'Full sun', 'Regular', NULL, '2024-01-02T03:04:05.0000000Z', '2024-01-02T03:04:05.0000000Z');
                    """);
            }

            await using var provider = CreateServiceProvider(databasePath);
            await InitializeAsync(provider);

            var context = provider.GetRequiredService<DatabaseContext>();
            var migratedPlant = await context.Plants.SingleAsync();
            Assert.Equal("Existing rose", migratedPlant.CommonName);
            Assert.Equal(PlantType.Flowering, migratedPlant.Category);
            Assert.Equal(migratedPlant.Description, migratedPlant.IdentifyingCharacteristics);
            await AssertAllMigrationsAppliedAsync(context);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task Startup_MigratesLegacyToxicityValuesToAssessmentsAndSources()
    {
        var databasePath = CreateDatabasePath();
        try
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlite($"Data Source={databasePath}")
                .Options;
            await using (var existingContext = new DatabaseContext(options))
            {
                var previousModelMigration = existingContext.Database.GetMigrations()
                    .Single(migration => migration.EndsWith("_AddComprehensivePlantDictionary", StringComparison.Ordinal));
                await existingContext.GetService<IMigrator>().MigrateAsync(previousModelMigration);
                await existingContext.Database.ExecuteSqlRawAsync("""
                    INSERT INTO "Plants" (
                        "ScientificName", "CommonName", "Category", "Family", "Description",
                        "IdentifyingCharacteristics", "CreatedAt", "UpdatedAt", "ToxicToCats", "ToxicToDogs",
                        "ToxicToHumans", "ToxicityDetails", "ToxicitySource")
                    VALUES (
                        'Monstera deliciosa', 'Monstera', 'Foliage', 'Araceae', 'A climbing plant.',
                        'Split leaves.', '2024-01-02T03:04:05.0000000Z', '2024-01-02T03:04:05.0000000Z',
                        1, 0, NULL, 'Irritating sap.', 'https://example.test/monstera-safety');
                    """);
            }

            await using var provider = CreateServiceProvider(databasePath);
            await InitializeAsync(provider);

            var context = provider.GetRequiredService<DatabaseContext>();
            var plant = await context.Plants
                .Include(candidate => candidate.ToxicityAssessments)
                    .ThenInclude(assessment => assessment.Sources)
                .SingleAsync();
            var catAssessment = Assert.Single(
                plant.ToxicityAssessments, assessment => assessment.TargetCode == "cat");
            var dogAssessment = Assert.Single(
                plant.ToxicityAssessments, assessment => assessment.TargetCode == "dog");

            Assert.Equal(ToxicityLevel.Toxic, catAssessment.Level);
            Assert.Equal("Irritating sap.", catAssessment.Details);
            Assert.Equal("https://example.test/monstera-safety", Assert.Single(catAssessment.Sources).Url);
            Assert.Equal(ToxicityLevel.NoKnownRisk, dogAssessment.Level);
            Assert.Equal("https://example.test/monstera-safety", Assert.Single(dogAssessment.Sources).Url);
            await AssertAllMigrationsAppliedAsync(context);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task Startup_RejectsLegacyDatabaseWithMissingPrimaryKey()
    {
        var databasePath = CreateDatabasePath();
        try
        {
            await using (var connection = new SqliteConnection($"Data Source={databasePath}"))
            {
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = """
                    CREATE TABLE "Plants" (
                        "Id" INTEGER NOT NULL,
                        "ScientificName" TEXT NOT NULL,
                        "CommonName" TEXT NOT NULL,
                        "Family" TEXT NOT NULL,
                        "Category" TEXT NOT NULL,
                        "Description" TEXT NOT NULL,
                        "Sunlight" TEXT NOT NULL,
                        "Watering" TEXT NOT NULL,
                        "FlowerColor" TEXT NULL,
                        "CreatedAt" TEXT NOT NULL,
                        "UpdatedAt" TEXT NOT NULL
                    );
                    """;
                await command.ExecuteNonQueryAsync();
            }

            await using var provider = CreateServiceProvider(databasePath);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => InitializeAsync(provider));
            Assert.Contains("does not match the initial schema", exception.Message);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task Startup_RejectsLegacyDatabaseWithUnexpectedRequiredColumn()
    {
        var databasePath = CreateDatabasePath();
        try
        {
            await using (var connection = new SqliteConnection($"Data Source={databasePath}"))
            {
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = """
                    CREATE TABLE "Plants" (
                        "Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                        "ScientificName" TEXT NOT NULL,
                        "CommonName" TEXT NOT NULL,
                        "Family" TEXT NOT NULL,
                        "Category" TEXT NOT NULL,
                        "Description" TEXT NOT NULL,
                        "Sunlight" TEXT NOT NULL,
                        "Watering" TEXT NOT NULL,
                        "FlowerColor" TEXT NULL,
                        "CreatedAt" TEXT NOT NULL,
                        "UpdatedAt" TEXT NOT NULL,
                        "LegacyRequiredValue" TEXT NOT NULL
                    );
                    """;
                await command.ExecuteNonQueryAsync();
            }

            await using var provider = CreateServiceProvider(databasePath);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => InitializeAsync(provider));

            Assert.Contains("LegacyRequiredValue", exception.Message);
            Assert.Contains("unexpected column", exception.Message);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    [Fact]
    public async Task UnitOfWorkSetsAuditTimestampsOnInsertAndUpdate()
    {
        var databasePath = CreateDatabasePath();
        try
        {
            await using var provider = CreateServiceProvider(databasePath);
            await InitializeAsync(provider);

            var context = provider.GetRequiredService<DatabaseContext>();
            var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
            var plant = CreatePlant("Rose");
            context.Plants.Add(plant);
            await unitOfWork.SaveChangesAsync();

            Assert.NotEqual(default, plant.CreatedAt);
            Assert.NotEqual(default, plant.UpdatedAt);
            var originalCreatedAt = plant.CreatedAt;

            plant.CommonName = "Updated rose";
            plant.UpdatedAt = DateTime.UnixEpoch;
            await unitOfWork.SaveChangesAsync();

            Assert.Equal(originalCreatedAt, plant.CreatedAt);
            Assert.True(plant.UpdatedAt > DateTime.UnixEpoch);
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    private static ServiceProvider CreateServiceProvider(string databasePath)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:Provider"] = "Sqlite",
                ["ConnectionStrings:Greenhouse"] = $"Data Source={databasePath}"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);
        return services.BuildServiceProvider();
    }

    private static async Task InitializeAsync(ServiceProvider provider)
    {
        foreach (var hostedService in provider.GetServices<IHostedService>())
        {
            await hostedService.StartAsync(CancellationToken.None);
        }
    }

    private static async Task AssertAllMigrationsAppliedAsync(DatabaseContext context)
    {
        var availableMigrations = context.Database.GetMigrations();
        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
        Assert.Equal(availableMigrations, appliedMigrations);
    }

    private static Plant CreatePlant(string commonName) => new()
    {
        ScientificName = "Rosa canina",
        CommonName = commonName,
        Family = "Rosaceae",
        Category = PlantType.Flowering,
        Description = "A flowering plant.",
        IdentifyingCharacteristics = "Fragrant flowers and thorny stems."
    };

    private static string CreateDatabasePath() =>
        Path.Combine(Path.GetTempPath(), $"greenhouse-{Guid.NewGuid():N}.db");

    private static void DeleteDatabaseFiles(string databasePath)
    {
        foreach (var path in new[] { databasePath, $"{databasePath}-shm", $"{databasePath}-wal" })
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
