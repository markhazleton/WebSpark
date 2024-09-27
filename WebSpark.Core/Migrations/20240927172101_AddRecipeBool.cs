using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebSpark.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeBool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRecipeSite",
                table: "Domain",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRecipeSite",
                table: "Domain");
        }
    }
}
