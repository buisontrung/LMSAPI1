using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSAPI1.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCourseClassToTrainingProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseClasses_Classes_ClassId",
                table: "CourseClasses");

            migrationBuilder.RenameColumn(
                name: "ClassId",
                table: "CourseClasses",
                newName: "TrainingProgramId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseClasses_ClassId",
                table: "CourseClasses",
                newName: "IX_CourseClasses_TrainingProgramId");

            migrationBuilder.AddColumn<Guid>(
                name: "TrainingProgramId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TrainingProgramId",
                table: "AspNetUsers",
                column: "TrainingProgramId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_TrainingPrograms_TrainingProgramId",
                table: "AspNetUsers",
                column: "TrainingProgramId",
                principalTable: "TrainingPrograms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseClasses_TrainingPrograms_TrainingProgramId",
                table: "CourseClasses",
                column: "TrainingProgramId",
                principalTable: "TrainingPrograms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_TrainingPrograms_TrainingProgramId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseClasses_TrainingPrograms_TrainingProgramId",
                table: "CourseClasses");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TrainingProgramId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TrainingProgramId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "TrainingProgramId",
                table: "CourseClasses",
                newName: "ClassId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseClasses_TrainingProgramId",
                table: "CourseClasses",
                newName: "IX_CourseClasses_ClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseClasses_Classes_ClassId",
                table: "CourseClasses",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
