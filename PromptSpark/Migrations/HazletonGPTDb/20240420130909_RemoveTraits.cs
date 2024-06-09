using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace PromptSpark.Migrations.HazletonGPTDb
{
    /// <inheritdoc />
    public partial class RemoveTraits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DefinitionPersonaTrait");

            migrationBuilder.DropTable(
                name: "PersonaTraits");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonaTraits",
                columns: table => new
                {
                    TraitId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Trait = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Updated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonaTraits", x => x.TraitId);
                });

            migrationBuilder.CreateTable(
                name: "DefinitionPersonaTrait",
                columns: table => new
                {
                    DefinitionsDefinitionId = table.Column<int>(type: "INTEGER", nullable: false),
                    PersonaTraitsTraitId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefinitionPersonaTrait", x => new { x.DefinitionsDefinitionId, x.PersonaTraitsTraitId });
                    table.ForeignKey(
                        name: "FK_DefinitionPersonaTrait_Definitions_DefinitionsDefinitionId",
                        column: x => x.DefinitionsDefinitionId,
                        principalTable: "Definitions",
                        principalColumn: "DefinitionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DefinitionPersonaTrait_PersonaTraits_PersonaTraitsTraitId",
                        column: x => x.PersonaTraitsTraitId,
                        principalTable: "PersonaTraits",
                        principalColumn: "TraitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DefinitionPersonaTrait_PersonaTraitsTraitId",
                table: "DefinitionPersonaTrait",
                column: "PersonaTraitsTraitId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonaTraits_Trait",
                table: "PersonaTraits",
                column: "Trait",
                unique: true);
        }
    }
}
