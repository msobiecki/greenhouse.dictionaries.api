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
        context.Plants.AddRange(
            CreatePlant("Rugosa rose", "Rosa rugosa", "flower"),
            CreatePlant("Orchid", "Phalaenopsis", "flower"),
            CreatePlant("Oak", "Quercus robur", "tree"));
        await context.SaveChangesAsync();
        var repository = new PlantReadRepository(context);

        var result = await repository.GetAllAsync("ROSA", "FLOWER");

        Assert.Equal(2, result.Count());
        Assert.Contains(result, plant => plant.CommonName == "Rugosa rose");
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

            var rose = CreatePlant("Rugosa rose", "Rosa rugosa", "flower");
            var orchid = CreatePlant("Orchid", "Phalaenopsis", "flower");
            var tree = CreatePlant("Oak", "Quercus robur", "tree");
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
            Assert.Equal("Rugosa rose", Assert.Single(await readRepository.GetAllAsync("rugosa")).CommonName);
            Assert.Equal(2, (await readRepository.GetAllAsync(category: "FLOWER")).Count());
            Assert.Equal(2, (await readRepository.GetAllAsync(search: "Rosaceae")).Count());
            Assert.Equal(3, (await readRepository.GetAllAsync()).Count());

            var detachedRose = await readRepository.GetByIdAsync(rose.Id);
            Assert.NotNull(detachedRose);
            Assert.Equal(EntityState.Detached, services.GetRequiredService<DatabaseContext>().Entry(detachedRose).State);

            detachedRose.CommonName = "Updated rose";
            await writeRepository.UpdateAsync(detachedRose);
            await unitOfWork.SaveChangesAsync();
            Assert.Equal("Updated rose", (await readRepository.GetByIdAsync(rose.Id))?.CommonName);

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

    private static Plant CreatePlant(string commonName, string scientificName, string category) => new()
    {
        CommonName = commonName,
        ScientificName = scientificName,
        Family = "Rosaceae",
        Category = category,
        Description = "A plant used to test repository behavior.",
        Sunlight = "Full sun",
        Watering = "Regular"
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
