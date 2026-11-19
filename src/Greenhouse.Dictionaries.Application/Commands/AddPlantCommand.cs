using Greenhouse.Dictionaries.Application.Configuration.Commands;
using Greenhouse.Dictionaries.Application.DTOs;

namespace Greenhouse.Dictionaries.Application.Commands;

public sealed record AddPlantCommand(PlantInputDto Plant) : ICommand<PlantDto>;
