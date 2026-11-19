using Greenhouse.Dictionaries.Application;
using Greenhouse.Dictionaries.Application.Commands;
using Greenhouse.Dictionaries.Application.DTOs;
using Greenhouse.Dictionaries.Application.Abstractions;
using Greenhouse.Dictionaries.Domain.Entities;
using Greenhouse.Dictionaries.Presentation.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Greenhouse.Dictionaries.Api.Tests.Presentation.Controllers;

public class PlantsControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsPlantsFromRepository()
    {
        IReadOnlyList<Plant> plants =
        [
            new Plant
            {
                Id = 1,
                ScientificName = "Rosa",
                CommonName = "Rose",
                Family = "Rosaceae",
                Category = PlantType.Flowering,
                Description = "A flowering shrub.",
                IdentifyingCharacteristics = "Red flowers and thorny stems."
            }
        ];
        using var provider = CreateServiceProvider(new StubPlantRepository(plants));
        var controller = provider.GetRequiredService<PlantsController>();

        var result = await controller.GetAll(
            "rose", "Flowering", null, null, null, null, null, null, null, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var resultPlants = Assert.IsAssignableFrom<IEnumerable<PlantDto>>(okResult.Value);
        var resultPlant = Assert.Single(resultPlants);
        Assert.Equal(plants[0].Id, resultPlant.Id);
        Assert.Equal(plants[0].CommonName, resultPlant.CommonName);
    }

    [Fact]
    public async Task GetById_ReturnsNotFoundWhenPlantDoesNotExist()
    {
        using var provider = CreateServiceProvider(new StubPlantRepository());
        var controller = provider.GetRequiredService<PlantsController>();

        var result = await controller.GetById(999, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Update_UsesRouteIdAndUpdatesPlant()
    {
        var existingPlant = new Plant
        {
            Id = 1,
            ScientificName = "Rosa rugosa",
            CommonName = "Old name",
            Family = "Rosaceae",
            Category = PlantType.Flowering,
            Description = "A hardy flowering shrub.",
            IdentifyingCharacteristics = "Thorny arching stems."
        };
        using var provider = CreateServiceProvider(new StubPlantRepository([existingPlant]));
        var controller = provider.GetRequiredService<PlantsController>();

        var result = await controller.Update(1, new PlantInputDto(
            "Rosa rugosa", "Rugosa rose", [], "Rosaceae", PlantType.Flowering,
            "A hardy flowering shrub.", "Thorny arching stems."), CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("Rugosa rose", existingPlant.CommonName);
    }

    private static ServiceProvider CreateServiceProvider(StubPlantRepository repository)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();
        services.AddSingleton<IPlantWriteRepository>(repository);
        services.AddSingleton<IPlantReadRepository>(repository);
        services.AddSingleton<IUnitOfWork, StubUnitOfWork>();
        services.AddTransient<PlantsController>();
        return services.BuildServiceProvider();
    }

    private sealed class StubPlantRepository(IEnumerable<Plant>? plants = null)
        : IPlantWriteRepository, IPlantReadRepository
    {
        private readonly Dictionary<int, Plant> _plants = (plants ?? []).ToDictionary(plant => plant.Id);

        public Task<IEnumerable<Plant>> GetAllAsync(
            string? search = null,
            PlantSearchFilters? filters = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IEnumerable<Plant>>(_plants.Values);

        public Task<Plant?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _plants.TryGetValue(id, out var plant);
            return Task.FromResult(plant);
        }

        public Task<bool> IsAlreadyExistAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_plants.ContainsKey(id));

        public Task AddAsync(Plant plant, CancellationToken cancellationToken = default)
        {
            if (plant.Id == 0)
            {
                plant.Id = _plants.Count == 0 ? 1 : _plants.Keys.Max() + 1;
            }
            _plants.Add(plant.Id, plant);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Plant plant, CancellationToken cancellationToken = default)
        {
            _plants[plant.Id] = plant;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Plant plant, CancellationToken cancellationToken = default)
        {
            _plants.Remove(plant.Id);
            return Task.CompletedTask;
        }
    }

    private sealed class StubUnitOfWork : IUnitOfWork
    {
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
