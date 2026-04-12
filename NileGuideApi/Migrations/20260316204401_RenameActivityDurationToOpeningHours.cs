using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NileGuideApi.Migrations
{
    /// <inheritdoc />
    public partial class RenameActivityDurationToOpeningHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('Activities', 'Duration') IS NOT NULL
                BEGIN
                    EXEC sp_rename 'Activities.Duration', 'OpeningHours', 'COLUMN';
                END;

                IF COL_LENGTH('Activities', 'OpeningHours') IS NOT NULL
                BEGIN
                    ALTER TABLE Activities
                    ALTER COLUMN OpeningHours NVARCHAR(200) NULL;
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH('Activities', 'OpeningHours') IS NOT NULL
                BEGIN
                    ALTER TABLE Activities
                    ALTER COLUMN OpeningHours VARCHAR(50) NULL;

                    EXEC sp_rename 'Activities.OpeningHours', 'Duration', 'COLUMN';
                END;
                """);
        }
    }
}
