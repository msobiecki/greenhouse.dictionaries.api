using Greenhouse.Dictionaries.Application.Configuration.Commands;
using Greenhouse.Dictionaries.Application.DTOs;
using Greenhouse.Dictionaries.Application.Configuration.Mappings;
using Greenhouse.Dictionaries.Application.Abstractions;
using Greenhouse.Dictionaries.Domain.Entities;
using AutoMapper;

namespace Greenhouse.Dictionaries.Application.Commands;

public sealed class AddPlantCommandHandler(
    IPlantWriteRepository plantRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : ICommandHandler<AddPlantCommand, PlantDto>
{
    public async Task<PlantDto> Handle(AddPlantCommand request, CancellationToken cancellationToken)
    {
        var plant = mapper.Map<Plant>(request.Plant);
        var translation = PlantLocalization.CreateTranslation(mapper, request.Plant, request.Language);
        plant.Translations.Add(translation);
        await plantRepository.AddAsync(plant, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return PlantLocalization.ToDto(mapper, plant, request.Language);
    }
}
