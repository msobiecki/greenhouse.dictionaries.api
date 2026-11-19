using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Greenhouse.Dictionaries.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlantEnvironmentalPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                migrationBuilder.AddColumn<string>(name: "EnvironmentalNotes", table: "Plants", type: "TEXT", nullable: true);
                migrationBuilder.AddColumn<decimal>(name: "HumidityMaxPercent", table: "Plants", type: "TEXT", nullable: true);
                migrationBuilder.AddColumn<decimal>(name: "HumidityMinPercent", table: "Plants", type: "TEXT", nullable: true);
                migrationBuilder.AddColumn<decimal>(name: "TemperatureMaxC", table: "Plants", type: "TEXT", nullable: true);
                migrationBuilder.AddColumn<decimal>(name: "TemperatureMinC", table: "Plants", type: "TEXT", nullable: true);
                return;
            }

            migrationBuilder.AddColumn<string>(
                name: "EnvironmentalNotes",
                schema: "greenhouse",
                table: "Plants",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HumidityMaxPercent",
                schema: "greenhouse",
                table: "Plants",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HumidityMinPercent",
                schema: "greenhouse",
                table: "Plants",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TemperatureMaxC",
                schema: "greenhouse",
                table: "Plants",
                type: "decimal(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TemperatureMinC",
                schema: "greenhouse",
                table: "Plants",
                type: "decimal(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                migrationBuilder.Sql("""
                    ALTER TABLE "Plants" DROP COLUMN "EnvironmentalNotes";
                    ALTER TABLE "Plants" DROP COLUMN "HumidityMaxPercent";
                    ALTER TABLE "Plants" DROP COLUMN "HumidityMinPercent";
                    ALTER TABLE "Plants" DROP COLUMN "TemperatureMaxC";
                    ALTER TABLE "Plants" DROP COLUMN "TemperatureMinC";
                    """);
                return;
            }

            migrationBuilder.DropColumn(
                name: "EnvironmentalNotes",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "HumidityMaxPercent",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "HumidityMinPercent",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "TemperatureMaxC",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "TemperatureMinC",
                schema: "greenhouse",
                table: "Plants");
        }
    }
}
