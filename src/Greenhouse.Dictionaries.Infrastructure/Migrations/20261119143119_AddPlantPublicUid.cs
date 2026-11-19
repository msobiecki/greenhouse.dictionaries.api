using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Greenhouse.Dictionaries.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlantPublicUid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                migrationBuilder.AddColumn<Guid>(
                    name: "Uid",
                    table: "Plants",
                    type: "TEXT",
                    nullable: false,
                    defaultValue: Guid.Empty);

                migrationBuilder.Sql("""
                    UPDATE "Plants"
                    SET "Uid" = hex(randomblob(4)) || '-' || hex(randomblob(2)) || '-4' ||
                        substr(hex(randomblob(2)), 2) || '-' ||
                        substr('89AB', (random() & 3) + 1, 1) || substr(hex(randomblob(2)), 2) || '-' ||
                        hex(randomblob(6));
                    """);

                migrationBuilder.CreateIndex(
                    name: "IX_Plants_Uid",
                    table: "Plants",
                    column: "Uid",
                    unique: true);
                return;
            }

            migrationBuilder.AddColumn<Guid>(
                name: "Uid",
                schema: "greenhouse",
                table: "Plants",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.CreateIndex(
                name: "IX_Plants_Uid",
                schema: "greenhouse",
                table: "Plants",
                column: "Uid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                migrationBuilder.DropIndex(name: "IX_Plants_Uid", table: "Plants");
                migrationBuilder.Sql("ALTER TABLE \"Plants\" DROP COLUMN \"Uid\";");
                return;
            }

            migrationBuilder.DropIndex(
                name: "IX_Plants_Uid",
                schema: "greenhouse",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "Uid",
                schema: "greenhouse",
                table: "Plants");
        }
    }
}
