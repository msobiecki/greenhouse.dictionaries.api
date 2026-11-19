using Greenhouse.Dictionaries.Domain.Entities;
using Greenhouse.Dictionaries.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace Greenhouse.Dictionaries.Api.Tests.Infrastructure.Context;

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
        Assert.Equal("Plants", plantEntity.GetTableName());
        Assert.Equal("datetime2(0)", plantEntity.FindProperty(nameof(Plant.CreatedAt))!.GetColumnType());
        Assert.Equal("datetime2(0)", plantEntity.FindProperty(nameof(Plant.UpdatedAt))!.GetColumnType());
        Assert.False(plantEntity.FindProperty(nameof(Plant.CommonName))!.IsNullable);
        Assert.False(plantEntity.FindProperty(nameof(Plant.IdentifyingCharacteristics))!.IsNullable);
        Assert.NotNull(plantEntity.FindNavigation(nameof(Plant.AlternativeNames)));
        Assert.NotNull(plantEntity.FindNavigation(nameof(Plant.Images)));
    }

    [Fact]
    public void SqlServerMigrationScriptCreatesSchemaAndIdentityKey()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlServer("Server=localhost;Database=Greenhouse;User Id=sa;Password=NotARealPassword123!;TrustServerCertificate=True")
            .Options;
        using var context = new DatabaseContext(options);

        var script = context.GetService<IMigrator>().GenerateScript();

        Assert.Contains("CREATE SCHEMA [greenhouse]", script, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("IDENTITY", script, StringComparison.OrdinalIgnoreCase);
    }
}
