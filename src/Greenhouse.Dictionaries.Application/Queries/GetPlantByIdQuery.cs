using Greenhouse.Dictionaries.Application.Configuration.Queries;
using Greenhouse.Dictionaries.Application.Configuration;
using Greenhouse.Dictionaries.Application.DTOs;

namespace Greenhouse.Dictionaries.Application.Queries;

public sealed record GetPlantByIdQuery(int Id, string Language = LanguageConfiguration.DefaultCulture) : IQuery<PlantDto?>;
