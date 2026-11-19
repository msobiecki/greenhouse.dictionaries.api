using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Greenhouse.Dictionaries.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NormalizePlantToxicity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                migrationBuilder.Sql("""
                    CREATE TABLE "PlantToxicityAssessments" (
                        "Id" INTEGER NOT NULL CONSTRAINT "PK_PlantToxicityAssessments" PRIMARY KEY AUTOINCREMENT,
                        "PlantId" INTEGER NOT NULL,
                        "TargetCode" TEXT NOT NULL,
                        "Level" TEXT NOT NULL,
                        "Details" TEXT NULL,
                        CONSTRAINT "FK_PlantToxicityAssessments_Plants_PlantId"
                            FOREIGN KEY ("PlantId") REFERENCES "Plants" ("Id") ON DELETE CASCADE
                    );
                    CREATE UNIQUE INDEX "IX_PlantToxicityAssessments_PlantId_TargetCode"
                        ON "PlantToxicityAssessments" ("PlantId", "TargetCode");
                    CREATE TABLE "PlantToxicitySources" (
                        "Id" INTEGER NOT NULL CONSTRAINT "PK_PlantToxicitySources" PRIMARY KEY AUTOINCREMENT,
                        "PlantToxicityAssessmentId" INTEGER NOT NULL,
                        "Url" TEXT NOT NULL,
                        "Publisher" TEXT NULL,
                        CONSTRAINT "FK_PlantToxicitySources_PlantToxicityAssessments_PlantToxicityAssessmentId"
                            FOREIGN KEY ("PlantToxicityAssessmentId")
                            REFERENCES "PlantToxicityAssessments" ("Id") ON DELETE CASCADE
                    );
                    CREATE INDEX "IX_PlantToxicitySources_PlantToxicityAssessmentId"
                        ON "PlantToxicitySources" ("PlantToxicityAssessmentId");

                    INSERT INTO "PlantToxicityAssessments" ("PlantId", "TargetCode", "Level", "Details")
                    SELECT "Id", 'cat', CASE WHEN "ToxicToCats" = 1 THEN 'Toxic' ELSE 'NoKnownRisk' END, "ToxicityDetails"
                    FROM "Plants" WHERE "ToxicToCats" IS NOT NULL;
                    INSERT INTO "PlantToxicityAssessments" ("PlantId", "TargetCode", "Level", "Details")
                    SELECT "Id", 'dog', CASE WHEN "ToxicToDogs" = 1 THEN 'Toxic' ELSE 'NoKnownRisk' END, "ToxicityDetails"
                    FROM "Plants" WHERE "ToxicToDogs" IS NOT NULL;
                    INSERT INTO "PlantToxicityAssessments" ("PlantId", "TargetCode", "Level", "Details")
                    SELECT "Id", 'human', CASE WHEN "ToxicToHumans" = 1 THEN 'Toxic' ELSE 'NoKnownRisk' END, "ToxicityDetails"
                    FROM "Plants" WHERE "ToxicToHumans" IS NOT NULL;
                    INSERT INTO "PlantToxicityAssessments" ("PlantId", "TargetCode", "Level", "Details")
                    SELECT "Id", 'unspecified', 'Unknown', "ToxicityDetails"
                    FROM "Plants"
                    WHERE "ToxicToCats" IS NULL AND "ToxicToDogs" IS NULL AND "ToxicToHumans" IS NULL
                        AND ("ToxicityDetails" IS NOT NULL OR "ToxicitySource" IS NOT NULL);

                    INSERT INTO "PlantToxicitySources" ("PlantToxicityAssessmentId", "Url", "Publisher")
                    SELECT assessment."Id", plant."ToxicitySource", NULL
                    FROM "PlantToxicityAssessments" AS assessment
                    JOIN "Plants" AS plant ON plant."Id" = assessment."PlantId"
                    WHERE plant."ToxicitySource" IS NOT NULL;

                    ALTER TABLE "Plants" DROP COLUMN "ToxicToCats";
                    ALTER TABLE "Plants" DROP COLUMN "ToxicToDogs";
                    ALTER TABLE "Plants" DROP COLUMN "ToxicToHumans";
                    ALTER TABLE "Plants" DROP COLUMN "ToxicityDetails";
                    ALTER TABLE "Plants" DROP COLUMN "ToxicitySource";
                    """);
                return;
            }

            migrationBuilder.CreateTable(
                name: "PlantToxicityAssessments",
                schema: "greenhouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlantId = table.Column<int>(type: "int", nullable: false),
                    TargetCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Level = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantToxicityAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlantToxicityAssessments_Plants_PlantId",
                        column: x => x.PlantId,
                        principalSchema: "greenhouse",
                        principalTable: "Plants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlantToxicitySources",
                schema: "greenhouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlantToxicityAssessmentId = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    Publisher = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantToxicitySources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlantToxicitySources_PlantToxicityAssessments_PlantToxicityAssessmentId",
                        column: x => x.PlantToxicityAssessmentId,
                        principalSchema: "greenhouse",
                        principalTable: "PlantToxicityAssessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlantToxicityAssessments_PlantId_TargetCode",
                schema: "greenhouse",
                table: "PlantToxicityAssessments",
                columns: new[] { "PlantId", "TargetCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlantToxicitySources_PlantToxicityAssessmentId",
                schema: "greenhouse",
                table: "PlantToxicitySources",
                column: "PlantToxicityAssessmentId");

            migrationBuilder.Sql("""
                INSERT INTO [greenhouse].[PlantToxicityAssessments] ([PlantId], [TargetCode], [Level], [Details])
                SELECT plant.[Id], target.[TargetCode],
                    CASE WHEN target.[IsToxic] = 1 THEN 'Toxic' ELSE 'NoKnownRisk' END,
                    plant.[ToxicityDetails]
                FROM [greenhouse].[Plants] AS plant
                CROSS APPLY (VALUES
                    ('cat', plant.[ToxicToCats]),
                    ('dog', plant.[ToxicToDogs]),
                    ('human', plant.[ToxicToHumans])) AS target([TargetCode], [IsToxic])
                WHERE target.[IsToxic] IS NOT NULL;

                INSERT INTO [greenhouse].[PlantToxicityAssessments] ([PlantId], [TargetCode], [Level], [Details])
                SELECT [Id], 'unspecified', 'Unknown', [ToxicityDetails]
                FROM [greenhouse].[Plants]
                WHERE [ToxicToCats] IS NULL AND [ToxicToDogs] IS NULL AND [ToxicToHumans] IS NULL
                    AND ([ToxicityDetails] IS NOT NULL OR [ToxicitySource] IS NOT NULL);

                INSERT INTO [greenhouse].[PlantToxicitySources] ([PlantToxicityAssessmentId], [Url], [Publisher])
                SELECT assessment.[Id], plant.[ToxicitySource], NULL
                FROM [greenhouse].[PlantToxicityAssessments] AS assessment
                JOIN [greenhouse].[Plants] AS plant ON plant.[Id] = assessment.[PlantId]
                WHERE plant.[ToxicitySource] IS NOT NULL;
                """);

            migrationBuilder.DropColumn(name: "ToxicToCats", schema: "greenhouse", table: "Plants");
            migrationBuilder.DropColumn(name: "ToxicToDogs", schema: "greenhouse", table: "Plants");
            migrationBuilder.DropColumn(name: "ToxicToHumans", schema: "greenhouse", table: "Plants");
            migrationBuilder.DropColumn(name: "ToxicityDetails", schema: "greenhouse", table: "Plants");
            migrationBuilder.DropColumn(name: "ToxicitySource", schema: "greenhouse", table: "Plants");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                migrationBuilder.AddColumn<bool>(name: "ToxicToCats", table: "Plants", type: "INTEGER", nullable: true);
                migrationBuilder.AddColumn<bool>(name: "ToxicToDogs", table: "Plants", type: "INTEGER", nullable: true);
                migrationBuilder.AddColumn<bool>(name: "ToxicToHumans", table: "Plants", type: "INTEGER", nullable: true);
                migrationBuilder.AddColumn<string>(name: "ToxicityDetails", table: "Plants", type: "TEXT", nullable: true);
                migrationBuilder.AddColumn<string>(name: "ToxicitySource", table: "Plants", type: "TEXT", nullable: true);
                migrationBuilder.Sql("""
                    UPDATE "Plants" SET
                        "ToxicToCats" = (SELECT CASE "Level" WHEN 'Toxic' THEN 1 WHEN 'NoKnownRisk' THEN 0 END
                            FROM "PlantToxicityAssessments" WHERE "PlantId" = "Plants"."Id" AND "TargetCode" = 'cat'),
                        "ToxicToDogs" = (SELECT CASE "Level" WHEN 'Toxic' THEN 1 WHEN 'NoKnownRisk' THEN 0 END
                            FROM "PlantToxicityAssessments" WHERE "PlantId" = "Plants"."Id" AND "TargetCode" = 'dog'),
                        "ToxicToHumans" = (SELECT CASE "Level" WHEN 'Toxic' THEN 1 WHEN 'NoKnownRisk' THEN 0 END
                            FROM "PlantToxicityAssessments" WHERE "PlantId" = "Plants"."Id" AND "TargetCode" = 'human'),
                        "ToxicityDetails" = (SELECT "Details" FROM "PlantToxicityAssessments"
                            WHERE "PlantId" = "Plants"."Id" AND "Details" IS NOT NULL ORDER BY "Id" LIMIT 1),
                        "ToxicitySource" = (SELECT source."Url" FROM "PlantToxicitySources" AS source
                            JOIN "PlantToxicityAssessments" AS assessment
                                ON assessment."Id" = source."PlantToxicityAssessmentId"
                            WHERE assessment."PlantId" = "Plants"."Id" ORDER BY source."Id" LIMIT 1);
                    """);
                migrationBuilder.DropTable(name: "PlantToxicitySources");
                migrationBuilder.DropTable(name: "PlantToxicityAssessments");
                return;
            }

            migrationBuilder.AddColumn<bool>(name: "ToxicToCats", schema: "greenhouse", table: "Plants", type: "bit", nullable: true);
            migrationBuilder.AddColumn<bool>(name: "ToxicToDogs", schema: "greenhouse", table: "Plants", type: "bit", nullable: true);
            migrationBuilder.AddColumn<bool>(name: "ToxicToHumans", schema: "greenhouse", table: "Plants", type: "bit", nullable: true);
            migrationBuilder.AddColumn<string>(name: "ToxicityDetails", schema: "greenhouse", table: "Plants", type: "nvarchar(max)", nullable: true);
            migrationBuilder.AddColumn<string>(name: "ToxicitySource", schema: "greenhouse", table: "Plants", type: "nvarchar(max)", nullable: true);
            migrationBuilder.Sql("""
                UPDATE plant SET
                    [ToxicToCats] = CASE cat.[Level] WHEN 'Toxic' THEN 1 WHEN 'NoKnownRisk' THEN 0 END,
                    [ToxicToDogs] = CASE dog.[Level] WHEN 'Toxic' THEN 1 WHEN 'NoKnownRisk' THEN 0 END,
                    [ToxicToHumans] = CASE human.[Level] WHEN 'Toxic' THEN 1 WHEN 'NoKnownRisk' THEN 0 END,
                    [ToxicityDetails] = details.[Details],
                    [ToxicitySource] = source.[Url]
                FROM [greenhouse].[Plants] AS plant
                LEFT JOIN [greenhouse].[PlantToxicityAssessments] AS cat
                    ON cat.[PlantId] = plant.[Id] AND cat.[TargetCode] = 'cat'
                LEFT JOIN [greenhouse].[PlantToxicityAssessments] AS dog
                    ON dog.[PlantId] = plant.[Id] AND dog.[TargetCode] = 'dog'
                LEFT JOIN [greenhouse].[PlantToxicityAssessments] AS human
                    ON human.[PlantId] = plant.[Id] AND human.[TargetCode] = 'human'
                OUTER APPLY (SELECT TOP (1) assessment.[Details]
                    FROM [greenhouse].[PlantToxicityAssessments] AS assessment
                    WHERE assessment.[PlantId] = plant.[Id] AND assessment.[Details] IS NOT NULL
                    ORDER BY assessment.[Id]) AS details
                OUTER APPLY (SELECT TOP (1) toxicitySource.[Url]
                    FROM [greenhouse].[PlantToxicitySources] AS toxicitySource
                    JOIN [greenhouse].[PlantToxicityAssessments] AS assessment
                        ON assessment.[Id] = toxicitySource.[PlantToxicityAssessmentId]
                    WHERE assessment.[PlantId] = plant.[Id]
                    ORDER BY toxicitySource.[Id]) AS source;
                """);
            migrationBuilder.DropTable(
                name: "PlantToxicitySources",
                schema: "greenhouse");

            migrationBuilder.DropTable(
                name: "PlantToxicityAssessments",
                schema: "greenhouse");

            migrationBuilder.AddColumn<bool>(
                name: "ToxicToCats",
                schema: "greenhouse",
                table: "Plants",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ToxicToDogs",
                schema: "greenhouse",
                table: "Plants",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ToxicToHumans",
                schema: "greenhouse",
                table: "Plants",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToxicityDetails",
                schema: "greenhouse",
                table: "Plants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToxicitySource",
                schema: "greenhouse",
                table: "Plants",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
