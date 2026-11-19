using FluentValidation;
using Greenhouse.Dictionaries.Domain.Entities;

namespace Greenhouse.Dictionaries.Application.Commands;

public sealed class UpdatePlantCommandValidation : AbstractValidator<UpdatePlantCommand>
{
    public UpdatePlantCommandValidation()
    {
        RuleFor(command => command.Id).GreaterThan(0);
        RuleFor(command => command.Plant.ScientificName).NotEmpty();
        RuleFor(command => command.Plant.CommonName).NotEmpty();
        RuleFor(command => command.Plant.Family).NotEmpty();
        RuleFor(command => command.Plant.Category)
            .IsInEnum()
            .Must(category => category != PlantType.Unknown)
            .WithMessage("A plant category is required.");
        RuleFor(command => command.Plant.Description).NotEmpty();
        RuleFor(command => command.Plant.IdentifyingCharacteristics).NotEmpty();
        RuleFor(command => command.Plant.TemperatureMinC).InclusiveBetween(-50, 60)
            .When(command => command.Plant.TemperatureMinC.HasValue);
        RuleFor(command => command.Plant.TemperatureMaxC).InclusiveBetween(-50, 60)
            .When(command => command.Plant.TemperatureMaxC.HasValue);
        RuleFor(command => command.Plant)
            .Must(plant => !plant.TemperatureMinC.HasValue || !plant.TemperatureMaxC.HasValue ||
                plant.TemperatureMinC <= plant.TemperatureMaxC)
            .WithMessage("Minimum temperature must not exceed maximum temperature.");
        RuleFor(command => command.Plant.HumidityMinPercent).InclusiveBetween(0, 100)
            .When(command => command.Plant.HumidityMinPercent.HasValue);
        RuleFor(command => command.Plant.HumidityMaxPercent).InclusiveBetween(0, 100)
            .When(command => command.Plant.HumidityMaxPercent.HasValue);
        RuleFor(command => command.Plant)
            .Must(plant => !plant.HumidityMinPercent.HasValue || !plant.HumidityMaxPercent.HasValue ||
                plant.HumidityMinPercent <= plant.HumidityMaxPercent)
            .WithMessage("Minimum humidity must not exceed maximum humidity.");
        RuleFor(command => command.Plant.EnvironmentalNotes).MaximumLength(2000);
        RuleFor(command => command.Plant.SunlightRequirement).IsInEnum()
            .When(command => command.Plant.SunlightRequirement.HasValue);
        RuleFor(command => command.Plant.SunlightHoursMin).InclusiveBetween(0, 24)
            .When(command => command.Plant.SunlightHoursMin.HasValue);
        RuleFor(command => command.Plant.SunlightHoursMax).InclusiveBetween(0, 24)
            .When(command => command.Plant.SunlightHoursMax.HasValue);
        RuleFor(command => command.Plant)
            .Must(plant => !plant.SunlightHoursMin.HasValue || !plant.SunlightHoursMax.HasValue ||
                plant.SunlightHoursMin <= plant.SunlightHoursMax)
            .WithMessage("Minimum sunlight hours must not exceed maximum sunlight hours.");
        RuleFor(command => command.Plant.WateringRequirement).IsInEnum()
            .When(command => command.Plant.WateringRequirement.HasValue);
        RuleFor(command => command.Plant.WateringInstructions).MaximumLength(2000);
        RuleFor(command => command.Plant.SeasonalWateringNotes).MaximumLength(2000);
        RuleFor(command => command.Plant.FertilizingInstructions).MaximumLength(2000);
        RuleFor(command => command.Plant.PruningInstructions).MaximumLength(2000);
        RuleFor(command => command.Plant.RepottingInstructions).MaximumLength(2000);
        RuleFor(command => command.Plant.MatureHeightCm).GreaterThan(0).When(command => command.Plant.MatureHeightCm.HasValue);
        RuleFor(command => command.Plant.MatureWidthCm).GreaterThan(0).When(command => command.Plant.MatureWidthCm.HasValue);
        RuleFor(command => command.Plant.DetailedDescription).MaximumLength(5000);
        RuleFor(command => command.Plant.GrowthHabit).MaximumLength(200);
        RuleFor(command => command.Plant.FoliageDescription).MaximumLength(2000);
        RuleFor(command => command.Plant.FlowerDescription).MaximumLength(2000);
        RuleFor(command => command.Plant.FloweringPeriod).MaximumLength(200);
        RuleFor(command => command.Plant.FruitDescription).MaximumLength(2000);
        RuleFor(command => command.Plant.LifeCycle).IsInEnum()
            .When(command => command.Plant.LifeCycle.HasValue);
        RuleForEach(command => command.Plant.AlternativeNames).NotEmpty().MaximumLength(200);
        RuleForEach(command => command.Plant.Images).ChildRules(image =>
        {
            image.RuleFor(item => item.Url).NotEmpty().MaximumLength(2048).Must(BeHttpUrl)
                .WithMessage("Image URL must be an absolute HTTP or HTTPS URL.");
            image.RuleFor(item => item.Source).NotEmpty().MaximumLength(500);
            image.RuleFor(item => item.License).NotEmpty().MaximumLength(200);
            image.RuleFor(item => item.Attribution).MaximumLength(500);
        });
        RuleFor(command => command.Plant.ToxicityAssessments)
            .Must(assessments => assessments is null ||
                assessments.Select(assessment => assessment.TargetCode).Distinct(StringComparer.Ordinal).Count() == assessments.Length)
            .WithMessage("Each toxicity target must be unique.");
        RuleForEach(command => command.Plant.ToxicityAssessments).ChildRules(assessment =>
        {
            assessment.RuleFor(item => item.TargetCode)
                .NotEmpty()
                .MaximumLength(100)
                .Matches("^[a-z0-9][a-z0-9-]*$");
            assessment.RuleFor(item => item.Level).IsInEnum();
            assessment.RuleFor(item => item.Details).MaximumLength(2000);
            assessment.RuleForEach(item => item.Sources).ChildRules(source =>
            {
                source.RuleFor(item => item.Url).NotEmpty().MaximumLength(2048).Must(BeHttpUrl)
                    .WithMessage("Toxicity source URL must be an absolute HTTP or HTTPS URL.");
                source.RuleFor(item => item.Publisher).MaximumLength(500);
            });
        });
    }

    private static bool BeHttpUrl(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
