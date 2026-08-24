using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HTFLMS.Migrations
{
    /// <inheritdoc />
    public partial class AddCertificateGenerationFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TitlePrefix",
                table: "Users",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryMode",
                table: "CourseEnrollments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Onsite");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryModeUpdatedAt",
                table: "CourseEnrollments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliveryModeUpdatedByUserId",
                table: "CourseEnrollments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StudentCertificateNumbers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    DeliveryMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BaseNumber = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentCertificateNumbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentCertificateNumbers_Users_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentCertificateNumbers_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CertificateRequestId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    StudentCertificateNumberId = table.Column<int>(type: "int", nullable: true),
                    DeliveryMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CertificateYear = table.Column<int>(type: "int", nullable: false),
                    BaseNumber = table.Column<int>(type: "int", nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    CertificateId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CertificateFilePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StudentNameSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitlePrefixSnapshot = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CourseTitleSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BatchNumberSnapshot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DurationSnapshot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BatchStartDateSnapshot = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BatchEndDateSnapshot = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveryModeSnapshot = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GeneratedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificates_CertificateRequests_CertificateRequestId",
                        column: x => x.CertificateRequestId,
                        principalTable: "CertificateRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Certificates_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Certificates_StudentCertificateNumbers_StudentCertificateNumberId",
                        column: x => x.StudentCertificateNumberId,
                        principalTable: "StudentCertificateNumbers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Certificates_Users_GeneratedByUserId",
                        column: x => x.GeneratedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Certificates_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseEnrollments_DeliveryModeUpdatedByUserId",
                table: "CourseEnrollments",
                column: "DeliveryModeUpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CertificateRequestId",
                table: "Certificates",
                column: "CertificateRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_CourseId",
                table: "Certificates",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_DeliveryMode_CertificateId",
                table: "Certificates",
                columns: new[] { "DeliveryMode", "CertificateId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_DeliveryMode_CertificateYear_BaseNumber_Suffix",
                table: "Certificates",
                columns: new[] { "DeliveryMode", "CertificateYear", "BaseNumber", "Suffix" },
                unique: true,
                filter: "[Suffix] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_GeneratedByUserId",
                table: "Certificates",
                column: "GeneratedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_StudentCertificateNumberId",
                table: "Certificates",
                column: "StudentCertificateNumberId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_StudentId",
                table: "Certificates",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCertificateNumbers_AssignedByUserId",
                table: "StudentCertificateNumbers",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentCertificateNumbers_DeliveryMode_BaseNumber",
                table: "StudentCertificateNumbers",
                columns: new[] { "DeliveryMode", "BaseNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentCertificateNumbers_StudentId_DeliveryMode",
                table: "StudentCertificateNumbers",
                columns: new[] { "StudentId", "DeliveryMode" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseEnrollments_Users_DeliveryModeUpdatedByUserId",
                table: "CourseEnrollments",
                column: "DeliveryModeUpdatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseEnrollments_Users_DeliveryModeUpdatedByUserId",
                table: "CourseEnrollments");

            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "StudentCertificateNumbers");

            migrationBuilder.DropIndex(
                name: "IX_CourseEnrollments_DeliveryModeUpdatedByUserId",
                table: "CourseEnrollments");

            migrationBuilder.DropColumn(
                name: "TitlePrefix",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeliveryMode",
                table: "CourseEnrollments");

            migrationBuilder.DropColumn(
                name: "DeliveryModeUpdatedAt",
                table: "CourseEnrollments");

            migrationBuilder.DropColumn(
                name: "DeliveryModeUpdatedByUserId",
                table: "CourseEnrollments");
        }
    }
}
