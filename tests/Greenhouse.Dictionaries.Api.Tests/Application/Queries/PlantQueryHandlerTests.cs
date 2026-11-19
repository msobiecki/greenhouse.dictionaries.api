using Greenhouse.Dictionaries.Application;
using Greenhouse.Dictionaries.Application.Abstractions;
using Greenhouse.Dictionaries.Application.Queries;
using Greenhouse.Dictionaries.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Greenhouse.Dictionaries.Api.Tests.Application.Queries;

public sealed class PlantQueryHandlerTests
{
    [Fact]
    public async Task GetPlantsQuery_MapsPlantsAndForwardsFilters()
    {
        var repository = new StubPlantReadRepository([CreatePlant(1), CreatePlant(2, "White rose")]);
        using var provider = CreateProvider(repository);
        var sender = provider.GetRequiredService<MediatR.ISender>();

        var result = (await sender.Send(new GetPlantsQuery("rose", "flower"))).ToArray();

        Assert.Equal(2, result.Length);
        Assert.Equal("Rose", result[0].CommonName);
        Assert.Equal("flower", result[0].Category);
        Assert.Equal("rose", repository.LastSearch);
        Assert.Equal("flower", repository.LastCategory);
    }

    [Fact]
    public async Task GetPlantByIdQuery_MapsPlantWhenFound()
    {
        var plant = CreatePlant(7);
        using var provider = CreateProvider(new StubPlantReadRepository([plant]));
        var sender = provider.GetRequiredService<MediatR.ISender>();

        var result = await sender.Send(new GetPlantByIdQuery(7));

        Assert.NotNull(result);
        Assert.Equal(7, result.Id);
        Assert.Equal(plant.CommonName, result.CommonName);
        Assert.Equal(plant.CreatedAt, result.CreatedAt);
    }

    [Fact]
    public async Task GetPlantByIdQuery_ReturnsNullWhenMissing()
    {
        using var provider = CreateProvider(new StubPlantReadRepository([]));
        var sender = provider.GetRequiredService<MediatR.ISender>();

        var result = await sender.Send(new GetPlantByIdQuery(404));

        Assert.Null(result);
    }

    private static ServiceProvider CreateProvider(StubPlantReadRepository repository)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();
        services.AddSingleton<IPlantReadRepository>(repository);
        return services.BuildServiceProvider();
    }

    private static Plant CreatePlant(int id, string commonName = "Rose") => new()
    {
        Id = id,
        ScientificName = "Rosa rugosa",
        CommonName = commonName,
        Family = "Rosaceae",
        Category = "flower",
        Description = "A flowering shrub.",
        Sunlight = "Full sun",
        Watering = "Regular"
    };

    private sealed class StubPlantReadRepository(IEnumerable<Plant> plants) : IPlantReadRepository
    {
        public string? LastSearch { get; private set; }
        public string? LastCategory { get; private set; }

        public Task<IEnumerable<Plant>> GetAllAsync(
            string? search = null,
            string? category = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastSearch = search;
            LastCategory = category;
            return Task.FromResult(plants);
        }

        public Task<Plant?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(plants.FirstOrDefault(plant => plant.Id == id));
        }

        public Task<bool> IsAlreadyExistAsync(int id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(plants.Any(plant => plant.Id == id));
        }
    }
}
