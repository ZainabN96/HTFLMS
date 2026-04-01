using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HTFLMS.Migrations
{
    /// <inheritdoc />
    public partial class RepairCoursesCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.Courses', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Courses](
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Title] NVARCHAR(160) NOT NULL,
        [Category] NVARCHAR(80) NOT NULL,
        [InstructorName] NVARCHAR(120) NOT NULL,
        [Level] NVARCHAR(80) NULL,
        [ShortDescription] NVARCHAR(500) NULL,
        [Content] NVARCHAR(MAX) NULL,
        [ImageUrl] NVARCHAR(MAX) NULL,
        [CreatedAtUtc] DATETIME2 NOT NULL
    );
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'dbo.Courses', N'U') IS NOT NULL
BEGIN
    DROP TABLE [dbo].[Courses];
END
");
        }
    }
}
