using Greenhouse.Dictionaries.Application.Configuration.Commands;
using Greenhouse.Dictionaries.Application.Abstractions;
using Greenhouse.Dictionaries.Domain.Exceptions;

namespace Greenhouse.Dictionaries.Application.Commands;

public sealed class DeletePlantCommandHandler(
    IPlantWriteRepository plantWriteRepository,
    IPlantReadRepository plantReadRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeletePlantCommand, bool>
{
    public async Task<bool> Handle(DeletePlantCommand request, CancellationToken cancellationToken)
    {
        var plant = await plantReadRepository.GetByIdAsync(request.Id, cancellationToken) ?? throw new PlantNotFoundException(request.Id);
        await plantWriteRepository.DeleteAsync(plant, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
