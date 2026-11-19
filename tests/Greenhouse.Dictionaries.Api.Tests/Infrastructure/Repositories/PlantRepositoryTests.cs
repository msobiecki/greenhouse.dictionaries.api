using Greenhouse.Dictionaries.Application.Abstractions;
using Greenhouse.Dictionaries.Domain.Entities;
using Greenhouse.Dictionaries.Infrastructure;
using Greenhouse.Dictionaries.Infrastructure.Repositories;
using Greenhouse.Dictionaries.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Greenhouse.Dictionaries.Api.Tests.Infrastructure.Repositories;

public sealed class PlantRepositoryTests
{
    [Fact]
    public async Task ReadRepositoryFiltersWithNonSqliteProvider()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new DatabaseContext(options);
        var rose = CreatePlant("Rugosa rose", "Rosa rugosa", PlantType.Flowering);
        rose.IdentifyingCharacteristics = "Thorny stems with wrinkled leaves.";
        rose.SunlightRequirement = SunlightRequirement.BrightIndirectLight;
        rose.WateringRequirement = WateringRequirement.WaterWhenTopLayerDry;
        rose.TemperatureMinC = 15;
        rose.TemperatureMaxC = 27;
        rose.HumidityMinPercent = 50;
        rose.HumidityMaxPercent = 70;
        rose.ToxicityAssessments.Add(new PlantToxicityAssessment
        {
            TargetCode = "cat",
            Level = ToxicityLevel.Toxic
        });
        context.Plants.AddRange(
            rose,
            CreatePlant("Orchid", "Phalaenopsis", PlantType.Flowering),
            CreatePlant("Oak", "Quercus robur", PlantType.Other));
        await context.SaveChangesAsync();
        var repository = new PlantReadRepository(context);

        var result = await repository.GetAllAsync(
            "ROSA", new PlantSearchFilters(Category: "FLOWERING"));

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, plant => plant.CommonName == "Rugosa rose");

        var discoverResults = await repository.GetAllAsync(
            search: "WRINKLED",
            filters: new PlantSearchFilters(
                "rosaceae",
                SunlightRequirement.BrightIndirectLight,
                WateringRequirement.WaterWhenTopLayerDry,
                "CAT",
                ToxicityLevel.Toxic,
                TemperatureC: 20,
                HumidityPercent: 60,
                Category: "FLOWERING"));

        Assert.Equal("Rugosa rose", Assert.Single(discoverResults.Items).CommonName);
        Assert.Empty((await repository.GetAllAsync(
            "rose", new PlantSearchFilters(Category: "not-a-plant-type"))).Items);
    }

    [Fact]
    public async Task RepositoriesSupportFilteredReadsAndCrudOperations()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"greenhouse-repository-{Guid.NewGuid():N}.db");
        try
        {
            await using var provider = CreateProvider(databasePath);
            await InitializeAsync(provider);
            await using var scope = provider.CreateAsyncScope();
            var services = scope.ServiceProvider;
            var readRepository = services.GetRequiredService<IPlantReadRepository>();
            var writeRepository = services.GetRequiredService<IPlantWriteRepository>();
            var unitOfWork = services.GetRequiredService<IUnitOfWork>();

            var rose = CreatePlant("Rugosa rose", "Rosa rugosa", PlantType.Flowering);
            rose.AlternativeNames.Add(new PlantAlias { Name = "Beach rose" });
            rose.Images.Add(new PlantImage
            {
                Url = "https://example.test/rose.jpg",
                Source = "Botanical archive",
                License = "CC BY 4.0",
                Attribution = "Example author"
            });
            rose.NativeRange = "East Asia";
            var orchid = CreatePlant("Orchid", "Phalaenopsis", PlantType.Flowering);
            var tree = CreatePlant("Oak", "Quercus robur", PlantType.Other);
            tree.Family = "Fagaceae";
            await writeRepository.AddAsync(rose);
            await writeRepository.AddAsync(orchid);
            await writeRepository.AddAsync(tree);
            await unitOfWork.SaveChangesAsync();
            services.GetRequiredService<DatabaseContext>().ChangeTracker.Clear();

            Assert.True(rose.Id > 0);
            Assert.True(await readRepository.IsAlreadyExistAsync(rose.Id));
            Assert.False(await readRepository.IsAlreadyExistAsync(int.MaxValue));
            Assert.Equal(rose.Id, (await readRepository.GetByIdAsync(rose.Id))?.Id);
            Assert.Null(await readRepository.GetByIdAsync(int.MaxValue));
            Assert.Equal("Rugosa rose", Assert.Single((await readRepository.GetAllAsync("rugosa")).Items).CommonName);
            Assert.Equal(2, (await readRepository.GetAllAsync(
                filters: new PlantSearchFilters(Category: "FLOWERING"))).TotalCount);
            Assert.Equal(2, (await readRepository.GetAllAsync(search: "Rosaceae")).TotalCount);
            Assert.Equal("Rugosa rose", Assert.Single((await readRepository.GetAllAsync("beach rose")).Items).CommonName);
            Assert.Equal(3, (await readRepository.GetAllAsync()).TotalCount);
            var loadedRose = await readRepository.GetByIdAsync(rose.Id);
            Assert.NotNull(loadedRose);
            Assert.Equal("East Asia", loadedRose.NativeRange);
            Assert.Equal("Beach rose", Assert.Single(loadedRose.AlternativeNames).Name);
            Assert.Equal("CC BY 4.0", Assert.Single(loadedRose.Images).License);

            var detachedRose = await readRepository.GetByIdAsync(rose.Id);
            Assert.NotNull(detachedRose);
            Assert.Equal(EntityState.Detached, services.GetRequiredService<DatabaseContext>().Entry(detachedRose).State);

            detachedRose.CommonName = "Updated rose";
            detachedRose.AlternativeNames = [new PlantAlias { Name = "Japanese rose" }];
            detachedRose.Images = [];
            await writeRepository.UpdateAsync(detachedRose);
            await unitOfWork.SaveChangesAsync();
            var updatedRose = await readRepository.GetByIdAsync(rose.Id);
            Assert.Equal("Updated rose", updatedRose?.CommonName);
            Assert.Equal("Japanese rose", Assert.Single(updatedRose!.AlternativeNames).Name);
            Assert.Empty(updatedRose.Images);

            await writeRepository.DeleteAsync(tree);
            await unitOfWork.SaveChangesAsync();
            Assert.False(await readRepository.IsAlreadyExistAsync(tree.Id));
        }
        finally
        {
            DeleteDatabaseFiles(databasePath);
        }
    }

    private static ServiceProvider CreateProvider(string databasePath)
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
        foreach (var service in provider.GetServices<IHostedService>())
        {
            await service.StartAsync(CancellationToken.None);
        }
    }

    private static Plant CreatePlant(string commonName, string scientificName, PlantType category) => new()
    {
        CommonName = commonName,
        ScientificName = scientificName,
        Family = "Rosaceae",
        Category = category,
        Description = "A plant used to test repository behavior.",
        IdentifyingCharacteristics = "A distinctive test plant."
    };

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
