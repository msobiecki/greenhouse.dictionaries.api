using Greenhouse.Dictionaries.Domain.Entities;

namespace Greenhouse.Dictionaries.Application.Abstractions;

public sealed record PlantSearchFilters(
    string? Family = null,
    SunlightRequirement? SunlightRequirement = null,
    WateringRequirement? WateringRequirement = null,
    string? ToxicityTarget = null,
    ToxicityLevel? ToxicityLevel = null,
    decimal? TemperatureC = null,
    decimal? HumidityPercent = null,
    string? Category = null);
