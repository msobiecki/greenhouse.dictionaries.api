using Greenhouse.Dictionaries.Application.Configuration.Queries;
using Greenhouse.Dictionaries.Application.DTOs;
using Greenhouse.Dictionaries.Application.Configuration.Mappings;
using Greenhouse.Dictionaries.Application.Abstractions;
using AutoMapper;

namespace Greenhouse.Dictionaries.Application.Queries;

public sealed class GetPlantsQueryHandler(IPlantReadRepository plantRepository, IMapper mapper) : IQueryHandler<GetPlantsQuery, PagedResult<PlantDto>>
{
    public async Task<PagedResult<PlantDto>> Handle(GetPlantsQuery request, CancellationToken cancellationToken)
    {
        var plants = await plantRepository.GetAllAsync(
            request.Search,
            request.Filters,
            request.Offset,
            request.Limit,
            request.Language,
            request.OnlyRequestedLanguage,
            cancellationToken);
        var items = plants.Items
            .Select(plant => PlantLocalization.ToDto(mapper, plant, request.Language))
            .ToArray();
        return new PagedResult<PlantDto>(items, plants.Offset, plants.Limit, plants.TotalCount);
    }
}
