using FluentValidation.TestHelper;
using Greenhouse.Dictionaries.Application.Commands;
using Greenhouse.Dictionaries.Application.DTOs;
using Xunit;

namespace Greenhouse.Dictionaries.Api.Tests.Application.Commands;

public class UpdatePlantCommandValidationTests
{
    private readonly UpdatePlantCommandValidation _validator = new();

    [Fact]
    public void Validate_AcceptsPlantWithRequiredFields()
    {
        var result = _validator.TestValidate(new UpdatePlantCommand(1, CreatePlant()));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_RejectsMissingRequiredPlantField()
    {
        var plant = CreatePlant() with { CommonName = " " };

        var result = _validator.TestValidate(new UpdatePlantCommand(1, plant));

        result.ShouldHaveValidationErrorFor(command => command.Plant.CommonName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_RejectsNonPositiveId(int id)
    {
        var result = _validator.TestValidate(new UpdatePlantCommand(id, CreatePlant()));

        result.ShouldHaveValidationErrorFor(command => command.Id);
    }

    [Fact]
    public void Validate_RejectsEveryMissingRequiredField()
    {
        var plant = CreatePlant() with
        {
            ScientificName = "",
            CommonName = "",
            Family = "",
            Category = "",
            Description = "",
            Sunlight = "",
            Watering = ""
        };

        var result = _validator.TestValidate(new UpdatePlantCommand(1, plant));

        result.ShouldHaveValidationErrorFor(command => command.Plant.ScientificName);
        result.ShouldHaveValidationErrorFor(command => command.Plant.CommonName);
        result.ShouldHaveValidationErrorFor(command => command.Plant.Family);
        result.ShouldHaveValidationErrorFor(command => command.Plant.Category);
        result.ShouldHaveValidationErrorFor(command => command.Plant.Description);
        result.ShouldHaveValidationErrorFor(command => command.Plant.Sunlight);
        result.ShouldHaveValidationErrorFor(command => command.Plant.Watering);
    }

    private static PlantInputDto CreatePlant() => new(
        "Rosa rugosa",
        "Rugosa rose",
        "Rosaceae",
        "flower",
        "A hardy flowering shrub.",
        "Full sun",
        "Water during dry periods.");
}
