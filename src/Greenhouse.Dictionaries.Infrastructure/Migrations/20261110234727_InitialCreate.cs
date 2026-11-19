using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Greenhouse.Dictionaries.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                migrationBuilder.CreateTable(
                    name: "Plants",
                    columns: table => new
                    {
                        Id = table.Column<int>(type: "INTEGER", nullable: false)
                            .Annotation("Sqlite:Autoincrement", true),
                        ScientificName = table.Column<string>(type: "TEXT", nullable: false),
                        CommonName = table.Column<string>(type: "TEXT", nullable: false),
                        Family = table.Column<string>(type: "TEXT", nullable: false),
                        Category = table.Column<string>(type: "TEXT", nullable: false),
                        Description = table.Column<string>(type: "TEXT", nullable: false),
                        Sunlight = table.Column<string>(type: "TEXT", nullable: false),
                        Watering = table.Column<string>(type: "TEXT", nullable: false),
                        FlowerColor = table.Column<string>(type: "TEXT", nullable: true),
                        CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                        UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                    },
                    constraints: table =>
                    {
                        table.PrimaryKey("PK_Plants", x => x.Id);
                    });
                return;
            }

            migrationBuilder.EnsureSchema(
                name: "greenhouse");

            migrationBuilder.CreateTable(
                name: "Plants",
                schema: "greenhouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScientificName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CommonName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Family = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sunlight = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Watering = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FlowerColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plants", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                migrationBuilder.DropTable(name: "Plants");
                return;
            }

            migrationBuilder.DropTable(
                name: "Plants",
                schema: "greenhouse");
        }
    }
}
