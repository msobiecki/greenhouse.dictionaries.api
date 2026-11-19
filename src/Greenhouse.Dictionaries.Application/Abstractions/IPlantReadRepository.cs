using Greenhouse.Dictionaries.Domain.Entities;
using Greenhouse.Dictionaries.Application.DTOs;
using Greenhouse.Dictionaries.Application.Configuration;

namespace Greenhouse.Dictionaries.Application.Abstractions;

public interface IPlantReadRepository
{
    Task<PagedResult<Plant>> GetAllAsync(
        string? search = null,
        PlantSearchFilters? filters = null,
        int offset = 0,
        int limit = 50,
        string language = LanguageConfiguration.DefaultCulture,
        bool onlyRequestedLanguage = false,
        CancellationToken cancellationToken = default);

    Task<Plant?> GetByUidAsync(Guid uid, CancellationToken cancellationToken = default);

    Task<bool> IsAlreadyExistAsync(Guid uid, CancellationToken cancellationToken = default);
}
