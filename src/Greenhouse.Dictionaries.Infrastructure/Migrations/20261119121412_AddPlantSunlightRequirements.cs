using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Greenhouse.Dictionaries.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlantSunlightRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                migrationBuilder.AddColumn<string>(name: "SunlightRequirement", table: "Plants", type: "TEXT", nullable: true);
                migrationBuilder.AddColumn<decimal>(name: "SunlightHoursMin", table: "Plants", type: "TEXT", nullable: true);
                migrationBuilder.AddColumn<decimal>(name: "SunlightHoursMax", table: "Plants", type: "TEXT", nullable: true);
                return;
            }

            migrationBuilder.AddColumn<decimal>(
                name: "SunlightHoursMax",
                schema: "greenhouse",
                table: "Plants",
                type: "decimal(4,2)",
                precision: 4,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SunlightHoursMin",
                schema: "greenhouse",
                table: "Plants",
                type: "decimal(4,2)",
                precision: 4,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SunlightRequirement",
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
                    ALTER TABLE "Plants" DROP COLUMN "SunlightHoursMax";
                    ALTER TABLE "Plants" DROP COLUMN "SunlightHoursMin";
                    ALTER TABLE "Plants" DROP COLUMN "SunlightRequirement";
                    """);
                return;
            }

            migrationBuilder.DropColumn(
                name: "SunlightHoursMax",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "SunlightHoursMin",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "SunlightRequirement",
                schema: "greenhouse",
                table: "Plants");
        }
    }
}
