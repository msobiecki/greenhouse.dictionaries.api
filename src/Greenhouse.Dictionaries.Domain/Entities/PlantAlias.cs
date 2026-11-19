namespace Greenhouse.Dictionaries.Domain.Entities;

public sealed class PlantAlias
{
    public int Id { get; set; }
    public int PlantId { get; set; }
    public required string Name { get; set; }
}
