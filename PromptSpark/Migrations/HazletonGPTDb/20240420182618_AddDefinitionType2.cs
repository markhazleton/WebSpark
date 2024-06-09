using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace PromptSpark.Migrations.HazletonGPTDb
{
    /// <inheritdoc />
    public partial class AddDefinitionType2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DefinitionTypes",
                columns: table => new
                {
                    DefinitionType = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefinitionTypes", x => x.DefinitionType);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DefinitionTypes_DefinitionType",
                table: "DefinitionTypes",
                column: "DefinitionType",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DefinitionTypes");
        }
    }
}
