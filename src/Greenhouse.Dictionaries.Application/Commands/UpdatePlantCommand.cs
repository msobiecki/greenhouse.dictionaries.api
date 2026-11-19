using Greenhouse.Dictionaries.Application.Configuration.Commands;
using Greenhouse.Dictionaries.Application.Configuration;
using Greenhouse.Dictionaries.Application.DTOs;

namespace Greenhouse.Dictionaries.Application.Commands;

public sealed record UpdatePlantCommand(Guid Uid, PlantInputDto Plant, string Language = LanguageConfiguration.DefaultCulture) : ICommand<PlantDto>;
