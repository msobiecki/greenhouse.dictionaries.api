namespace Greenhouse.Dictionaries.Domain.Exceptions;

public sealed class PlantAlreadyExistsException : BaseException
{
    public int PlantId { get; }

    public PlantAlreadyExistsException(int plantId)
        : base($"Plant with ID '{plantId}' already exists.")
    {
        PlantId = plantId;
    }

}
