using Greenhouse.Dictionaries.Application.Configuration.Commands;
using Greenhouse.Dictionaries.Application.DTOs;
using Greenhouse.Dictionaries.Application.Configuration.Mappings;
using Greenhouse.Dictionaries.Application.Abstractions;
using Greenhouse.Dictionaries.Domain.Exceptions;
using AutoMapper;

namespace Greenhouse.Dictionaries.Application.Commands;

public sealed class UpdatePlantCommandHandler(
    IPlantWriteRepository plantWriteRepository,
    IPlantReadRepository plantReadRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper)
    : ICommandHandler<UpdatePlantCommand, PlantDto>
{
    public async Task<PlantDto> Handle(UpdatePlantCommand request, CancellationToken cancellationToken)
    {
        var plant = await plantReadRepository.GetByIdAsync(request.Id, cancellationToken) ?? throw new PlantNotFoundException(request.Id);
        mapper.Map(request.Plant, plant);
        var translation = plant.Translations.FirstOrDefault(item =>
            string.Equals(item.Language, request.Language, StringComparison.OrdinalIgnoreCase));
        if (translation is null)
        {
            translation = PlantLocalization.CreateTranslation(mapper, request.Plant, request.Language);
            plant.Translations.Add(translation);
        }
        else
        {
            PlantLocalization.UpdateTranslation(mapper, translation, request.Plant, request.Language);
        }
        await plantWriteRepository.UpdateAsync(plant, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return PlantLocalization.ToDto(mapper, plant, request.Language);
    }
}
