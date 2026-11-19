using Greenhouse.Dictionaries.Domain.Entities;

namespace Greenhouse.Dictionaries.Application.DTOs;

public sealed record PlantDto(
    int Id,
    string ScientificName,
    string CommonName,
    string[] AlternativeNames,
    string Family,
    PlantType Category,
    string Description,
    string IdentifyingCharacteristics,
    string? NativeRange,
    string? Habitat,
    decimal? TemperatureMinC,
    decimal? TemperatureMaxC,
    decimal? HumidityMinPercent,
    decimal? HumidityMaxPercent,
    string? EnvironmentalNotes,
    SunlightRequirement? SunlightRequirement,
    decimal? SunlightHoursMin,
    decimal? SunlightHoursMax,
    decimal? MatureHeightCm,
    decimal? MatureWidthCm,
    string? DetailedDescription,
    string? GrowthHabit,
    string? FoliageDescription,
    string? FlowerDescription,
    string? FloweringPeriod,
    string? FruitDescription,
    PlantLifeCycle? LifeCycle,
    WateringRequirement? WateringRequirement,
    string? WateringInstructions,
    string? SeasonalWateringNotes,
    string? FertilizingInstructions,
    string? PruningInstructions,
    string? RepottingInstructions,
    PlantToxicityAssessmentDto[] ToxicityAssessments,
    PlantImageDto[] Images,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record PlantToxicityAssessmentDto(
    string TargetCode,
    ToxicityLevel Level,
    string? Details,
    PlantToxicitySourceDto[] Sources);

public sealed record PlantToxicitySourceDto(string Url, string? Publisher);

public sealed record PlantImageDto(
    string Url,
    string Source,
    string License,
    string? Attribution);
