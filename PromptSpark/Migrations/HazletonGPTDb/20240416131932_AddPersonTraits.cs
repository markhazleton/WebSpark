using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace PromptSpark.Migrations.HazletonGPTDb
{
    /// <inheritdoc />
    public partial class AddPersonTraits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                table: "Definitions",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "PersonaTraits",
                columns: table => new
                {
                    TraitId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Trait = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    GPTDefinitionDefinitionId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonaTraits", x => x.TraitId);
                    table.ForeignKey(
                        name: "FK_PersonaTraits_Definitions_GPTDefinitionDefinitionId",
                        column: x => x.GPTDefinitionDefinitionId,
                        principalTable: "Definitions",
                        principalColumn: "DefinitionId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonaTraits_GPTDefinitionDefinitionId",
                table: "PersonaTraits",
                column: "GPTDefinitionDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonaTraits");

            migrationBuilder.DropColumn(
                name: "Updated",
                table: "Definitions");
        }
    }
}
