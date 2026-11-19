using System.Text.Json.Serialization;

namespace Greenhouse.Dictionaries.Domain.Entities;

[JsonConverter(typeof(JsonStringEnumConverter<WateringRequirement>))]
public enum WateringRequirement
{
    KeepEvenlyMoist,
    WaterWhenTopLayerDry,
    WaterWhenSubstratePartlyDry,
    WaterWhenSubstrateDry,
    AllowToDryBetweenWaterings,
    KeepDry
}
