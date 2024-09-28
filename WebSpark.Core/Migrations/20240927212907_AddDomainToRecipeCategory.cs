using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebSpark.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddDomainToRecipeCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DomainId",
                table: "RecipeCategory",
                type: "INTEGER",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.CreateIndex(
                name: "IX_RecipeCategory_DomainId",
                table: "RecipeCategory",
                column: "DomainId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeCategory_Domain_DomainId",
                table: "RecipeCategory",
                column: "DomainId",
                principalTable: "Domain",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeCategory_Domain_DomainId",
                table: "RecipeCategory");

            migrationBuilder.DropIndex(
                name: "IX_RecipeCategory_DomainId",
                table: "RecipeCategory");

            migrationBuilder.DropColumn(
                name: "DomainId",
                table: "RecipeCategory");
        }
    }
}
