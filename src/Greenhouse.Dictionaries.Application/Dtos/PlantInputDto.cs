namespace Greenhouse.Dictionaries.Application.DTOs;

public sealed record PlantInputDto(
    string ScientificName,
    string CommonName,
    string Family,
    string Category,
    string Description,
    string Sunlight,
    string Watering,
    string? FlowerColor = null);
