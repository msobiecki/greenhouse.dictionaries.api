using Greenhouse.Dictionaries.Domain.Abstractions;

namespace Greenhouse.Dictionaries.Domain.Entities;

public abstract class Entity : IAuditableEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
