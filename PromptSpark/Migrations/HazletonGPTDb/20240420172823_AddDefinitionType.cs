using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PromptSpark.Migrations.HazletonGPTDb
{
    /// <inheritdoc />
    public partial class AddDefinitionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefinitionType",
                table: "DefinitionResponses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Temperature",
                table: "DefinitionResponses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefinitionType",
                table: "Chats",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefinitionType",
                table: "DefinitionResponses");

            migrationBuilder.DropColumn(
                name: "Temperature",
                table: "DefinitionResponses");

            migrationBuilder.DropColumn(
                name: "DefinitionType",
                table: "Chats");
        }
    }
}
