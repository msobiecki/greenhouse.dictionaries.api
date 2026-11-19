using Greenhouse.Dictionaries.Application.Configuration.Queries;
using Greenhouse.Dictionaries.Application.Configuration;
using Greenhouse.Dictionaries.Application.Abstractions;
using Greenhouse.Dictionaries.Application.DTOs;

namespace Greenhouse.Dictionaries.Application.Queries;

public sealed record GetPlantsQuery(
	string? Search,
	PlantSearchFilters? Filters = null,
	string Language = LanguageConfiguration.DefaultCulture) : IQuery<IEnumerable<PlantDto>>;
