using Greenhouse.Dictionaries.Domain.Entities;

namespace Greenhouse.Dictionaries.Application.Abstractions;

public interface IPlantReadRepository
{
    Task<IEnumerable<Plant>> GetAllAsync(
        string? search = null,
        string? category = null,
        CancellationToken cancellationToken = default);

    Task<Plant?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> IsAlreadyExistAsync(int id, CancellationToken cancellationToken = default);
}
