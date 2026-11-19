using Greenhouse.Dictionaries.Domain.Entities;

namespace Greenhouse.Dictionaries.Application.DTOs;

public sealed record PlantInputDto(
    string ScientificName,
    string CommonName,
    string[]? AlternativeNames,
    string Family,
    PlantType Category,
    string Description,
    string IdentifyingCharacteristics,
    string? NativeRange = null,
    string? Habitat = null,
    decimal? TemperatureMinC = null,
    decimal? TemperatureMaxC = null,
    decimal? HumidityMinPercent = null,
    decimal? HumidityMaxPercent = null,
    string? EnvironmentalNotes = null,
    SunlightRequirement? SunlightRequirement = null,
    decimal? SunlightHoursMin = null,
    decimal? SunlightHoursMax = null,
    decimal? MatureHeightCm = null,
    decimal? MatureWidthCm = null,
    PlantToxicityAssessmentInputDto[]? ToxicityAssessments = null,
    PlantImageInputDto[]? Images = null,
    string? DetailedDescription = null,
    string? GrowthHabit = null,
    string? FoliageDescription = null,
    string? FlowerDescription = null,
    string? FloweringPeriod = null,
    string? FruitDescription = null,
    PlantLifeCycle? LifeCycle = null,
    WateringRequirement? WateringRequirement = null,
    string? WateringInstructions = null,
    string? SeasonalWateringNotes = null,
    string? FertilizingInstructions = null,
    string? PruningInstructions = null,
    string? RepottingInstructions = null);

public sealed record PlantToxicityAssessmentInputDto(
    string TargetCode,
    ToxicityLevel Level,
    string? Details = null,
    PlantToxicitySourceInputDto[]? Sources = null);

public sealed record PlantToxicitySourceInputDto(string Url, string? Publisher = null);

public sealed record PlantImageInputDto(
    string Url,
    string Source,
    string License,
    string? Attribution = null);
