namespace Greenhouse.Dictionaries.Domain.Entities;

public sealed class PlantTranslation
{
    public int Id { get; set; }
    public int PlantId { get; set; }
    public required string Language { get; set; }
    public required string CommonName { get; set; }
    public required string AlternativeNamesJson { get; set; }
    public required string Description { get; set; }
    public required string IdentifyingCharacteristics { get; set; }
    public string? ToxicityDetailsJson { get; set; }
    public string? NativeRange { get; set; }
    public string? Habitat { get; set; }
    public string? EnvironmentalNotes { get; set; }
    public string? DetailedDescription { get; set; }
    public string? GrowthHabit { get; set; }
    public string? FoliageDescription { get; set; }
    public string? FlowerDescription { get; set; }
    public string? FloweringPeriod { get; set; }
    public string? FruitDescription { get; set; }
    public string? WateringInstructions { get; set; }
    public string? SeasonalWateringNotes { get; set; }
    public string? FertilizingInstructions { get; set; }
    public string? PruningInstructions { get; set; }
    public string? RepottingInstructions { get; set; }
}
