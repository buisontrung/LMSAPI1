using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSAPI1.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStudentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Students_StudentId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Students_StudentId",
                table: "Submissions");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Submissions",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Submissions_StudentId",
                table: "Submissions",
                newName: "IX_Submissions_UserId");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "Enrollments",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollments_StudentId",
                table: "Enrollments",
                newName: "IX_Enrollments_UserId");

            migrationBuilder.AddColumn<Guid>(
                name: "ClassId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentCode",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ClassId",
                table: "AspNetUsers",
                column: "ClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Classes_ClassId",
                table: "AspNetUsers",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_AspNetUsers_UserId",
                table: "Enrollments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_AspNetUsers_UserId",
                table: "Submissions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Classes_ClassId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_AspNetUsers_UserId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_AspNetUsers_UserId",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ClassId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "StudentCode",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Submissions",
                newName: "StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Submissions_UserId",
                table: "Submissions",
                newName: "IX_Submissions_StudentId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Enrollments",
                newName: "StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollments_UserId",
                table: "Enrollments",
                newName: "IX_Enrollments_StudentId");

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StudentCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Students_ClassId",
                table: "Students",
                column: "ClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Students_StudentId",
                table: "Enrollments",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Students_StudentId",
                table: "Submissions",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
