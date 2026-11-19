using Greenhouse.Dictionaries.Domain.Entities;
using Greenhouse.Dictionaries.Infrastructure.Config;
using Microsoft.EntityFrameworkCore;

namespace Greenhouse.Dictionaries.Infrastructure.Context;

public sealed class DatabaseContext : DbContext
{
    public DbSet<Plant> Plants => Set<Plant>();
    public DbSet<PlantTranslation> PlantTranslations => Set<PlantTranslation>();
    public DbSet<PlantAlias> PlantAliases => Set<PlantAlias>();
    public DbSet<PlantImage> PlantImages => Set<PlantImage>();
    public DbSet<PlantToxicityAssessment> PlantToxicityAssessments => Set<PlantToxicityAssessment>();
    public DbSet<PlantToxicitySource> PlantToxicitySources => Set<PlantToxicitySource>();

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
        modelBuilder.Entity<PlantTranslation>().ToTable("PlantTranslations");
        modelBuilder.Entity<PlantTranslation>().Property(translation => translation.Language).HasMaxLength(10).IsRequired();
        modelBuilder.Entity<PlantTranslation>().Property(translation => translation.CommonName).HasMaxLength(500).IsRequired();
        modelBuilder.Entity<PlantTranslation>().Property(translation => translation.AlternativeNamesJson).IsRequired();
        modelBuilder.Entity<PlantTranslation>().Property(translation => translation.Description).IsRequired();
        modelBuilder.Entity<PlantTranslation>().Property(translation => translation.IdentifyingCharacteristics).IsRequired();
        modelBuilder.Entity<PlantTranslation>().Property(translation => translation.ToxicityDetailsJson);
        modelBuilder.Entity<PlantTranslation>().Property(translation => translation.EnvironmentalNotes).HasMaxLength(2000);
        modelBuilder.Entity<PlantTranslation>().Property(translation => translation.DetailedDescription).HasMaxLength(5000);
        modelBuilder.Entity<PlantTranslation>().Property(translation => translation.GrowthHabit).HasMaxLength(200);
        modelBuilder.Entity<PlantTranslation>().Property(translation => translation.FoliageDescription).HasMaxLength(2000);
        modelBuilder.Entity<PlantTranslation>().Property(translation => translation.FlowerDescription).HasMaxLength(2000);
        modelBuilder.Entity<PlantTranslation>().Property(translation => translation.FloweringPeriod).HasMaxLength(200);
        modelBuilder.Entity<PlantTranslation>().Property(translation => translation.FruitDescription).HasMaxLength(2000);
        modelBuilder.Entity<PlantTranslation>().Property(translation => translation.WateringInstructions).HasMaxLength(2000);
        modelBuilder.Entity<PlantTranslation>().Property(translation => translation.SeasonalWateringNotes).HasMaxLength(2000);
        modelBuilder.Entity<PlantTranslation>().Property(translation => translation.FertilizingInstructions).HasMaxLength(2000);
        modelBuilder.Entity<PlantTranslation>().Property(translation => translation.PruningInstructions).HasMaxLength(2000);
        modelBuilder.Entity<PlantTranslation>().Property(translation => translation.RepottingInstructions).HasMaxLength(2000);
        modelBuilder.Entity<PlantTranslation>().HasIndex(translation => new { translation.PlantId, translation.Language }).IsUnique();
        modelBuilder.Entity<PlantAlias>().ToTable("PlantAliases");
        modelBuilder.Entity<PlantAlias>().Property(alias => alias.Name).HasMaxLength(200).IsRequired();
        modelBuilder.Entity<PlantImage>().ToTable("PlantImages");
        modelBuilder.Entity<PlantImage>().Property(image => image.Url).HasMaxLength(2048).IsRequired();
        modelBuilder.Entity<PlantImage>().Property(image => image.Source).HasMaxLength(500).IsRequired();
        modelBuilder.Entity<PlantImage>().Property(image => image.License).HasMaxLength(200).IsRequired();
        modelBuilder.Entity<PlantImage>().Property(image => image.Attribution).HasMaxLength(500);
        modelBuilder.Entity<PlantToxicityAssessment>().ToTable("PlantToxicityAssessments");
        modelBuilder.Entity<PlantToxicityAssessment>()
            .Property(assessment => assessment.TargetCode).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<PlantToxicityAssessment>()
            .Property(assessment => assessment.Level).HasConversion<string>().HasMaxLength(30).IsRequired();
        modelBuilder.Entity<PlantToxicityAssessment>()
            .Property(assessment => assessment.Details).HasMaxLength(2000);
        modelBuilder.Entity<PlantToxicityAssessment>()
            .HasIndex(assessment => new { assessment.PlantId, assessment.TargetCode }).IsUnique();
        modelBuilder.Entity<PlantToxicityAssessment>()
            .HasMany(assessment => assessment.Sources)
            .WithOne()
            .HasForeignKey(source => source.PlantToxicityAssessmentId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PlantToxicitySource>().ToTable("PlantToxicitySources");
        modelBuilder.Entity<PlantToxicitySource>().Property(source => source.Url).HasMaxLength(2048).IsRequired();
        modelBuilder.Entity<PlantToxicitySource>().Property(source => source.Publisher).HasMaxLength(500);
    }
}
