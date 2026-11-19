namespace Greenhouse.Dictionaries.Domain.Entities;

public class Plant : Entity
{
    public required string ScientificName { get; set; }
    public required string CommonName { get; set; }
    public List<PlantAlias> AlternativeNames { get; set; } = [];
    public required string Family { get; set; }
    public required PlantType Category { get; set; }
    public required string Description { get; set; }
    public required string IdentifyingCharacteristics { get; set; }
    public string? NativeRange { get; set; }
    public string? Habitat { get; set; }
    public decimal? TemperatureMinC { get; set; }
    public decimal? TemperatureMaxC { get; set; }
    public decimal? HumidityMinPercent { get; set; }
    public decimal? HumidityMaxPercent { get; set; }
    public string? EnvironmentalNotes { get; set; }
    public SunlightRequirement? SunlightRequirement { get; set; }
    public decimal? SunlightHoursMin { get; set; }
    public decimal? SunlightHoursMax { get; set; }
    public WateringRequirement? WateringRequirement { get; set; }
    public string? WateringInstructions { get; set; }
    public string? SeasonalWateringNotes { get; set; }
    public string? FertilizingInstructions { get; set; }
    public string? PruningInstructions { get; set; }
    public string? RepottingInstructions { get; set; }
    public decimal? MatureHeightCm { get; set; }
    public decimal? MatureWidthCm { get; set; }
    public string? DetailedDescription { get; set; }
    public string? GrowthHabit { get; set; }
    public string? FoliageDescription { get; set; }
    public string? FlowerDescription { get; set; }
    public string? FloweringPeriod { get; set; }
    public string? FruitDescription { get; set; }
    public PlantLifeCycle? LifeCycle { get; set; }
    public List<PlantTranslation> Translations { get; set; } = [];
    public List<PlantToxicityAssessment> ToxicityAssessments { get; set; } = [];
    public List<PlantImage> Images { get; set; } = [];
}
