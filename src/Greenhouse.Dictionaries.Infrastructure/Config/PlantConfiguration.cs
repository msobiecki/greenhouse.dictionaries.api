using Greenhouse.Dictionaries.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Greenhouse.Dictionaries.Infrastructure.Config;

public class PlantConfiguration : BaseEntityConfiguration<Plant>
{
    public override void Configure(EntityTypeBuilder<Plant> builder)
    {
        builder.ToTable("Plants");  

        builder.Property(p => p.ScientificName).IsRequired();
        builder.Property(p => p.CommonName).IsRequired();
        builder.Property(p => p.Family).IsRequired();
        builder.Property(p => p.Category).IsRequired();
        builder.Property(p => p.Description).IsRequired();
        builder.Property(p => p.Sunlight).IsRequired();
        builder.Property(p => p.Watering).IsRequired();
        builder.Property(p => p.FlowerColor);
        
        base.Configure(builder);
    }
}
