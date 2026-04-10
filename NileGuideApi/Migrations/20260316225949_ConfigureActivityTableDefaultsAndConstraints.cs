using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NileGuideApi.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureActivityTableDefaultsAndConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE Activities
                SET PriceCurrency = 'USD'
                WHERE PriceCurrency IS NULL OR LTRIM(RTRIM(PriceCurrency)) = '';

                UPDATE Activities
                SET Status = 'Available'
                WHERE Status IS NULL OR LTRIM(RTRIM(Status)) = '';

                UPDATE Activities
                SET PriceBasis = NULL
                WHERE PriceBasis IS NOT NULL AND LTRIM(RTRIM(PriceBasis)) = '';
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Activities",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Available",
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<bool>(
                name: "RequiresPersonalID",
                table: "Activities",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "PriceCurrency",
                table: "Activities",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "USD",
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Activities_PriceBasis_Allowed",
                table: "Activities",
                sql: "[PriceBasis] IS NULL OR [PriceBasis] IN ('Free', 'PerPerson', 'PerTicket', 'PerHour', 'PerTrip', 'PerNight')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Activities_Status_Allowed",
                table: "Activities",
                sql: "[Status] IN ('Available', 'Unavailable', 'Temporarily Closed')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Activities_PriceBasis_Allowed",
                table: "Activities");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Activities_Status_Allowed",
                table: "Activities");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Activities",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Available");

            migrationBuilder.AlterColumn<bool>(
                name: "RequiresPersonalID",
                table: "Activities",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "PriceCurrency",
                table: "Activities",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10,
                oldDefaultValue: "USD");
        }
    }
}
