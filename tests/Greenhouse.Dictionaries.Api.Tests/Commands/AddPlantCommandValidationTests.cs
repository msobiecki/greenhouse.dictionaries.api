using FluentValidation.TestHelper;
using Greenhouse.Dictionaries.Application.Commands;
using Greenhouse.Dictionaries.Application.DTOs;
using Xunit;

namespace Greenhouse.Dictionaries.Api.Tests.Commands;

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

    private static PlantInputDto CreatePlant() => new(
        "Rosa rugosa",
        "Rugosa rose",
        "Rosaceae",
        "flower",
        "A hardy flowering shrub.",
        "Full sun",
        "Water during dry periods.");
}
