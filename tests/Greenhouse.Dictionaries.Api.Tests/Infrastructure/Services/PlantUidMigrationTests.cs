using Greenhouse.Dictionaries.Domain.Entities;
using Greenhouse.Dictionaries.Infrastructure.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace Greenhouse.Dictionaries.Api.Tests.Infrastructure.Services;

public sealed class PlantUidMigrationTests
{
    [Fact]
    public async Task MigrationBackfillsDistinctUidsAndPreservesExistingPlantsAndRelations()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(connection)
            .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;
        await using var context = new DatabaseContext(options);
        var migrator = context.GetService<IMigrator>();
        await migrator.MigrateAsync("20261119124007_AddLocalizedToxicityDetails");
        await context.Database.ExecuteSqlRawAsync("""
            INSERT INTO "Plants" ("ScientificName", "CommonName", "Family", "Category", "Description",
                "IdentifyingCharacteristics", "CreatedAt", "UpdatedAt")
            VALUES ('Monstera deliciosa', 'Monstera', 'Araceae', 'Foliage', 'Description', 'Split leaves',
                '2020-01-02 03:04:05', '2020-01-02 03:04:05'),
                ('Ficus elastica', 'Rubber plant', 'Moraceae', 'Foliage', 'Description', 'Glossy leaves',
                '2020-01-02 03:04:05', '2020-01-02 03:04:05');
            INSERT INTO "PlantAliases" ("PlantId", "Name") VALUES (1, 'Swiss cheese plant');
            """);

        await migrator.MigrateAsync();

        var plants = await context.Plants.Include(plant => plant.AlternativeNames).OrderBy(plant => plant.Id).ToArrayAsync();
        Assert.Equal(2, plants.Length);
        Assert.Equal(new[] { 1, 2 }, plants.Select(plant => plant.Id));
        Assert.Equal("Monstera deliciosa", plants[0].ScientificName);
        Assert.Equal("Swiss cheese plant", Assert.Single(plants[0].AlternativeNames).Name);
        Assert.Equal(new DateTime(2020, 1, 2, 3, 4, 5), plants[0].CreatedAt);
        Assert.Equal(2, plants.Select(plant => plant.Uid).Distinct().Count());
        Assert.All(plants, plant =>
        {
            Assert.NotEqual(Guid.Empty, plant.Uid);
            Assert.Equal(4, plant.Uid.Version);
        });
        foreach (var plant in plants)
        {
            var reloaded = await context.Plants.AsNoTracking().SingleAsync(existing => existing.Uid == plant.Uid);
            Assert.Equal(plant.Id, reloaded.Id);
        }
        var uids = plants.Select(plant => plant.Uid).ToArray();
        await migrator.MigrateAsync();
        context.ChangeTracker.Clear();
        Assert.Equal(uids, await context.Plants.OrderBy(plant => plant.Id).Select(plant => plant.Uid).ToArrayAsync());

        var duplicate = new Plant
        {
            ScientificName = "Ficus lyrata",
            CommonName = "Fiddle-leaf fig",
            Family = "Moraceae",
            Category = PlantType.Foliage,
            Description = "Description",
            IdentifyingCharacteristics = "Violin-shaped leaves."
        };
        context.Plants.Add(duplicate);
        context.Entry(duplicate).Property(plant => plant.Uid).CurrentValue = uids[0];
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public void SqlServerMigrationUsesRandomUidsAndUniqueIndex()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlServer("Server=localhost;Database=GreenhouseTestsUid;Integrated Security=true;TrustServerCertificate=true")
            .Options;
        using var context = new DatabaseContext(options);

        var script = context.GetService<IMigrator>().GenerateScript(
            "20261119124007_AddLocalizedToxicityDetails", "20261119143119_AddPlantPublicUid");

        Assert.Contains("uniqueidentifier NOT NULL DEFAULT (NEWID())", script);
        Assert.Contains("CREATE UNIQUE INDEX [IX_Plants_Uid]", script);
        Assert.DoesNotContain("NEWSEQUENTIALID", script, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Guid.Empty.ToString(), script);
    }
}