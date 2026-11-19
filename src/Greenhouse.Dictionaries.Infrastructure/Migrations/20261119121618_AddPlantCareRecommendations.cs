using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Greenhouse.Dictionaries.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlantCareRecommendations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                migrationBuilder.AddColumn<string>(name: "FertilizingInstructions", table: "Plants", type: "TEXT", nullable: true);
                migrationBuilder.AddColumn<string>(name: "PruningInstructions", table: "Plants", type: "TEXT", nullable: true);
                migrationBuilder.AddColumn<string>(name: "RepottingInstructions", table: "Plants", type: "TEXT", nullable: true);
                migrationBuilder.AddColumn<string>(name: "SeasonalWateringNotes", table: "Plants", type: "TEXT", nullable: true);
                migrationBuilder.AddColumn<string>(name: "WateringInstructions", table: "Plants", type: "TEXT", nullable: true);
                migrationBuilder.AddColumn<string>(name: "WateringRequirement", table: "Plants", type: "TEXT", nullable: true);
                return;
            }

            migrationBuilder.AddColumn<string>(
                name: "FertilizingInstructions",
                schema: "greenhouse",
                table: "Plants",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PruningInstructions",
                schema: "greenhouse",
                table: "Plants",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepottingInstructions",
                schema: "greenhouse",
                table: "Plants",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeasonalWateringNotes",
                schema: "greenhouse",
                table: "Plants",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WateringInstructions",
                schema: "greenhouse",
                table: "Plants",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WateringRequirement",
                schema: "greenhouse",
                table: "Plants",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                migrationBuilder.Sql("""
                    ALTER TABLE "Plants" DROP COLUMN "FertilizingInstructions";
                    ALTER TABLE "Plants" DROP COLUMN "PruningInstructions";
                    ALTER TABLE "Plants" DROP COLUMN "RepottingInstructions";
                    ALTER TABLE "Plants" DROP COLUMN "SeasonalWateringNotes";
                    ALTER TABLE "Plants" DROP COLUMN "WateringInstructions";
                    ALTER TABLE "Plants" DROP COLUMN "WateringRequirement";
                    """);
                return;
            }

            migrationBuilder.DropColumn(
                name: "FertilizingInstructions",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "PruningInstructions",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "RepottingInstructions",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "SeasonalWateringNotes",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "WateringInstructions",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "WateringRequirement",
                schema: "greenhouse",
                table: "Plants");
        }
    }
}
