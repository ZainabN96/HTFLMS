using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HTFLMS.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAccessibleToModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAccessible",
                table: "Modules",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAccessible",
                table: "Modules");
        }
    }
}
