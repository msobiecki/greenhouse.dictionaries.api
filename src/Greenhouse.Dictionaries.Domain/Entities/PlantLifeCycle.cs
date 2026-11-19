using System.Text.Json.Serialization;

namespace Greenhouse.Dictionaries.Domain.Entities;

[JsonConverter(typeof(JsonStringEnumConverter<PlantLifeCycle>))]
public enum PlantLifeCycle
{
    Annual,
    Biennial,
    Perennial
}
