using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NileGuideApi.Migrations
{
    /// <inheritdoc />
    public partial class RenameActivityHoursTableToActivityHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[activity_hours]', N'U') IS NOT NULL
                BEGIN
                    IF OBJECT_ID(N'[dbo].[CK_activity_hours_closing_period]', N'C') IS NOT NULL
                        ALTER TABLE [dbo].[activity_hours] DROP CONSTRAINT [CK_activity_hours_closing_period];

                    IF OBJECT_ID(N'[dbo].[CK_activity_hours_opening_period]', N'C') IS NOT NULL
                        ALTER TABLE [dbo].[activity_hours] DROP CONSTRAINT [CK_activity_hours_opening_period];

                    IF OBJECT_ID(N'[dbo].[FK_activity_hours_Activities_activity_id]', N'F') IS NOT NULL
                        ALTER TABLE [dbo].[activity_hours] DROP CONSTRAINT [FK_activity_hours_Activities_activity_id];

                    IF OBJECT_ID(N'[dbo].[PK_activity_hours]', N'PK') IS NOT NULL
                        ALTER TABLE [dbo].[activity_hours] DROP CONSTRAINT [PK_activity_hours];

                    EXEC sp_rename N'dbo.activity_hours', N'ActivityHours';
                END;

                IF COL_LENGTH(N'dbo.ActivityHours', N'activity_id') IS NOT NULL
                    EXEC sp_rename N'dbo.ActivityHours.activity_id', N'ActivityID', N'COLUMN';

                IF COL_LENGTH(N'dbo.ActivityHours', N'opening_hour') IS NOT NULL
                    EXEC sp_rename N'dbo.ActivityHours.opening_hour', N'OpeningHour', N'COLUMN';

                IF COL_LENGTH(N'dbo.ActivityHours', N'opening_period') IS NOT NULL
                    EXEC sp_rename N'dbo.ActivityHours.opening_period', N'OpeningPeriod', N'COLUMN';

                IF COL_LENGTH(N'dbo.ActivityHours', N'closing_hour') IS NOT NULL
                    EXEC sp_rename N'dbo.ActivityHours.closing_hour', N'ClosingHour', N'COLUMN';

                IF COL_LENGTH(N'dbo.ActivityHours', N'closing_period') IS NOT NULL
                    EXEC sp_rename N'dbo.ActivityHours.closing_period', N'ClosingPeriod', N'COLUMN';

                IF EXISTS (
                    SELECT 1
                    FROM sys.indexes
                    WHERE name = N'IX_activity_hours_activity_id'
                      AND object_id = OBJECT_ID(N'[dbo].[ActivityHours]')
                )
                BEGIN
                    EXEC sp_rename N'dbo.ActivityHours.IX_activity_hours_activity_id', N'IX_ActivityHours_ActivityID', N'INDEX';
                END;

                IF OBJECT_ID(N'[dbo].[PK_ActivityHours]', N'PK') IS NULL
                    ALTER TABLE [dbo].[ActivityHours] ADD CONSTRAINT [PK_ActivityHours] PRIMARY KEY ([id]);

                IF OBJECT_ID(N'[dbo].[CK_ActivityHours_ClosingPeriod]', N'C') IS NULL
                    ALTER TABLE [dbo].[ActivityHours] ADD CONSTRAINT [CK_ActivityHours_ClosingPeriod] CHECK ([ClosingPeriod] IS NULL OR [ClosingPeriod] IN ('am', 'pm'));

                IF OBJECT_ID(N'[dbo].[CK_ActivityHours_OpeningPeriod]', N'C') IS NULL
                    ALTER TABLE [dbo].[ActivityHours] ADD CONSTRAINT [CK_ActivityHours_OpeningPeriod] CHECK ([OpeningPeriod] IS NULL OR [OpeningPeriod] IN ('am', 'pm'));

                IF OBJECT_ID(N'[dbo].[FK_ActivityHours_Activities_ActivityID]', N'F') IS NULL
                    ALTER TABLE [dbo].[ActivityHours] ADD CONSTRAINT [FK_ActivityHours_Activities_ActivityID]
                    FOREIGN KEY ([ActivityID]) REFERENCES [dbo].[Activities]([ActivityID]) ON DELETE CASCADE;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[ActivityHours]', N'U') IS NOT NULL
                BEGIN
                    IF OBJECT_ID(N'[dbo].[CK_ActivityHours_ClosingPeriod]', N'C') IS NOT NULL
                        ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [CK_ActivityHours_ClosingPeriod];

                    IF OBJECT_ID(N'[dbo].[CK_ActivityHours_OpeningPeriod]', N'C') IS NOT NULL
                        ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [CK_ActivityHours_OpeningPeriod];

                    IF OBJECT_ID(N'[dbo].[FK_ActivityHours_Activities_ActivityID]', N'F') IS NOT NULL
                        ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [FK_ActivityHours_Activities_ActivityID];

                    IF OBJECT_ID(N'[dbo].[PK_ActivityHours]', N'PK') IS NOT NULL
                        ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [PK_ActivityHours];

                    IF EXISTS (
                        SELECT 1
                        FROM sys.indexes
                        WHERE name = N'IX_ActivityHours_ActivityID'
                          AND object_id = OBJECT_ID(N'[dbo].[ActivityHours]')
                    )
                    BEGIN
                        EXEC sp_rename N'dbo.ActivityHours.IX_ActivityHours_ActivityID', N'IX_activity_hours_activity_id', N'INDEX';
                    END;

                    IF COL_LENGTH(N'dbo.ActivityHours', N'ActivityID') IS NOT NULL
                        EXEC sp_rename N'dbo.ActivityHours.ActivityID', N'activity_id', N'COLUMN';

                    IF COL_LENGTH(N'dbo.ActivityHours', N'OpeningHour') IS NOT NULL
                        EXEC sp_rename N'dbo.ActivityHours.OpeningHour', N'opening_hour', N'COLUMN';

                    IF COL_LENGTH(N'dbo.ActivityHours', N'OpeningPeriod') IS NOT NULL
                        EXEC sp_rename N'dbo.ActivityHours.OpeningPeriod', N'opening_period', N'COLUMN';

                    IF COL_LENGTH(N'dbo.ActivityHours', N'ClosingHour') IS NOT NULL
                        EXEC sp_rename N'dbo.ActivityHours.ClosingHour', N'closing_hour', N'COLUMN';

                    IF COL_LENGTH(N'dbo.ActivityHours', N'ClosingPeriod') IS NOT NULL
                        EXEC sp_rename N'dbo.ActivityHours.ClosingPeriod', N'closing_period', N'COLUMN';

                    EXEC sp_rename N'dbo.ActivityHours', N'activity_hours';

                    ALTER TABLE [dbo].[activity_hours] ADD CONSTRAINT [PK_activity_hours] PRIMARY KEY ([id]);
                    ALTER TABLE [dbo].[activity_hours] ADD CONSTRAINT [CK_activity_hours_closing_period] CHECK ([closing_period] IS NULL OR [closing_period] IN ('am', 'pm'));
                    ALTER TABLE [dbo].[activity_hours] ADD CONSTRAINT [CK_activity_hours_opening_period] CHECK ([opening_period] IS NULL OR [opening_period] IN ('am', 'pm'));
                    ALTER TABLE [dbo].[activity_hours] ADD CONSTRAINT [FK_activity_hours_Activities_activity_id]
                    FOREIGN KEY ([activity_id]) REFERENCES [dbo].[Activities]([ActivityID]) ON DELETE CASCADE;
                END;
                """);
        }
    }
}
