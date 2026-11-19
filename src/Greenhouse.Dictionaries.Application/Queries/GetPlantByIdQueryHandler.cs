using Greenhouse.Dictionaries.Application.Configuration.Queries;
using Greenhouse.Dictionaries.Application.DTOs;
using Greenhouse.Dictionaries.Application.Configuration.Mappings;
using Greenhouse.Dictionaries.Application.Abstractions;
using AutoMapper;

namespace Greenhouse.Dictionaries.Application.Queries;

public sealed class GetPlantByIdQueryHandler(IPlantReadRepository plantRepository, IMapper mapper)
    : IQueryHandler<GetPlantByIdQuery, PlantDto?>
{
    public async Task<PlantDto?> Handle(GetPlantByIdQuery request, CancellationToken cancellationToken)
    {
        var plant = await plantRepository.GetByUidAsync(request.Uid, cancellationToken);
        return plant is null ? null : PlantLocalization.ToDto(mapper, plant, request.Language);
    }
}
