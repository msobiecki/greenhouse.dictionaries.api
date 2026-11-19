using Greenhouse.Dictionaries.Domain.Entities;
using Greenhouse.Dictionaries.Infrastructure.Config;
using Microsoft.EntityFrameworkCore;

namespace Greenhouse.Dictionaries.Infrastructure.Context;

public sealed class DatabaseContext : DbContext
{
    public DbSet<Plant> Plants => Set<Plant>();

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (!Database.IsSqlite())
        {
            modelBuilder.HasDefaultSchema("greenhouse");
        }

        modelBuilder.ApplyConfiguration(new PlantConfiguration());
    }
}
