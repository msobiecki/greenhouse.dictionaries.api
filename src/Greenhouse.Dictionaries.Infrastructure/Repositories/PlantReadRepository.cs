using Greenhouse.Dictionaries.Application.Abstractions;
using Greenhouse.Dictionaries.Domain.Entities;
using Greenhouse.Dictionaries.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Greenhouse.Dictionaries.Infrastructure.Repositories;

internal sealed class PlantReadRepository(DatabaseContext dbContext) : IPlantReadRepository
{
    public async Task<IEnumerable<Plant>> GetAllAsync(
        string? search = null,
        PlantSearchFilters? filters = null,
        CancellationToken cancellationToken = default)
    {
        var category = filters?.Category;
        PlantType? categoryFilter = null;
        if (!string.IsNullOrWhiteSpace(category))
        {
            if (!Enum.TryParse<PlantType>(category, true, out var parsedCategory) ||
                !Enum.IsDefined(parsedCategory))
            {
                return [];
            }

            categoryFilter = parsedCategory;
        }

        if (dbContext.Database.IsSqlite())
        {
            var plants = await dbContext.Plants
                .AsNoTracking()
                .Include(plant => plant.AlternativeNames)
                .Include(plant => plant.Images)
                .Include(plant => plant.ToxicityAssessments)
                    .ThenInclude(assessment => assessment.Sources)
                .ToArrayAsync(cancellationToken);
            return plants.Where(plant =>
                (string.IsNullOrWhiteSpace(search) ||
                    ContainsIgnoreCase(plant.CommonName, search) ||
                    ContainsIgnoreCase(plant.ScientificName, search) ||
                    ContainsIgnoreCase(plant.Family, search) ||
                    ContainsIgnoreCase(plant.Description, search) ||
                    ContainsIgnoreCase(plant.IdentifyingCharacteristics, search) ||
                    ContainsIgnoreCase(plant.DetailedDescription, search) ||
                    ContainsIgnoreCase(plant.NativeRange, search) ||
                    ContainsIgnoreCase(plant.Habitat, search) ||
                    ContainsIgnoreCase(plant.GrowthHabit, search) ||
                    ContainsIgnoreCase(plant.FoliageDescription, search) ||
                    ContainsIgnoreCase(plant.FlowerDescription, search) ||
                    ContainsIgnoreCase(plant.FruitDescription, search) ||
                    plant.AlternativeNames.Any(alias => ContainsIgnoreCase(alias.Name, search)) ||
                    plant.ToxicityAssessments.Any(assessment =>
                        ContainsIgnoreCase(assessment.TargetCode, search) ||
                        ContainsIgnoreCase(assessment.Details, search))) &&
                (!categoryFilter.HasValue || plant.Category == categoryFilter.Value) &&
                MatchesFilters(plant, filters));
        }

        IQueryable<Plant> query = dbContext.Plants
            .AsNoTracking()
            .Include(plant => plant.AlternativeNames)
            .Include(plant => plant.Images)
            .Include(plant => plant.ToxicityAssessments)
                .ThenInclude(assessment => assessment.Sources);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.ToLowerInvariant();
            query = query.Where(plant =>
                plant.CommonName.ToLower().Contains(normalizedSearch) ||
                plant.ScientificName.ToLower().Contains(normalizedSearch) ||
                plant.Family.ToLower().Contains(normalizedSearch) ||
                plant.Description.ToLower().Contains(normalizedSearch) ||
                plant.IdentifyingCharacteristics.ToLower().Contains(normalizedSearch) ||
                (plant.DetailedDescription != null && plant.DetailedDescription.ToLower().Contains(normalizedSearch)) ||
                (plant.NativeRange != null && plant.NativeRange.ToLower().Contains(normalizedSearch)) ||
                (plant.Habitat != null && plant.Habitat.ToLower().Contains(normalizedSearch)) ||
                (plant.GrowthHabit != null && plant.GrowthHabit.ToLower().Contains(normalizedSearch)) ||
                (plant.FoliageDescription != null && plant.FoliageDescription.ToLower().Contains(normalizedSearch)) ||
                (plant.FlowerDescription != null && plant.FlowerDescription.ToLower().Contains(normalizedSearch)) ||
                (plant.FruitDescription != null && plant.FruitDescription.ToLower().Contains(normalizedSearch)) ||
                plant.AlternativeNames.Any(alias => alias.Name.ToLower().Contains(normalizedSearch)) ||
                plant.ToxicityAssessments.Any(assessment =>
                    assessment.TargetCode.ToLower().Contains(normalizedSearch) ||
                    (assessment.Details != null && assessment.Details.ToLower().Contains(normalizedSearch))));
        }

        if (categoryFilter.HasValue)
        {
            query = query.Where(plant => plant.Category == categoryFilter.Value);
        }

        if (filters is not null)
        {
            if (!string.IsNullOrWhiteSpace(filters.Family))
            {
                var normalizedFamily = filters.Family.Trim().ToLowerInvariant();
                query = query.Where(plant => plant.Family.ToLower() == normalizedFamily);
            }

            if (filters.SunlightRequirement.HasValue)
            {
                query = query.Where(plant => plant.SunlightRequirement == filters.SunlightRequirement.Value);
            }

            if (filters.WateringRequirement.HasValue)
            {
                query = query.Where(plant => plant.WateringRequirement == filters.WateringRequirement.Value);
            }

            if (!string.IsNullOrWhiteSpace(filters.ToxicityTarget))
            {
                var normalizedTarget = filters.ToxicityTarget.Trim().ToLowerInvariant();
                query = query.Where(plant => plant.ToxicityAssessments.Any(assessment =>
                    assessment.TargetCode.ToLower() == normalizedTarget &&
                    (!filters.ToxicityLevel.HasValue || assessment.Level == filters.ToxicityLevel.Value)));
            }
            else if (filters.ToxicityLevel.HasValue)
            {
                query = query.Where(plant => plant.ToxicityAssessments.Any(assessment =>
                    assessment.Level == filters.ToxicityLevel.Value));
            }

            if (filters.TemperatureC.HasValue)
            {
                var temperature = filters.TemperatureC.Value;
                query = query.Where(plant =>
                    plant.TemperatureMinC <= temperature && plant.TemperatureMaxC >= temperature);
            }

            if (filters.HumidityPercent.HasValue)
            {
                var humidity = filters.HumidityPercent.Value;
                query = query.Where(plant =>
                    plant.HumidityMinPercent <= humidity && plant.HumidityMaxPercent >= humidity);
            }
        }

        return await query.ToArrayAsync(cancellationToken);
    }

    private static bool ContainsIgnoreCase(string? value, string search) =>
        value?.Contains(search, StringComparison.OrdinalIgnoreCase) == true;

    private static bool MatchesFilters(Plant plant, PlantSearchFilters? filters)
    {
        if (filters is null)
        {
            return true;
        }

        return (string.IsNullOrWhiteSpace(filters.Family) ||
                string.Equals(plant.Family, filters.Family.Trim(), StringComparison.OrdinalIgnoreCase)) &&
            (!filters.SunlightRequirement.HasValue || plant.SunlightRequirement == filters.SunlightRequirement) &&
            (!filters.WateringRequirement.HasValue || plant.WateringRequirement == filters.WateringRequirement) &&
            (string.IsNullOrWhiteSpace(filters.ToxicityTarget) && !filters.ToxicityLevel.HasValue ||
                plant.ToxicityAssessments.Any(assessment =>
                    (string.IsNullOrWhiteSpace(filters.ToxicityTarget) ||
                        string.Equals(assessment.TargetCode, filters.ToxicityTarget.Trim(), StringComparison.OrdinalIgnoreCase)) &&
                    (!filters.ToxicityLevel.HasValue || assessment.Level == filters.ToxicityLevel))) &&
            (!filters.TemperatureC.HasValue ||
                plant.TemperatureMinC <= filters.TemperatureC && plant.TemperatureMaxC >= filters.TemperatureC) &&
            (!filters.HumidityPercent.HasValue ||
                plant.HumidityMinPercent <= filters.HumidityPercent && plant.HumidityMaxPercent >= filters.HumidityPercent);
    }

    public Task<Plant?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Plants
            .AsNoTracking()
            .Include(plant => plant.AlternativeNames)
            .Include(plant => plant.Images)
            .Include(plant => plant.ToxicityAssessments)
                .ThenInclude(assessment => assessment.Sources)
            .FirstOrDefaultAsync(plant => plant.Id == id, cancellationToken);

    public Task<bool> IsAlreadyExistAsync(int id, CancellationToken cancellationToken = default) =>
        dbContext.Plants.AnyAsync(plant => plant.Id == id, cancellationToken);
}
