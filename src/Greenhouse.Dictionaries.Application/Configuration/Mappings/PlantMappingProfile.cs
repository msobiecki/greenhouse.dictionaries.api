using AutoMapper;
using System.Text.Json;

using Greenhouse.Dictionaries.Application.DTOs;
using Greenhouse.Dictionaries.Domain.Entities;

namespace Greenhouse.Dictionaries.Application.Configuration.Mappings;

public sealed class PlantMappingProfile : Profile
{
    public PlantMappingProfile()
    {
        CreateMap<string, PlantAlias>()
            .ForMember(alias => alias.Name, options => options.MapFrom(name => name));
        CreateMap<PlantAlias, string>().ConvertUsing(alias => alias.Name);
        CreateMap<PlantImageInputDto, PlantImage>();
        CreateMap<PlantImage, PlantImageDto>();
        CreateMap<PlantToxicitySourceInputDto, PlantToxicitySource>();
        CreateMap<PlantToxicitySource, PlantToxicitySourceDto>();
        CreateMap<PlantToxicityAssessmentInputDto, PlantToxicityAssessment>()
            .ForMember(assessment => assessment.Sources,
                options => options.MapFrom(dto => dto.Sources ?? Array.Empty<PlantToxicitySourceInputDto>()));
        CreateMap<PlantToxicityAssessment, PlantToxicityAssessmentDto>()
            .ForMember(dto => dto.Sources,
                options => options.MapFrom(assessment => assessment.Sources));
        CreateMap<Plant, PlantDto>()
            .ForMember(dto => dto.AlternativeNames,
                options => options.MapFrom(plant => plant.AlternativeNames));
        CreateMap<PlantInputDto, PlantTranslation>()
            .ForMember(translation => translation.AlternativeNamesJson,
                options => options.MapFrom(dto => JsonSerializer.Serialize(dto.AlternativeNames ?? Array.Empty<string>())));
        CreateMap<PlantInputDto, Plant>()
            .ForMember(plant => plant.AlternativeNames,
                options => options.MapFrom(dto => dto.AlternativeNames ?? Array.Empty<string>()))
            .ForMember(plant => plant.ToxicityAssessments,
                options => options.MapFrom(dto => dto.ToxicityAssessments ?? Array.Empty<PlantToxicityAssessmentInputDto>()))
            .ForMember(plant => plant.Images,
                options => options.MapFrom(dto => dto.Images ?? Array.Empty<PlantImageInputDto>()));
    }
}
