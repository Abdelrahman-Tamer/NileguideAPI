using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NileGuideApi.Migrations
{
    /// <inheritdoc />
    public partial class ConvertActivityDurationToMinutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE a
SET [Duration] = CONVERT(nvarchar(50), CASE
        WHEN parsed.DurationValue IS NULL THEN 0
        WHEN parsed.CleanDuration LIKE '%hour%' THEN parsed.DurationValue * 60
        WHEN parsed.CleanDuration LIKE '%minute%' THEN parsed.DurationValue
        ELSE parsed.DurationValue
    END)
FROM [Activities] AS a
CROSS APPLY (
    SELECT LOWER(LTRIM(RTRIM(ISNULL(a.[Duration], '')))) AS CleanDuration
) AS clean
CROSS APPLY (
    SELECT PATINDEX('%[0-9]%', clean.CleanDuration) AS FirstDigitIndex
) AS firstDigit
CROSS APPLY (
    SELECT CASE
        WHEN firstDigit.FirstDigitIndex > 0
            THEN SUBSTRING(clean.CleanDuration, firstDigit.FirstDigitIndex, 50)
        ELSE ''
    END AS FromFirstDigit
) AS fromDigit
CROSS APPLY (
    SELECT PATINDEX('%[^0-9]%', fromDigit.FromFirstDigit + 'x') - 1 AS DigitCount
) AS digitCount
CROSS APPLY (
    SELECT
        clean.CleanDuration,
        TRY_CONVERT(int, NULLIF(LEFT(fromDigit.FromFirstDigit, digitCount.DigitCount), '')) AS DurationValue
) AS parsed;
");

            migrationBuilder.AlterColumn<int>(
                name: "Duration",
                table: "Activities",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Duration",
                table: "Activities",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.Sql(@"
UPDATE [Activities]
SET [Duration] =
    CASE
        WHEN TRY_CONVERT(int, [Duration]) IS NULL OR TRY_CONVERT(int, [Duration]) <= 0 THEN NULL
        WHEN TRY_CONVERT(int, [Duration]) % 60 = 0 THEN CONCAT(TRY_CONVERT(int, [Duration]) / 60, ' hours')
        ELSE CONCAT([Duration], ' minutes')
    END;
");
        }
    }
}
