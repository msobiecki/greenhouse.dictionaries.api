using System.Text.Json;
using AutoMapper;
using Greenhouse.Dictionaries.Application.DTOs;
using Greenhouse.Dictionaries.Application.Configuration;
using Greenhouse.Dictionaries.Domain.Entities;

namespace Greenhouse.Dictionaries.Application.Configuration.Mappings;

public static class PlantLocalization
{
    public static PlantDto ToDto(IMapper mapper, Plant plant, string requestedLanguage)
    {
        var translation = SelectTranslation(plant, requestedLanguage);
        var dto = mapper.Map<PlantDto>(plant);
        var toxicityDetails = translation.ToxicityDetailsJson is null
            ? null
            : JsonSerializer.Deserialize<Dictionary<string, string?>>(translation.ToxicityDetailsJson);
        return dto with
        {
            Language = translation.Language,
            CommonName = translation.CommonName,
            AlternativeNames = JsonSerializer.Deserialize<string[]>(translation.AlternativeNamesJson) ?? [],
            Description = translation.Description,
            IdentifyingCharacteristics = translation.IdentifyingCharacteristics,
            NativeRange = translation.NativeRange,
            Habitat = translation.Habitat,
            EnvironmentalNotes = translation.EnvironmentalNotes,
            DetailedDescription = translation.DetailedDescription,
            GrowthHabit = translation.GrowthHabit,
            FoliageDescription = translation.FoliageDescription,
            FlowerDescription = translation.FlowerDescription,
            FloweringPeriod = translation.FloweringPeriod,
            FruitDescription = translation.FruitDescription,
            WateringInstructions = translation.WateringInstructions,
            SeasonalWateringNotes = translation.SeasonalWateringNotes,
            FertilizingInstructions = translation.FertilizingInstructions,
            PruningInstructions = translation.PruningInstructions,
            RepottingInstructions = translation.RepottingInstructions,
            ToxicityAssessments = toxicityDetails is null
                ? dto.ToxicityAssessments
                : dto.ToxicityAssessments.Select(assessment =>
                    toxicityDetails.TryGetValue(assessment.TargetCode, out var details)
                        ? assessment with { Details = details }
                        : assessment).ToArray()
        };
    }

    public static PlantTranslation CreateTranslation(
        IMapper mapper,
        PlantInputDto input,
        string language)
    {
        var translation = mapper.Map<PlantTranslation>(input);
        UpdateLocalizedToxicityDetails(translation, input);
        translation.Language = language;
        return translation;
    }

    public static void UpdateTranslation(
        IMapper mapper,
        PlantTranslation translation,
        PlantInputDto input,
        string language)
    {
        mapper.Map(input, translation);
        UpdateLocalizedToxicityDetails(translation, input);
        translation.Language = language;
    }

    public static PlantTranslation SelectTranslation(Plant plant, string requestedLanguage) =>
        plant.Translations.FirstOrDefault(translation =>
            string.Equals(translation.Language, requestedLanguage, StringComparison.OrdinalIgnoreCase)) ??
        plant.Translations.FirstOrDefault(translation =>
            string.Equals(translation.Language, LanguageConfiguration.DefaultCulture, StringComparison.OrdinalIgnoreCase)) ??
        plant.Translations.FirstOrDefault() ?? CreateLegacyTranslation(plant);

    private static PlantTranslation CreateLegacyTranslation(Plant plant) => new()
    {
        PlantId = plant.Id,
        Language = LanguageConfiguration.DefaultCulture,
        CommonName = plant.CommonName,
        AlternativeNamesJson = JsonSerializer.Serialize(plant.AlternativeNames.Select(alias => alias.Name)),
        Description = plant.Description,
        IdentifyingCharacteristics = plant.IdentifyingCharacteristics,
        NativeRange = plant.NativeRange,
        Habitat = plant.Habitat,
        EnvironmentalNotes = plant.EnvironmentalNotes,
        DetailedDescription = plant.DetailedDescription,
        GrowthHabit = plant.GrowthHabit,
        FoliageDescription = plant.FoliageDescription,
        FlowerDescription = plant.FlowerDescription,
        FloweringPeriod = plant.FloweringPeriod,
        FruitDescription = plant.FruitDescription,
        WateringInstructions = plant.WateringInstructions,
        SeasonalWateringNotes = plant.SeasonalWateringNotes,
        FertilizingInstructions = plant.FertilizingInstructions,
        PruningInstructions = plant.PruningInstructions,
        RepottingInstructions = plant.RepottingInstructions
    };

    private static void UpdateLocalizedToxicityDetails(PlantTranslation translation, PlantInputDto input) =>
        translation.ToxicityDetailsJson = JsonSerializer.Serialize(
            (input.ToxicityAssessments ?? [])
                .ToDictionary(assessment => assessment.TargetCode, assessment => assessment.Details));
}
