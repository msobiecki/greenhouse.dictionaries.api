using System.Text.Json.Serialization;

namespace Greenhouse.Dictionaries.Domain.Entities;

[JsonConverter(typeof(JsonStringEnumConverter<PlantType>))]
public enum PlantType
{
    Unknown,
    Flowering,
    Foliage,
    Succulent,
    Fern,
    Palm,
    Climber,
    Carnivorous,
    Other
}
