using Greenhouse.Dictionaries.Application.Configuration.Commands;

namespace Greenhouse.Dictionaries.Application.Commands;

public sealed record DeletePlantCommand(int Id) : ICommand<bool>;
