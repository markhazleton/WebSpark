using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PromptSpark.Migrations.HazletonGPTDb
{
    /// <inheritdoc />
    public partial class AddPersonTraits2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonaTraits_Definitions_GPTDefinitionDefinitionId",
                table: "PersonaTraits");

            migrationBuilder.DropIndex(
                name: "IX_PersonaTraits_GPTDefinitionDefinitionId",
                table: "PersonaTraits");

            migrationBuilder.DropColumn(
                name: "GPTDefinitionDefinitionId",
                table: "PersonaTraits");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DefinitionPersonaTrait");

            migrationBuilder.AddColumn<int>(
                name: "GPTDefinitionDefinitionId",
                table: "PersonaTraits",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonaTraits_GPTDefinitionDefinitionId",
                table: "PersonaTraits",
                column: "GPTDefinitionDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonaTraits_Definitions_GPTDefinitionDefinitionId",
                table: "PersonaTraits",
                column: "GPTDefinitionDefinitionId",
                principalTable: "Definitions",
                principalColumn: "DefinitionId");
        }
    }
}
