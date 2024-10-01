using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TriviaSpark.JShow.Migrations
{
    /// <inheritdoc />
    public partial class AddShowType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "JShows",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "JShows");
        }
    }
}
