using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PromptSpark.Migrations.HazletonGPTDb
{
    /// <inheritdoc />
    public partial class AddOutputType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OutputType",
                table: "DefinitionTypes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ServiceDescriptionName",
                table: "Definitions",
                type: "TEXT",
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<int>(
                name: "OutputType",
                table: "Definitions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Temperature",
                table: "DefinitionResponses",
                type: "TEXT",
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SystemResponse",
                table: "DefinitionResponses",
                type: "TEXT",
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "DefinitionResponses",
                type: "TEXT",
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DefinitionType",
                table: "DefinitionResponses",
                type: "TEXT",
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceDescriptionName",
                table: "DefinitionResponses",
                type: "TEXT",
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<int>(
                name: "OutputType",
                table: "DefinitionResponses",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserExpectedResponse",
                table: "DefinitionResponses",
                type: "TEXT",
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AlterColumn<string>(
                name: "TestPrompt",
                table: "Chats",
                type: "TEXT",
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DefinitionType",
                table: "Chats",
                type: "TEXT",
                nullable: false,
                defaultValue: string.Empty,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserExpectedResponse",
                table: "Chats",
                type: "TEXT",
                nullable: false,
                defaultValue: string.Empty);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OutputType",
                table: "DefinitionTypes");

            migrationBuilder.DropColumn(
                name: "ServiceDescriptionName",
                table: "Definitions");

            migrationBuilder.DropColumn(
                name: "OutputType",
                table: "Definitions");

            migrationBuilder.DropColumn(
                name: "ServiceDescriptionName",
                table: "DefinitionResponses");

            migrationBuilder.DropColumn(
                name: "OutputType",
                table: "DefinitionResponses");

            migrationBuilder.DropColumn(
                name: "UserExpectedResponse",
                table: "DefinitionResponses");

            migrationBuilder.DropColumn(
                name: "UserExpectedResponse",
                table: "Chats");

            migrationBuilder.AlterColumn<string>(
                name: "Temperature",
                table: "DefinitionResponses",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "SystemResponse",
                table: "DefinitionResponses",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "DefinitionResponses",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "DefinitionType",
                table: "DefinitionResponses",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "TestPrompt",
                table: "Chats",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "DefinitionType",
                table: "Chats",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }
    }
}
