using Greenhouse.Dictionaries.Application.Configuration.Commands;
using Greenhouse.Dictionaries.Application.DTOs;
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
        var plant = await plantReadRepository.GetByIdAsync(request.Id, cancellationToken);
        if (plant is null)
        {
            throw new PlantNotFoundException(request.Id);
        }

        mapper.Map(request.Plant, plant);
        await plantWriteRepository.UpdateAsync(plant, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return mapper.Map<PlantDto>(plant);
    }
}
