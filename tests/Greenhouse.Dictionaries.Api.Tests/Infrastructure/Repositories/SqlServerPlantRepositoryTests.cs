using Greenhouse.Dictionaries.Application.Abstractions;
using Greenhouse.Dictionaries.Domain.Entities;
using Greenhouse.Dictionaries.Infrastructure.Context;
using Greenhouse.Dictionaries.Infrastructure.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Greenhouse.Dictionaries.Api.Tests.Infrastructure.Repositories;

public sealed class SqlServerPlantRepositoryTests
{
    [SqlServerIntegrationFact]
    public async Task ProductionProviderMigratesAndExecutesFilteredPlantQuery()
    {
        var connectionString = Environment.GetEnvironmentVariable("GREENHOUSE_SQLSERVER_TEST_CONNECTION")!;
        var connection = new SqlConnectionStringBuilder(connectionString);
        if (!connection.InitialCatalog.StartsWith("GreenhouseTests", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "GREENHOUSE_SQLSERVER_TEST_CONNECTION must target a dedicated database whose name starts with 'GreenhouseTests'.");
        }

        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlServer(connectionString)
            .Options;
        await using var context = new DatabaseContext(options);
        await context.Database.MigrateAsync();

        var uniqueName = $"Integration-{Guid.NewGuid():N}";
        var plant = new Plant
        {
            ScientificName = "Rosa rugosa",
            CommonName = uniqueName,
            Family = "Rosaceae",
            Category = PlantType.Flowering,
            Description = "SQL Server provider integration test.",
            IdentifyingCharacteristics = "Thorny stems and pink flowers."
        };

        try
        {
            context.Plants.Add(plant);
            await context.SaveChangesAsync();

            var repository = new PlantReadRepository(context);
            var result = await repository.GetAllAsync(
                uniqueName, new PlantSearchFilters(Category: "FLOWERING"));

            Assert.Equal(plant.Id, Assert.Single(result.Items).Id);
            Assert.True(await repository.IsAlreadyExistAsync(plant.Id));
            Assert.Equal(plant.Id, (await repository.GetByIdAsync(plant.Id))?.Id);
        }
        finally
        {
            var insertedPlant = await context.Plants
                .SingleOrDefaultAsync(candidate => candidate.Id == plant.Id);
            if (insertedPlant is not null)
            {
                context.Plants.Remove(insertedPlant);
                await context.SaveChangesAsync();
            }
        }
    }
}

public sealed class SqlServerIntegrationFactAttribute : FactAttribute
{
    public SqlServerIntegrationFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GREENHOUSE_SQLSERVER_TEST_CONNECTION")))
        {
            Skip = "Set GREENHOUSE_SQLSERVER_TEST_CONNECTION to run against a dedicated SQL Server test database.";
        }
    }
}
