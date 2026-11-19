using Greenhouse.Dictionaries.Application;
using Greenhouse.Dictionaries.Application.Commands;
using Greenhouse.Dictionaries.Application.DTOs;
using Greenhouse.Dictionaries.Application.Abstractions;
using Greenhouse.Dictionaries.Application.Configuration;
using Greenhouse.Dictionaries.Domain.Entities;
using Greenhouse.Dictionaries.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Greenhouse.Dictionaries.Api.Tests.Application.Commands;

public class PlantCommandsTests
{
    [Fact]
    public async Task AddPlantTask_AddsPlant()
    {
        var repository = new StubPlantRepository();
        using var provider = CreateServiceProvider(repository);
        var sender = provider.GetRequiredService<ISender>();
        var plant = CreatePlant();

        var result = await sender.Send(new AddPlantCommand(plant));

        Assert.Equal(1, result.Id);
        Assert.Equal(plant.CommonName, result.CommonName);
        Assert.Equal(plant.CommonName, (await repository.GetByIdAsync(result.Id))?.CommonName);
    }

    [Fact]
    public async Task AddPlantTask_AssignsDifferentIdsToNewPlants()
    {
        var repository = new StubPlantRepository();
        using var provider = CreateServiceProvider(repository);
        var sender = provider.GetRequiredService<ISender>();
        var plant = CreatePlant();

        var first = await sender.Send(new AddPlantCommand(plant));
        var second = await sender.Send(new AddPlantCommand(plant));

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public async Task AddPlantTask_RejectsInvalidInputThroughValidationPipeline()
    {
        using var provider = CreateServiceProvider(new StubPlantRepository());
        var sender = provider.GetRequiredService<ISender>();
        var invalidPlant = CreatePlant() with { CommonName = " " };

        await Assert.ThrowsAsync<ValidationException>(
            () => sender.Send(new AddPlantCommand(invalidPlant)));
    }

    [Fact]
    public async Task UpdatePlantTask_UpdatesExistingPlant()
    {
        var plant = CreatePlant(3);
        plant.CreatedAt = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var repository = new StubPlantRepository([plant]);
        using var provider = CreateServiceProvider(repository);
        var sender = provider.GetRequiredService<ISender>();
        var updatedPlant = CreatePlant("Updated plant");

        var result = await sender.Send(new UpdatePlantCommand(plant.Id, updatedPlant));

        Assert.Equal("Updated plant", (await repository.GetByIdAsync(plant.Id))?.CommonName);
        Assert.Equal(plant.CreatedAt, result.CreatedAt);
    }

    [Fact]
    public async Task UpdatePlantTask_ThrowsWhenPlantDoesNotExist()
    {
        using var provider = CreateServiceProvider(new StubPlantRepository());
        var sender = provider.GetRequiredService<ISender>();

        await Assert.ThrowsAsync<PlantNotFoundException>(() => sender.Send(new UpdatePlantCommand(4, CreatePlant())));
    }

    [Fact]
    public async Task DeletePlantTask_DeletesExistingPlant()
    {
        var plant = CreatePlant(5);
        var repository = new StubPlantRepository([plant]);
        using var provider = CreateServiceProvider(repository);
        var sender = provider.GetRequiredService<ISender>();

        await sender.Send(new DeletePlantCommand(plant.Id));

        Assert.Null(await repository.GetByIdAsync(plant.Id));
    }

    [Fact]
    public async Task DeletePlantTask_ThrowsWhenPlantDoesNotExist()
    {
        using var provider = CreateServiceProvider(new StubPlantRepository());
        var sender = provider.GetRequiredService<ISender>();

        await Assert.ThrowsAsync<PlantNotFoundException>(() => sender.Send(new DeletePlantCommand(6)));
    }

    private static ServiceProvider CreateServiceProvider(StubPlantRepository repository)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();
        services.AddSingleton<IPlantWriteRepository>(repository);
        services.AddSingleton<IPlantReadRepository>(repository);
        services.AddSingleton<IUnitOfWork, StubUnitOfWork>();
        return services.BuildServiceProvider();
    }

    private static Plant CreatePlant(int id, string commonName = "Common name") =>
        new()
        {
            Id = id,
            ScientificName = "Scientific name",
            CommonName = commonName,
            Family = "Family",
            Category = PlantType.Foliage,
            Description = "Description",
            IdentifyingCharacteristics = "Distinctive foliage."
        };

    private static PlantInputDto CreatePlant(string commonName = "Common name") =>
        new("Scientific name", commonName, [], "Family", PlantType.Foliage,
            "Description", "Distinctive foliage.");

    private sealed class StubPlantRepository(IEnumerable<Plant>? plants = null)
        : IPlantWriteRepository, IPlantReadRepository
    {
        private readonly Dictionary<int, Plant> _plants =
            (plants ?? []).ToDictionary(plant => plant.Id);

        public Task<PagedResult<Plant>> GetAllAsync(
            string? search = null,
            PlantSearchFilters? filters = null,
            int offset = 0,
            int limit = 50,
            string language = LanguageConfiguration.DefaultCulture,
            bool onlyRequestedLanguage = false,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new PagedResult<Plant>([], offset, limit, 0));

        public Task<Plant?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            _plants.TryGetValue(id, out var plant);
            return Task.FromResult(plant);
        }

        public Task<bool> IsAlreadyExistAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_plants.ContainsKey(id));

        public Task AddAsync(Plant plant, CancellationToken cancellationToken = default)
        {
            plant.Id = _plants.Count == 0 ? 1 : _plants.Keys.Max() + 1;
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
        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }
}
