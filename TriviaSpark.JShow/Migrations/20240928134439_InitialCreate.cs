using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TriviaSpark.JShow.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JShows",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ShowNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    AirDate = table.Column<string>(type: "TEXT", nullable: false),
                    Theme = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JShows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JShowRounds",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    JShowId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Theme = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JShowRounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JShowRounds_JShows_JShowId",
                        column: x => x.JShowId,
                        principalTable: "JShows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    RoundId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_JShowRounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "JShowRounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<int>(type: "INTEGER", nullable: false),
                    QuestionText = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Answer = table.Column<string>(type: "TEXT", nullable: false),
                    Theme = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questions_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Category_RoundId_Name",
                table: "Categories",
                columns: new[] { "RoundId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JShowRounds_JShowId",
                table: "JShowRounds",
                column: "JShowId");

            migrationBuilder.CreateIndex(
                name: "IX_JShows_Theme",
                table: "JShows",
                column: "Theme",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Question_CategoryId_QuestionText",
                table: "Questions",
                columns: new[] { "CategoryId", "QuestionText" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Question_CategoryId_Value",
                table: "Questions",
                columns: new[] { "CategoryId", "Value" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "JShowRounds");

            migrationBuilder.DropTable(
                name: "JShows");
        }
    }
}
