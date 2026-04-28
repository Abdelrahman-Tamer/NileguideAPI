using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NileGuideApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDateOfBirth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Users', N'date_of_birth') IS NULL
    ALTER TABLE [dbo].[Users] ADD [date_of_birth] date NULL;
""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
IF COL_LENGTH(N'dbo.Users', N'date_of_birth') IS NOT NULL
    ALTER TABLE [dbo].[Users] DROP COLUMN [date_of_birth];
""");
        }
    }
}
