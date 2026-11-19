using FluentValidation.TestHelper;
using Greenhouse.Dictionaries.Application.Commands;
using Greenhouse.Dictionaries.Application.DTOs;
using Greenhouse.Dictionaries.Domain.Entities;
using Xunit;

namespace Greenhouse.Dictionaries.Api.Tests.Application.Commands;

public class AddPlantCommandValidationTests
{
    private readonly AddPlantCommandValidation _validator = new();

    [Fact]
    public void Validate_AcceptsPlantWithRequiredFields()
    {
        var result = _validator.TestValidate(new AddPlantCommand(CreatePlant()));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_RejectsMissingRequiredPlantField()
    {
        var plant = CreatePlant() with { CommonName = " " };

        var result = _validator.TestValidate(new AddPlantCommand(plant));

        result.ShouldHaveValidationErrorFor(command => command.Plant.CommonName);
    }

    [Fact]
    public void Validate_RejectsDuplicateToxicityTargets()
    {
        var plant = CreatePlant() with
        {
            ToxicityAssessments =
            [
                new("cat", ToxicityLevel.Toxic),
                new("cat", ToxicityLevel.Caution)
            ]
        };

        var result = _validator.TestValidate(new AddPlantCommand(plant));

        result.ShouldHaveValidationErrorFor(command => command.Plant.ToxicityAssessments);
    }

    [Fact]
    public void Validate_RejectsInvalidSunlightHourRange()
    {
        var plant = CreatePlant() with { SunlightHoursMin = 8, SunlightHoursMax = 4 };

        var result = _validator.TestValidate(new AddPlantCommand(plant));

        result.ShouldHaveValidationErrorFor(command => command.Plant);
    }

    [Fact]
    public void Validate_RejectsInvalidTemperatureAndHumidityRanges()
    {
        var plant = CreatePlant() with
        {
            TemperatureMinC = 28,
            TemperatureMaxC = 18,
            HumidityMinPercent = 80,
            HumidityMaxPercent = 40
        };

        var result = _validator.TestValidate(new AddPlantCommand(plant));

        result.ShouldHaveValidationErrorFor(command => command.Plant);
    }

    [Fact]
    public void Validate_RejectsEveryMissingRequiredField()
    {
        var plant = CreatePlant() with
        {
            ScientificName = "",
            CommonName = "",
            Family = "",
            Category = (PlantType)999,
            Description = "",
            IdentifyingCharacteristics = ""
        };

        var result = _validator.TestValidate(new AddPlantCommand(plant));

        result.ShouldHaveValidationErrorFor(command => command.Plant.ScientificName);
        result.ShouldHaveValidationErrorFor(command => command.Plant.CommonName);
        result.ShouldHaveValidationErrorFor(command => command.Plant.Family);
        result.ShouldHaveValidationErrorFor(command => command.Plant.Category);
        result.ShouldHaveValidationErrorFor(command => command.Plant.Description);
        result.ShouldHaveValidationErrorFor(command => command.Plant.IdentifyingCharacteristics);
    }

    private static PlantInputDto CreatePlant() => new(
        "Rosa rugosa",
        "Rugosa rose",
        [],
        "Rosaceae",
        PlantType.Flowering,
        "A hardy flowering shrub.",
        "Thorny arching stems and fragrant pink flowers.");
}
