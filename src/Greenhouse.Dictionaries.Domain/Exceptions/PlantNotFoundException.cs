namespace Greenhouse.Dictionaries.Domain.Exceptions;

public sealed class PlantNotFoundException : BaseException
{
    public Guid PlantUid { get; }

    public PlantNotFoundException(Guid plantUid)
        : base($"Plant with UID '{plantUid}' was not found.")
    {
        PlantUid = plantUid;
    }

}
