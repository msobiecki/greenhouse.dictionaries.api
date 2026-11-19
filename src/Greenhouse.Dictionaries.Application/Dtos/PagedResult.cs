namespace Greenhouse.Dictionaries.Application.DTOs;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Offset,
    int Limit,
    int TotalCount)
{
    public bool HasPreviousPage => Offset > 0 && TotalCount > 0;
    public bool HasNextPage => Offset + (long)Items.Count < TotalCount;
    public int? NextOffset => HasNextPage ? Offset + Items.Count : null;
}
