using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebSpark.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddKeywordtoMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KeyWords",
                table: "Menu",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KeyWords",
                table: "Menu");
        }
    }
}
