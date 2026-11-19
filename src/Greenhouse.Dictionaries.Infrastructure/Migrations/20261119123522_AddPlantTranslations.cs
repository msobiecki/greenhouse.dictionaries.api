using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Greenhouse.Dictionaries.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlantTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                migrationBuilder.CreateTable(
                    name: "PlantTranslations",
                    columns: table => new
                    {
                        Id = table.Column<int>(type: "INTEGER", nullable: false)
                            .Annotation("Sqlite:Autoincrement", true),
                        PlantId = table.Column<int>(type: "INTEGER", nullable: false),
                        Language = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                        CommonName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                        AlternativeNamesJson = table.Column<string>(type: "TEXT", nullable: false),
                        Description = table.Column<string>(type: "TEXT", nullable: false),
                        IdentifyingCharacteristics = table.Column<string>(type: "TEXT", nullable: false),
                        NativeRange = table.Column<string>(type: "TEXT", nullable: true),
                        Habitat = table.Column<string>(type: "TEXT", nullable: true),
                        EnvironmentalNotes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                        DetailedDescription = table.Column<string>(type: "TEXT", maxLength: 5000, nullable: true),
                        GrowthHabit = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                        FoliageDescription = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                        FlowerDescription = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                        FloweringPeriod = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                        FruitDescription = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                        WateringInstructions = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                        SeasonalWateringNotes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                        FertilizingInstructions = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                        PruningInstructions = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                        RepottingInstructions = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_PlantTranslations", translation => translation.Id);
                        table.ForeignKey(
                            name: "FK_PlantTranslations_Plants_PlantId",
                            column: translation => translation.PlantId,
                            principalTable: "Plants",
                            principalColumn: "Id",
                            onDelete: ReferentialAction.Cascade);
                    });

                migrationBuilder.CreateIndex(
                    name: "IX_PlantTranslations_PlantId_Language",
                    table: "PlantTranslations",
                    columns: new[] { "PlantId", "Language" },
                    unique: true);

                migrationBuilder.Sql("""
                    INSERT INTO "PlantTranslations" (
                        "PlantId", "Language", "CommonName", "AlternativeNamesJson", "Description",
                        "IdentifyingCharacteristics", "NativeRange", "Habitat", "EnvironmentalNotes",
                        "DetailedDescription", "GrowthHabit", "FoliageDescription", "FlowerDescription",
                        "FloweringPeriod", "FruitDescription", "WateringInstructions", "SeasonalWateringNotes",
                        "FertilizingInstructions", "PruningInstructions", "RepottingInstructions")
                    SELECT p."Id", 'en-US', p."CommonName",
                        COALESCE((SELECT json_group_array(a."Name") FROM "PlantAliases" a WHERE a."PlantId" = p."Id"), '[]'),
                        p."Description", p."IdentifyingCharacteristics", p."NativeRange", p."Habitat",
                        p."EnvironmentalNotes", p."DetailedDescription", p."GrowthHabit", p."FoliageDescription",
                        p."FlowerDescription", p."FloweringPeriod", p."FruitDescription", p."WateringInstructions",
                        p."SeasonalWateringNotes", p."FertilizingInstructions", p."PruningInstructions", p."RepottingInstructions"
                    FROM "Plants" p;
                    """);
                return;
            }

            migrationBuilder.CreateTable(
                name: "PlantTranslations",
                schema: "greenhouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlantId = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CommonName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AlternativeNamesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentifyingCharacteristics = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NativeRange = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Habitat = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnvironmentalNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DetailedDescription = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    GrowthHabit = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FoliageDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FlowerDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FloweringPeriod = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FruitDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    WateringInstructions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SeasonalWateringNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FertilizingInstructions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PruningInstructions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RepottingInstructions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlantTranslations_Plants_PlantId",
                        column: x => x.PlantId,
                        principalSchema: "greenhouse",
                        principalTable: "Plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlantTranslations_PlantId_Language",
                schema: "greenhouse",
                table: "PlantTranslations",
                columns: new[] { "PlantId", "Language" },
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO greenhouse.PlantTranslations (
                    PlantId, Language, CommonName, AlternativeNamesJson, Description,
                    IdentifyingCharacteristics, NativeRange, Habitat, EnvironmentalNotes,
                    DetailedDescription, GrowthHabit, FoliageDescription, FlowerDescription,
                    FloweringPeriod, FruitDescription, WateringInstructions, SeasonalWateringNotes,
                    FertilizingInstructions, PruningInstructions, RepottingInstructions)
                SELECT p.Id, 'en-US', p.CommonName,
                    COALESCE((SELECT '[' + STRING_AGG('"' + STRING_ESCAPE(a.Name, 'json') + '"', ',') + ']'
                        FROM greenhouse.PlantAliases a WHERE a.PlantId = p.Id), '[]'),
                    p.Description, p.IdentifyingCharacteristics, p.NativeRange, p.Habitat,
                    p.EnvironmentalNotes, p.DetailedDescription, p.GrowthHabit, p.FoliageDescription,
                    p.FlowerDescription, p.FloweringPeriod, p.FruitDescription, p.WateringInstructions,
                    p.SeasonalWateringNotes, p.FertilizingInstructions, p.PruningInstructions, p.RepottingInstructions
                FROM greenhouse.Plants p;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                migrationBuilder.DropTable(name: "PlantTranslations");
                return;
            }

            migrationBuilder.DropTable(
                name: "PlantTranslations",
                schema: "greenhouse");
        }
    }
}
