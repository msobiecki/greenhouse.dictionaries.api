using System.Text.Json.Serialization;

namespace Greenhouse.Dictionaries.Domain.Entities;

[JsonConverter(typeof(JsonStringEnumConverter<SunlightRequirement>))]
public enum SunlightRequirement
{
    LowLight,
    MediumLight,
    BrightIndirectLight,
    PartialSun,
    FullSun,
    PartialShade,
    FullShade
}
