namespace Greenhouse.Dictionaries.Domain.Entities;

public sealed class PlantToxicityAssessment
{
    public int Id { get; set; }
    public int PlantId { get; set; }
    public required string TargetCode { get; set; }
    public ToxicityLevel Level { get; set; }
    public string? Details { get; set; }
    public List<PlantToxicitySource> Sources { get; set; } = [];
}
