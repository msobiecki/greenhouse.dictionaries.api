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
        builder.Property(p => p.Category).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(p => p.Family).IsRequired();
        builder.Property(p => p.Description).IsRequired();
        builder.Property(p => p.IdentifyingCharacteristics).IsRequired();
        builder.Property(p => p.NativeRange);
        builder.Property(p => p.Habitat);
        builder.Property(p => p.TemperatureMinC).HasPrecision(4, 1);
        builder.Property(p => p.TemperatureMaxC).HasPrecision(4, 1);
        builder.Property(p => p.HumidityMinPercent).HasPrecision(5, 2);
        builder.Property(p => p.HumidityMaxPercent).HasPrecision(5, 2);
        builder.Property(p => p.EnvironmentalNotes).HasMaxLength(2000);
        builder.Property(p => p.SunlightRequirement).HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.SunlightHoursMin).HasPrecision(4, 2);
        builder.Property(p => p.SunlightHoursMax).HasPrecision(4, 2);
        builder.Property(p => p.WateringRequirement).HasConversion<string>().HasMaxLength(40);
        builder.Property(p => p.WateringInstructions).HasMaxLength(2000);
        builder.Property(p => p.SeasonalWateringNotes).HasMaxLength(2000);
        builder.Property(p => p.FertilizingInstructions).HasMaxLength(2000);
        builder.Property(p => p.PruningInstructions).HasMaxLength(2000);
        builder.Property(p => p.RepottingInstructions).HasMaxLength(2000);
        builder.Property(p => p.MatureHeightCm).HasPrecision(8, 2);
        builder.Property(p => p.MatureWidthCm).HasPrecision(8, 2);
        builder.Property(p => p.DetailedDescription).HasMaxLength(5000);
        builder.Property(p => p.GrowthHabit).HasMaxLength(200);
        builder.Property(p => p.FoliageDescription).HasMaxLength(2000);
        builder.Property(p => p.FlowerDescription).HasMaxLength(2000);
        builder.Property(p => p.FloweringPeriod).HasMaxLength(200);
        builder.Property(p => p.FruitDescription).HasMaxLength(2000);
        builder.Property(p => p.LifeCycle).HasConversion<string>().HasMaxLength(30);

        builder.HasMany(p => p.AlternativeNames)
            .WithOne()
            .HasForeignKey(alias => alias.PlantId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Images)
            .WithOne()
            .HasForeignKey(image => image.PlantId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.ToxicityAssessments)
            .WithOne()
            .HasForeignKey(assessment => assessment.PlantId)
            .OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
