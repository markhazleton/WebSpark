using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PromptSpark.Migrations.HazletonGPTDb
{
    /// <inheritdoc />
    public partial class AddOpenAIData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "DefinitionResponses",
                type: "TEXT",
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<int>(
                name: "TotalTokens",
                table: "DefinitionResponses",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Model",
                table: "DefinitionResponses");

            migrationBuilder.DropColumn(
                name: "TotalTokens",
                table: "DefinitionResponses");
        }
    }
}
