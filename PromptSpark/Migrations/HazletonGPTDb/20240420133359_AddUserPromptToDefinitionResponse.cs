using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PromptSpark.Migrations.HazletonGPTDb
{
    /// <inheritdoc />
    public partial class AddUserPromptToDefinitionResponse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TestPrompt",
                table: "DefinitionResponses",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TestPrompt",
                table: "DefinitionResponses");
        }
    }
}
