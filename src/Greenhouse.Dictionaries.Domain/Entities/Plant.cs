namespace Greenhouse.Dictionaries.Domain.Entities;

public class Plant : Entity
{
    public required string ScientificName { get; set; }
    public required string CommonName { get; set; }
    public required string Family { get; set; }
    public required string Category { get; set; }
    public required string Description { get; set; }
    public required string Sunlight { get; set; }
    public required string Watering { get; set; }
    public string? FlowerColor { get; set; }
}
