using System.Text.Json.Serialization;

namespace Greenhouse.Dictionaries.Domain.Entities;

[JsonConverter(typeof(JsonStringEnumConverter<ToxicityLevel>))]
public enum ToxicityLevel
{
    Unknown,
    NoKnownRisk,
    Caution,
    Toxic
}
