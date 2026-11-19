using Greenhouse.Dictionaries.Application.Abstractions;
using Greenhouse.Dictionaries.Domain.Abstractions;
using Greenhouse.Dictionaries.Infrastructure.Context;

namespace Greenhouse.Dictionaries.Infrastructure;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly DatabaseContext dbContext;

    public UnitOfWork(DatabaseContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntries();
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public void UpdateAuditableEntries()
    {
        var entries = dbContext.ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditableEntity && (e.State == Microsoft.EntityFrameworkCore.EntityState.Added || e.State == Microsoft.EntityFrameworkCore.EntityState.Modified));

        foreach (var entry in entries)
        {
            var auditableEntity = (IAuditableEntity)entry.Entity;
            if (entry.State == Microsoft.EntityFrameworkCore.EntityState.Added)
            {
                auditableEntity.CreatedAt = DateTime.UtcNow;
            }
            auditableEntity.UpdatedAt = DateTime.UtcNow;
        }
    }
}
