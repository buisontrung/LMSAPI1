using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMSAPI1.Migrations
{
    /// <inheritdoc />
    public partial class AddNameToCourseClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "CourseClasses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "CourseClasses");
        }
    }
}
