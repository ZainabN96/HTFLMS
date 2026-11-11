using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HTFLMS.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizNotificationTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LockNotificationSentAt",
                table: "QuizAttemptsResets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UnlockNotificationSentAt",
                table: "QuizAttemptsResets",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LockNotificationSentAt",
                table: "QuizAttemptsResets");

            migrationBuilder.DropColumn(
                name: "UnlockNotificationSentAt",
                table: "QuizAttemptsResets");
        }
    }
}
