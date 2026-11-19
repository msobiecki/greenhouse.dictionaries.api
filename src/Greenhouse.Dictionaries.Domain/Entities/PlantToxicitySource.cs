namespace Greenhouse.Dictionaries.Domain.Entities;

public sealed class PlantToxicitySource
{
    public int Id { get; set; }
    public int PlantToxicityAssessmentId { get; set; }
    public required string Url { get; set; }
    public string? Publisher { get; set; }
}
