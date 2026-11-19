namespace Greenhouse.Dictionaries.Application.DTOs;

public sealed record PlantDto(
    int Id,
    string ScientificName,
    string CommonName,
    string Family,
    string Category,
    string Description,
    string Sunlight,
    string Watering,
    string? FlowerColor,
    DateTime CreatedAt,
    DateTime UpdatedAt);
