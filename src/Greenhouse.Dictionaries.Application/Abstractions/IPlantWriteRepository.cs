using Greenhouse.Dictionaries.Domain.Entities;

namespace Greenhouse.Dictionaries.Application.Abstractions;

public interface IPlantWriteRepository
{
    Task AddAsync(Plant plant, CancellationToken cancellationToken = default);

    Task UpdateAsync(Plant plant, CancellationToken cancellationToken = default);

    Task DeleteAsync(Plant plant, CancellationToken cancellationToken = default);
}
