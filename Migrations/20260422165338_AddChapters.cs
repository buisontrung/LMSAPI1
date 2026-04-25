using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSAPI1.Migrations
{
    /// <inheritdoc />
    public partial class AddChapters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_Courses_CourseId",
                table: "Lessons");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "Lessons",
                newName: "ChapterId");

            migrationBuilder.RenameIndex(
                name: "IX_Lessons_CourseId",
                table: "Lessons",
                newName: "IX_Lessons_ChapterId");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Lessons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Chapters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chapters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chapters_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_CourseId",
                table: "Chapters",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_Chapters_ChapterId",
                table: "Lessons",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_Chapters_ChapterId",
                table: "Lessons");

            migrationBuilder.DropTable(
                name: "Chapters");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Lessons");

            migrationBuilder.RenameColumn(
                name: "ChapterId",
                table: "Lessons",
                newName: "CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_Lessons_ChapterId",
                table: "Lessons",
                newName: "IX_Lessons_CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_Courses_CourseId",
                table: "Lessons",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
