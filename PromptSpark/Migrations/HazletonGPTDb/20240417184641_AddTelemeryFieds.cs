using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PromptSpark.Migrations.HazletonGPTDb
{
    /// <inheritdoc />
    public partial class AddTelemeryFieds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "temperature",
                table: "Definitions",
                newName: "Temperature");

            migrationBuilder.RenameColumn(
                name: "model",
                table: "Definitions",
                newName: "Model");

            migrationBuilder.AddColumn<long>(
                name: "ElapsedMilliseconds",
                table: "DefinitionResponses",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ElapsedMilliseconds",
                table: "DefinitionResponses");

            migrationBuilder.RenameColumn(
                name: "Temperature",
                table: "Definitions",
                newName: "temperature");

            migrationBuilder.RenameColumn(
                name: "Model",
                table: "Definitions",
                newName: "model");
        }
    }
}
