using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Greenhouse.Dictionaries.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDetailedPlantInformation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                migrationBuilder.AddColumn<string>(name: "DetailedDescription", table: "Plants", type: "TEXT", nullable: true);
                migrationBuilder.AddColumn<string>(name: "FlowerDescription", table: "Plants", type: "TEXT", nullable: true);
                migrationBuilder.AddColumn<string>(name: "FloweringPeriod", table: "Plants", type: "TEXT", nullable: true);
                migrationBuilder.AddColumn<string>(name: "FoliageDescription", table: "Plants", type: "TEXT", nullable: true);
                migrationBuilder.AddColumn<string>(name: "FruitDescription", table: "Plants", type: "TEXT", nullable: true);
                migrationBuilder.AddColumn<string>(name: "GrowthHabit", table: "Plants", type: "TEXT", nullable: true);
                migrationBuilder.AddColumn<string>(name: "LifeCycle", table: "Plants", type: "TEXT", nullable: true);
                return;
            }

            migrationBuilder.AddColumn<string>(
                name: "DetailedDescription",
                schema: "greenhouse",
                table: "Plants",
                type: "nvarchar(max)",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlowerDescription",
                schema: "greenhouse",
                table: "Plants",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FloweringPeriod",
                schema: "greenhouse",
                table: "Plants",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FoliageDescription",
                schema: "greenhouse",
                table: "Plants",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FruitDescription",
                schema: "greenhouse",
                table: "Plants",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GrowthHabit",
                schema: "greenhouse",
                table: "Plants",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LifeCycle",
                schema: "greenhouse",
                table: "Plants",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                migrationBuilder.Sql("""
                    ALTER TABLE "Plants" DROP COLUMN "DetailedDescription";
                    ALTER TABLE "Plants" DROP COLUMN "FlowerDescription";
                    ALTER TABLE "Plants" DROP COLUMN "FloweringPeriod";
                    ALTER TABLE "Plants" DROP COLUMN "FoliageDescription";
                    ALTER TABLE "Plants" DROP COLUMN "FruitDescription";
                    ALTER TABLE "Plants" DROP COLUMN "GrowthHabit";
                    ALTER TABLE "Plants" DROP COLUMN "LifeCycle";
                    """);
                return;
            }

            migrationBuilder.DropColumn(
                name: "DetailedDescription",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "FlowerDescription",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "FloweringPeriod",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "FoliageDescription",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "FruitDescription",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "GrowthHabit",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "LifeCycle",
                schema: "greenhouse",
                table: "Plants");
        }
    }
}
