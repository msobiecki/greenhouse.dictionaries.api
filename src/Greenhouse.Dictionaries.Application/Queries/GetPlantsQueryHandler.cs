using Greenhouse.Dictionaries.Application.Configuration.Queries;
using Greenhouse.Dictionaries.Application.DTOs;
using Greenhouse.Dictionaries.Application.Abstractions;
using AutoMapper;

namespace Greenhouse.Dictionaries.Application.Queries;

public sealed class GetPlantsQueryHandler(IPlantReadRepository plantRepository, IMapper mapper) : IQueryHandler<GetPlantsQuery, IEnumerable<PlantDto>>
{
    public async Task<IEnumerable<PlantDto>> Handle(GetPlantsQuery request, CancellationToken cancellationToken)
    {
        var plants = await plantRepository.GetAllAsync(request.Search, request.Category, cancellationToken);
        return plants.Select(plant => mapper.Map<PlantDto>(plant)).ToArray();
    }
}
