using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSAPI1.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherToCourseClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TeacherId",
                table: "CourseClasses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseClasses_TeacherId",
                table: "CourseClasses",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseClasses_AspNetUsers_TeacherId",
                table: "CourseClasses",
                column: "TeacherId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseClasses_AspNetUsers_TeacherId",
                table: "CourseClasses");

            migrationBuilder.DropIndex(
                name: "IX_CourseClasses_TeacherId",
                table: "CourseClasses");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "CourseClasses");
        }
    }
}
