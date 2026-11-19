using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Greenhouse.Dictionaries.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddComprehensivePlantDictionary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                migrationBuilder.Sql("""
                    CREATE TABLE "Plants_new" (
                        "Id" INTEGER NOT NULL CONSTRAINT "PK_Plants" PRIMARY KEY AUTOINCREMENT,
                        "ScientificName" TEXT NOT NULL,
                        "CommonName" TEXT NOT NULL,
                        "Category" TEXT NOT NULL,
                        "Family" TEXT NOT NULL,
                        "Description" TEXT NOT NULL,
                        "IdentifyingCharacteristics" TEXT NOT NULL,
                        "NativeRange" TEXT NULL,
                        "Habitat" TEXT NULL,
                        "MatureHeightCm" TEXT NULL,
                        "MatureWidthCm" TEXT NULL,
                        "ToxicToCats" INTEGER NULL,
                        "ToxicToDogs" INTEGER NULL,
                        "ToxicToHumans" INTEGER NULL,
                        "ToxicityDetails" TEXT NULL,
                        "ToxicitySource" TEXT NULL,
                        "CreatedAt" TEXT NOT NULL,
                        "UpdatedAt" TEXT NOT NULL
                    );
                    INSERT INTO "Plants_new" (
                        "Id", "ScientificName", "CommonName", "Category", "Family", "Description",
                        "IdentifyingCharacteristics", "CreatedAt", "UpdatedAt")
                    SELECT "Id", "ScientificName", "CommonName",
                        CASE lower("Category")
                            WHEN 'flower' THEN 'Flowering' WHEN 'flowering' THEN 'Flowering'
                            WHEN 'foliage' THEN 'Foliage' WHEN 'succulent' THEN 'Succulent'
                            WHEN 'fern' THEN 'Fern' WHEN 'palm' THEN 'Palm'
                            WHEN 'climber' THEN 'Climber' WHEN 'carnivorous' THEN 'Carnivorous'
                            ELSE 'Other' END,
                        "Family", "Description", "Description", "CreatedAt", "UpdatedAt"
                    FROM "Plants";
                    DROP TABLE "Plants";
                    ALTER TABLE "Plants_new" RENAME TO "Plants";
                    """);
                CreateChildTables(migrationBuilder, sqlite: true);
                return;
            }

            migrationBuilder.DropColumn(
                name: "Sunlight",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "Watering",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "FlowerColor",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.AddColumn<string>(
                name: "IdentifyingCharacteristics",
                schema: "greenhouse",
                table: "Plants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ToxicitySource",
                schema: "greenhouse",
                table: "Plants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE [greenhouse].[Plants] SET [Category] = CASE LOWER([Category])
                    WHEN 'flower' THEN 'Flowering' WHEN 'flowering' THEN 'Flowering'
                    WHEN 'foliage' THEN 'Foliage' WHEN 'succulent' THEN 'Succulent'
                    WHEN 'fern' THEN 'Fern' WHEN 'palm' THEN 'Palm'
                    WHEN 'climber' THEN 'Climber' WHEN 'carnivorous' THEN 'Carnivorous'
                    ELSE 'Other' END;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                schema: "greenhouse",
                table: "Plants",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Habitat",
                schema: "greenhouse",
                table: "Plants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MatureHeightCm",
                schema: "greenhouse",
                table: "Plants",
                type: "decimal(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MatureWidthCm",
                schema: "greenhouse",
                table: "Plants",
                type: "decimal(8,2)",
                precision: 8,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NativeRange",
                schema: "greenhouse",
                table: "Plants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ToxicToCats", schema: "greenhouse", table: "Plants", type: "bit", nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ToxicToDogs", schema: "greenhouse", table: "Plants", type: "bit", nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ToxicToHumans", schema: "greenhouse", table: "Plants", type: "bit", nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToxicityDetails", schema: "greenhouse", table: "Plants", type: "nvarchar(max)", nullable: true);

            migrationBuilder.Sql("UPDATE [greenhouse].[Plants] SET [IdentifyingCharacteristics] = [Description];");
            CreateChildTables(migrationBuilder, sqlite: false);
        }

        private static void CreateChildTables(MigrationBuilder migrationBuilder, bool sqlite)
        {
            var schema = sqlite ? null : "greenhouse";
            var idType = sqlite ? "INTEGER" : "int";
            var nameType = sqlite ? "TEXT" : "nvarchar(200)";
            var urlType = sqlite ? "TEXT" : "nvarchar(2048)";
            var sourceType = sqlite ? "TEXT" : "nvarchar(500)";
            var licenseType = sqlite ? "TEXT" : "nvarchar(200)";
            var attributionType = sqlite ? "TEXT" : "nvarchar(500)";

            migrationBuilder.CreateTable(
                name: "PlantAliases",
                schema: schema,
                columns: table => new
                {
                    Id = sqlite
                        ? table.Column<int>(type: idType, nullable: false).Annotation("Sqlite:Autoincrement", true)
                        : table.Column<int>(type: idType, nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    PlantId = table.Column<int>(type: idType, nullable: false),
                    Name = table.Column<string>(type: nameType, maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantAliases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlantAliases_Plants_PlantId",
                        column: x => x.PlantId,
                        principalSchema: schema,
                        principalTable: "Plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlantImages",
                schema: schema,
                columns: table => new
                {
                    Id = sqlite
                        ? table.Column<int>(type: idType, nullable: false).Annotation("Sqlite:Autoincrement", true)
                        : table.Column<int>(type: idType, nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    PlantId = table.Column<int>(type: idType, nullable: false),
                    Url = table.Column<string>(type: urlType, maxLength: 2048, nullable: false),
                    Source = table.Column<string>(type: sourceType, maxLength: 500, nullable: false),
                    License = table.Column<string>(type: licenseType, maxLength: 200, nullable: false),
                    Attribution = table.Column<string>(type: attributionType, maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlantImages_Plants_PlantId",
                        column: x => x.PlantId,
                        principalSchema: schema,
                        principalTable: "Plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlantAliases_PlantId",
                schema: schema,
                table: "PlantAliases",
                column: "PlantId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantImages_PlantId",
                schema: schema,
                table: "PlantImages",
                column: "PlantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                migrationBuilder.DropTable(name: "PlantAliases");
                migrationBuilder.DropTable(name: "PlantImages");
                migrationBuilder.Sql("""
                    CREATE TABLE "Plants_old" (
                        "Id" INTEGER NOT NULL CONSTRAINT "PK_Plants" PRIMARY KEY AUTOINCREMENT,
                        "ScientificName" TEXT NOT NULL,
                        "CommonName" TEXT NOT NULL,
                        "Family" TEXT NOT NULL,
                        "Category" TEXT NOT NULL,
                        "Description" TEXT NOT NULL,
                        "Sunlight" TEXT NOT NULL,
                        "Watering" TEXT NOT NULL,
                        "FlowerColor" TEXT NULL,
                        "CreatedAt" TEXT NOT NULL,
                        "UpdatedAt" TEXT NOT NULL
                    );
                    INSERT INTO "Plants_old" (
                        "Id", "ScientificName", "CommonName", "Family", "Category", "Description",
                        "Sunlight", "Watering", "FlowerColor", "CreatedAt", "UpdatedAt")
                    SELECT "Id", "ScientificName", "CommonName", "Family", "Category", "Description",
                        '', '', NULL, "CreatedAt", "UpdatedAt"
                    FROM "Plants";
                    DROP TABLE "Plants";
                    ALTER TABLE "Plants_old" RENAME TO "Plants";
                    """);
                return;
            }

            migrationBuilder.DropTable(
                name: "PlantAliases",
                schema: "greenhouse");

            migrationBuilder.DropTable(
                name: "PlantImages",
                schema: "greenhouse");

            migrationBuilder.DropColumn(
                name: "Habitat",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(name: "IdentifyingCharacteristics", schema: "greenhouse", table: "Plants");

            migrationBuilder.DropColumn(
                name: "MatureHeightCm",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "MatureWidthCm",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "NativeRange",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "ToxicToCats",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "ToxicToDogs",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "ToxicToHumans",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "ToxicityDetails",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(name: "ToxicitySource", schema: "greenhouse", table: "Plants");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                schema: "greenhouse",
                table: "Plants",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "Sunlight",
                schema: "greenhouse",
                table: "Plants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Watering",
                schema: "greenhouse",
                table: "Plants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FlowerColor",
                schema: "greenhouse",
                table: "Plants",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
