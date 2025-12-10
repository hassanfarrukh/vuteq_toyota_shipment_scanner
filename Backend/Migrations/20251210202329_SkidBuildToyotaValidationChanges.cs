using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class SkidBuildToyotaValidationChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SkidNumber",
                table: "tblSkidScans",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "PalletizationCode",
                table: "tblSkidScans",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RawSkidId",
                table: "tblSkidScans",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SkidSide",
                table: "tblSkidScans",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InternalReferenceNumber",
                table: "tblSkidBuildSessions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToyotaConfirmationNumber",
                table: "tblSkidBuildSessions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToyotaErrorMessage",
                table: "tblSkidBuildSessions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToyotaSubmissionStatus",
                table: "tblSkidBuildSessions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PalletizationCode",
                table: "tblSkidScans");

            migrationBuilder.DropColumn(
                name: "RawSkidId",
                table: "tblSkidScans");

            migrationBuilder.DropColumn(
                name: "SkidSide",
                table: "tblSkidScans");

            migrationBuilder.DropColumn(
                name: "InternalReferenceNumber",
                table: "tblSkidBuildSessions");

            migrationBuilder.DropColumn(
                name: "ToyotaConfirmationNumber",
                table: "tblSkidBuildSessions");

            migrationBuilder.DropColumn(
                name: "ToyotaErrorMessage",
                table: "tblSkidBuildSessions");

            migrationBuilder.DropColumn(
                name: "ToyotaSubmissionStatus",
                table: "tblSkidBuildSessions");

            migrationBuilder.AlterColumn<int>(
                name: "SkidNumber",
                table: "tblSkidScans",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);
        }
    }
}
