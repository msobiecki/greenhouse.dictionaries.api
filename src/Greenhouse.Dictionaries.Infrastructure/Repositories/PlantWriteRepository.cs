using Greenhouse.Dictionaries.Application.Abstractions;
using Greenhouse.Dictionaries.Domain.Entities;
using Greenhouse.Dictionaries.Infrastructure.Context;

namespace Greenhouse.Dictionaries.Infrastructure.Repositories;

internal sealed class PlantWriteRepository(DatabaseContext dbContext) : IPlantWriteRepository
{
    public async Task AddAsync(Plant plant, CancellationToken cancellationToken = default)
    {
        await dbContext.Plants.AddAsync(plant, cancellationToken);
    }

    public Task UpdateAsync(Plant plant, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        dbContext.Plants.Update(plant);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Plant plant, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        dbContext.Plants.Remove(plant);
        return Task.CompletedTask;
    }
}
