using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PromptSpark.Migrations.HazletonGPTDb
{
    /// <inheritdoc />
    public partial class AddPersonTraitUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PersonaTraits_Trait",
                table: "PersonaTraits",
                column: "Trait",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Definitions_GPTName",
                table: "Definitions",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PersonaTraits_Trait",
                table: "PersonaTraits");

            migrationBuilder.DropIndex(
                name: "IX_Definitions_GPTName",
                table: "Definitions");
        }
    }
}
