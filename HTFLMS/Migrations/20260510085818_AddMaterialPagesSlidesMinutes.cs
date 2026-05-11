using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HTFLMS.Migrations
{
    public partial class AddMaterialPagesSlidesMinutes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Minutes",
                table: "Materials",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Pages",
                table: "Materials",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Slides",
                table: "Materials",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Minutes",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "Pages",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "Slides",
                table: "Materials");
        }
    }
}