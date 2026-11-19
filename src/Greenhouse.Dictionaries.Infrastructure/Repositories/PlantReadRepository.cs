using Greenhouse.Dictionaries.Application.Abstractions;
using Greenhouse.Dictionaries.Domain.Entities;
using Greenhouse.Dictionaries.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Greenhouse.Dictionaries.Infrastructure.Repositories;

internal sealed class PlantReadRepository(DatabaseContext dbContext) : IPlantReadRepository
{
    public async Task<IEnumerable<Plant>> GetAllAsync(
        string? search = null,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        if (dbContext.Database.IsSqlite())
        {
            var plants = await dbContext.Plants.AsNoTracking().ToArrayAsync(cancellationToken);
            return plants.Where(plant =>
                (string.IsNullOrWhiteSpace(search) ||
                    plant.CommonName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    plant.ScientificName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    plant.Family.Contains(search, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(category) ||
                    string.Equals(plant.Category, category, StringComparison.OrdinalIgnoreCase)));
        }

        IQueryable<Plant> query = dbContext.Plants.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.ToLowerInvariant();
            query = query.Where(plant =>
                plant.CommonName.ToLower().Contains(normalizedSearch) ||
                plant.ScientificName.ToLower().Contains(normalizedSearch) ||
                plant.Family.ToLower().Contains(normalizedSearch));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var normalizedCategory = category.ToLowerInvariant();
            query = query.Where(plant => plant.Category.ToLower() == normalizedCategory);
        }

        return await query.ToArrayAsync(cancellationToken);
    }

    public Task<Plant?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Plants
            .AsNoTracking()
            .FirstOrDefaultAsync(plant => plant.Id == id, cancellationToken);

    public Task<bool> IsAlreadyExistAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Plants.AnyAsync(plant => plant.Id == id, cancellationToken);
}
