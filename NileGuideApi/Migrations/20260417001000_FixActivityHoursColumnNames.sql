BEGIN TRANSACTION;
GO

IF OBJECT_ID(N'[dbo].[ActivityHours]', N'U') IS NOT NULL
BEGIN
    IF OBJECT_ID(N'[dbo].[FK_ActivityHours_Activities_activity_id]', N'F') IS NOT NULL
        ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [FK_ActivityHours_Activities_activity_id];
    IF OBJECT_ID(N'[dbo].[CK_ActivityHours_closing_period]', N'C') IS NOT NULL
        ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [CK_ActivityHours_closing_period];
    IF OBJECT_ID(N'[dbo].[CK_ActivityHours_opening_period]', N'C') IS NOT NULL
        ALTER TABLE [dbo].[ActivityHours] DROP CONSTRAINT [CK_ActivityHours_opening_period];
END;
GO

IF COL_LENGTH(N'dbo.ActivityHours', N'activity_id') IS NOT NULL
    EXEC sp_rename N'dbo.ActivityHours.activity_id', N'ActivityID', N'COLUMN';
GO

IF COL_LENGTH(N'dbo.ActivityHours', N'opening_hour') IS NOT NULL
    EXEC sp_rename N'dbo.ActivityHours.opening_hour', N'OpeningHour', N'COLUMN';
GO

IF COL_LENGTH(N'dbo.ActivityHours', N'opening_period') IS NOT NULL
    EXEC sp_rename N'dbo.ActivityHours.opening_period', N'OpeningPeriod', N'COLUMN';
GO

IF COL_LENGTH(N'dbo.ActivityHours', N'closing_hour') IS NOT NULL
    EXEC sp_rename N'dbo.ActivityHours.closing_hour', N'ClosingHour', N'COLUMN';
GO

IF COL_LENGTH(N'dbo.ActivityHours', N'closing_period') IS NOT NULL
    EXEC sp_rename N'dbo.ActivityHours.closing_period', N'ClosingPeriod', N'COLUMN';
GO

IF EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'[dbo].[ActivityHours]')
      AND name = N'IX_ActivityHours_activity_id'
)
    EXEC sp_rename N'dbo.ActivityHours.IX_ActivityHours_activity_id', N'IX_ActivityHours_ActivityID', N'INDEX';
GO

IF OBJECT_ID(N'[dbo].[CK_ActivityHours_ClosingPeriod]', N'C') IS NULL
    ALTER TABLE [dbo].[ActivityHours]
    ADD CONSTRAINT [CK_ActivityHours_ClosingPeriod]
    CHECK ([ClosingPeriod] IS NULL OR [ClosingPeriod] IN ('am', 'pm'));
GO

IF OBJECT_ID(N'[dbo].[CK_ActivityHours_OpeningPeriod]', N'C') IS NULL
    ALTER TABLE [dbo].[ActivityHours]
    ADD CONSTRAINT [CK_ActivityHours_OpeningPeriod]
    CHECK ([OpeningPeriod] IS NULL OR [OpeningPeriod] IN ('am', 'pm'));
GO

IF OBJECT_ID(N'[dbo].[FK_ActivityHours_Activities_ActivityID]', N'F') IS NULL
    ALTER TABLE [dbo].[ActivityHours]
    ADD CONSTRAINT [FK_ActivityHours_Activities_ActivityID]
    FOREIGN KEY ([ActivityID]) REFERENCES [dbo].[Activities]([ActivityID]) ON DELETE CASCADE;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260417001000_FixActivityHoursColumnNames', N'8.0.0');
GO

COMMIT;
GO

