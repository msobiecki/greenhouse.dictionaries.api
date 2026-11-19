using Greenhouse.Dictionaries.Application.Configuration.Queries;
using Greenhouse.Dictionaries.Application.DTOs;

namespace Greenhouse.Dictionaries.Application.Queries;

public sealed record GetPlantsQuery(string? Search, string? Category) : IQuery<IEnumerable<PlantDto>>;
