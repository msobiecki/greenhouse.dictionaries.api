using Greenhouse.Dictionaries.Domain.Entities;
using Greenhouse.Dictionaries.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace Greenhouse.Dictionaries.Api.Tests.Context;

public class DatabaseContextTests
{
    [Fact]
    public void PlantEntity_UsesIdAsPrimaryKeyAndHasNoSeedData()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlServer("Server=localhost;Database=Greenhouse;User Id=sa;Password=NotARealPassword123!;TrustServerCertificate=True")
            .Options;
        using var context = new DatabaseContext(options);

        var plantEntity = context.Model.FindEntityType(typeof(Plant));
        var designTimePlantEntity = context.GetService<IDesignTimeModel>()
            .Model.FindEntityType(typeof(Plant));

        Assert.NotNull(plantEntity);
        Assert.Equal(nameof(Plant.Id), Assert.Single(plantEntity.FindPrimaryKey()!.Properties).Name);
        Assert.NotNull(designTimePlantEntity);
        Assert.Empty(designTimePlantEntity.GetSeedData());
        Assert.Equal("greenhouse", plantEntity.GetSchema());
    }
}
