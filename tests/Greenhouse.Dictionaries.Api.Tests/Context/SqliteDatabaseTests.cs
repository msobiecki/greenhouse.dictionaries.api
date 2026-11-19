using Greenhouse.Dictionaries.Application.Abstractions;
using Greenhouse.Dictionaries.Domain.Entities;
using Greenhouse.Dictionaries.Infrastructure;
using Greenhouse.Dictionaries.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Greenhouse.Dictionaries.Api.Tests.Context;

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
            var result = await repository.GetAllAsync("ŻÓŁĆ", "FLOWER");

            Assert.Equal("Żółć", Assert.Single(result).CommonName);
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
    public async Task Startup_BaselinesExistingEnsureCreatedDatabaseAndPreservesData()
    {
        var databasePath = CreateDatabasePath();
        try
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlite($"Data Source={databasePath}")
                .Options;
            await using (var existingContext = new DatabaseContext(options))
            {
                await existingContext.Database.EnsureCreatedAsync();
                existingContext.Plants.Add(CreatePlant("Existing rose"));
                await existingContext.SaveChangesAsync();
            }

            await using var provider = CreateServiceProvider(databasePath);
            await InitializeAsync(provider);

            var context = provider.GetRequiredService<DatabaseContext>();
            Assert.Equal("Existing rose", (await context.Plants.SingleAsync()).CommonName);
            await AssertAllMigrationsAppliedAsync(context);
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
        Category = "flower",
        Description = "A flowering plant.",
        Sunlight = "Full sun",
        Watering = "Regular"
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
