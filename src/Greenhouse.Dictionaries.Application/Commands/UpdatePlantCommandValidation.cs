using FluentValidation;

namespace Greenhouse.Dictionaries.Application.Commands;

public sealed class UpdatePlantCommandValidation : AbstractValidator<UpdatePlantCommand>
{
    public UpdatePlantCommandValidation()
    {
        RuleFor(command => command.Id).GreaterThan(0);
        RuleFor(command => command.Plant.ScientificName).NotEmpty();
        RuleFor(command => command.Plant.CommonName).NotEmpty();
        RuleFor(command => command.Plant.Family).NotEmpty();
        RuleFor(command => command.Plant.Category).NotEmpty();
        RuleFor(command => command.Plant.Description).NotEmpty();
        RuleFor(command => command.Plant.Sunlight).NotEmpty();
        RuleFor(command => command.Plant.Watering).NotEmpty();
    }
}
