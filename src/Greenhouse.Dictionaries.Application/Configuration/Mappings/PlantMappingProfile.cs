using AutoMapper;

using Greenhouse.Dictionaries.Application.DTOs;
using Greenhouse.Dictionaries.Domain.Entities;

namespace Greenhouse.Dictionaries.Application.Configuration.Mappings;

public sealed class PlantMappingProfile : Profile
{
    public PlantMappingProfile()
    {
        CreateMap<Plant, PlantDto>();
        CreateMap<PlantInputDto, Plant>();
    }
}
