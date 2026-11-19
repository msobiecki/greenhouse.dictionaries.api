namespace Greenhouse.Dictionaries.Domain.Exceptions;

public sealed class PlantNotFoundException : BaseException
{
    public int PlantId { get; }

    public PlantNotFoundException(int plantId)
        : base($"Plant with ID '{plantId}' was not found.")
    {
        PlantId = plantId;
    }

}
