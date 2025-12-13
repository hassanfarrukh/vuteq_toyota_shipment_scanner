using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class ToyotaApiConfigIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmationNumber",
                table: "tblSkidBuildSessions");

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

            migrationBuilder.AddColumn<string>(
                name: "ToyotaShipmentConfirmationNumber",
                table: "tblOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToyotaShipmentErrorMessage",
                table: "tblOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToyotaShipmentStatus",
                table: "tblOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ToyotaShipmentSubmittedAt",
                table: "tblOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToyotaSkidBuildConfirmationNumber",
                table: "tblOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToyotaSkidBuildErrorMessage",
                table: "tblOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToyotaSkidBuildStatus",
                table: "tblOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ToyotaSkidBuildSubmittedAt",
                table: "tblOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tblToyotaApiConfig",
                columns: table => new
                {
                    ConfigId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Environment = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ApplicationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClientSecret = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TokenUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ApiBaseUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblToyotaApiConfig", x => x.ConfigId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tblToyotaApiConfig_Environment",
                table: "tblToyotaApiConfig",
                column: "Environment");

            migrationBuilder.CreateIndex(
                name: "IX_tblToyotaApiConfig_Environment_IsActive",
                table: "tblToyotaApiConfig",
                columns: new[] { "Environment", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblToyotaApiConfig");

            migrationBuilder.DropColumn(
                name: "ToyotaShipmentConfirmationNumber",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "ToyotaShipmentErrorMessage",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "ToyotaShipmentStatus",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "ToyotaShipmentSubmittedAt",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "ToyotaSkidBuildConfirmationNumber",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "ToyotaSkidBuildErrorMessage",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "ToyotaSkidBuildStatus",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "ToyotaSkidBuildSubmittedAt",
                table: "tblOrders");

            migrationBuilder.AddColumn<string>(
                name: "ConfirmationNumber",
                table: "tblSkidBuildSessions",
                type: "nvarchar(100)",
                maxLength: 100,
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
    }
}
