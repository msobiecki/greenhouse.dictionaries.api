using Greenhouse.Dictionaries.Application.Abstractions;
using Greenhouse.Dictionaries.Domain.Entities;
using Greenhouse.Dictionaries.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Greenhouse.Dictionaries.Infrastructure.Repositories;

internal sealed class PlantWriteRepository(DatabaseContext dbContext) : IPlantWriteRepository
{
    public async Task AddAsync(Plant plant, CancellationToken cancellationToken = default)
    {
        await dbContext.Plants.AddAsync(plant, cancellationToken);
    }

    public async Task UpdateAsync(Plant plant, CancellationToken cancellationToken = default)
    {
        var existingAliases = await dbContext.PlantAliases
            .Where(alias => alias.PlantId == plant.Id)
            .ToArrayAsync(cancellationToken);
        var existingImages = await dbContext.PlantImages
            .Where(image => image.PlantId == plant.Id)
            .ToArrayAsync(cancellationToken);
        var existingToxicityAssessments = await dbContext.PlantToxicityAssessments
            .Include(assessment => assessment.Sources)
            .Where(assessment => assessment.PlantId == plant.Id)
            .ToArrayAsync(cancellationToken);
        dbContext.PlantAliases.RemoveRange(existingAliases);
        dbContext.PlantImages.RemoveRange(existingImages);
        dbContext.PlantToxicityAssessments.RemoveRange(existingToxicityAssessments);
        dbContext.Plants.Update(plant);
    }

    public Task DeleteAsync(Plant plant, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        dbContext.Plants.Remove(plant);
        return Task.CompletedTask;
    }
}
