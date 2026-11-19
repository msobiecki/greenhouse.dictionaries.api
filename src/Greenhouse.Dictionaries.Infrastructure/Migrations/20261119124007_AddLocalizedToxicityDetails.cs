using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Greenhouse.Dictionaries.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLocalizedToxicityDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                migrationBuilder.AddColumn<string>(
                    name: "ToxicityDetailsJson",
                    table: "PlantTranslations",
                    type: "TEXT",
                    nullable: true);
                return;
            }

            migrationBuilder.AddColumn<string>(
                name: "ToxicityDetailsJson",
                schema: "greenhouse",
                table: "PlantTranslations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                migrationBuilder.Sql("ALTER TABLE \"PlantTranslations\" DROP COLUMN \"ToxicityDetailsJson\";");
                return;
            }

            migrationBuilder.DropColumn(
                name: "ToxicityDetailsJson",
                schema: "greenhouse",
                table: "PlantTranslations");
        }
    }
}
