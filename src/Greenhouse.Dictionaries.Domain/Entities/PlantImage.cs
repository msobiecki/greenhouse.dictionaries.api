namespace Greenhouse.Dictionaries.Domain.Entities;

public sealed class PlantImage
{
    public int Id { get; set; }
    public int PlantId { get; set; }
    public required string Url { get; set; }
    public required string Source { get; set; }
    public required string License { get; set; }
    public string? Attribution { get; set; }
}
