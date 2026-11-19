using Greenhouse.Dictionaries.Domain.Entities;
using Xunit;

namespace Greenhouse.Dictionaries.Api.Tests.Domain;

public sealed class PlantUidTests
{
    [Fact]
    public void NewPlantsReceiveDistinctRandomUids()
    {
        var first = CreatePlant();
        var second = CreatePlant();

        Assert.NotEqual(Guid.Empty, first.Uid);
        Assert.NotEqual(first.Uid, second.Uid);
        Assert.Equal(4, first.Uid.Version);
        Assert.Equal(4, second.Uid.Version);
    }

    [Fact]
    public void UpdatingPlantDoesNotChangeUid()
    {
        var plant = CreatePlant();
        var uid = plant.Uid;

        plant.CommonName = "Updated name";

        Assert.Equal(uid, plant.Uid);
        Assert.False(typeof(Plant).GetProperty(nameof(Plant.Uid))!.SetMethod!.IsPublic);
    }

    private static Plant CreatePlant() => new()
    {
        ScientificName = "Monstera deliciosa",
        CommonName = "Monstera",
        Family = "Araceae",
        Category = PlantType.Foliage,
        Description = "Tropical foliage plant.",
        IdentifyingCharacteristics = "Fenestrated mature leaves."
    };
}