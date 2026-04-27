using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HTFLMS.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedNotificationandCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RedirectUrl",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BatchNumber",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RedirectUrl",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "BatchNumber",
                table: "Courses");
        }
    }
}
