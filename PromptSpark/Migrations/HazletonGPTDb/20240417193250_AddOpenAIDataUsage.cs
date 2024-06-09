using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PromptSpark.Migrations.HazletonGPTDb
{
    /// <inheritdoc />
    public partial class AddOpenAIDataUsage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompletionTokens",
                table: "DefinitionResponses",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PromptTokens",
                table: "DefinitionResponses",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionTokens",
                table: "DefinitionResponses");

            migrationBuilder.DropColumn(
                name: "PromptTokens",
                table: "DefinitionResponses");
        }
    }
}
