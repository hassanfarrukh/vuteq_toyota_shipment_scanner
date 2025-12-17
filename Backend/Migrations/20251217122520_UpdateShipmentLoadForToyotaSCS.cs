using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateShipmentLoadForToyotaSCS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblScannedSkids");

            migrationBuilder.DropColumn(
                name: "CarrierName",
                table: "tblShipmentLoadSessions");

            migrationBuilder.DropColumn(
                name: "CurrentScreen",
                table: "tblShipmentLoadSessions");

            migrationBuilder.DropColumn(
                name: "DriverName",
                table: "tblShipmentLoadSessions");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "tblShipmentLoadSessions");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "tblShipmentLoadSessions");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "tblShipmentLoadSessions");

            migrationBuilder.RenameColumn(
                name: "ConfirmationNumber",
                table: "tblShipmentLoadSessions",
                newName: "ToyotaConfirmationNumber");

            migrationBuilder.AddColumn<bool>(
                name: "IsSkidCut",
                table: "tblSkidScans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DriverFirstName",
                table: "tblShipmentLoadSessions",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverLastName",
                table: "tblShipmentLoadSessions",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LpCode",
                table: "tblShipmentLoadSessions",
                type: "nvarchar(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PickupDateTime",
                table: "tblShipmentLoadSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Run",
                table: "tblShipmentLoadSessions",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierCode",
                table: "tblShipmentLoadSessions",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierFirstName",
                table: "tblShipmentLoadSessions",
                type: "nvarchar(9)",
                maxLength: 9,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierLastName",
                table: "tblShipmentLoadSessions",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToyotaErrorMessage",
                table: "tblShipmentLoadSessions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToyotaStatus",
                table: "tblShipmentLoadSessions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ToyotaSubmittedAt",
                table: "tblShipmentLoadSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShipmentLoadSessionId",
                table: "tblOrders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblOrders_ShipmentLoadSessionId",
                table: "tblOrders",
                column: "ShipmentLoadSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_tblOrders_tblShipmentLoadSessions_ShipmentLoadSessionId",
                table: "tblOrders",
                column: "ShipmentLoadSessionId",
                principalTable: "tblShipmentLoadSessions",
                principalColumn: "SessionId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tblOrders_tblShipmentLoadSessions_ShipmentLoadSessionId",
                table: "tblOrders");

            migrationBuilder.DropIndex(
                name: "IX_tblOrders_ShipmentLoadSessionId",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "IsSkidCut",
                table: "tblSkidScans");

            migrationBuilder.DropColumn(
                name: "DriverFirstName",
                table: "tblShipmentLoadSessions");

            migrationBuilder.DropColumn(
                name: "DriverLastName",
                table: "tblShipmentLoadSessions");

            migrationBuilder.DropColumn(
                name: "LpCode",
                table: "tblShipmentLoadSessions");

            migrationBuilder.DropColumn(
                name: "PickupDateTime",
                table: "tblShipmentLoadSessions");

            migrationBuilder.DropColumn(
                name: "Run",
                table: "tblShipmentLoadSessions");

            migrationBuilder.DropColumn(
                name: "SupplierCode",
                table: "tblShipmentLoadSessions");

            migrationBuilder.DropColumn(
                name: "SupplierFirstName",
                table: "tblShipmentLoadSessions");

            migrationBuilder.DropColumn(
                name: "SupplierLastName",
                table: "tblShipmentLoadSessions");

            migrationBuilder.DropColumn(
                name: "ToyotaErrorMessage",
                table: "tblShipmentLoadSessions");

            migrationBuilder.DropColumn(
                name: "ToyotaStatus",
                table: "tblShipmentLoadSessions");

            migrationBuilder.DropColumn(
                name: "ToyotaSubmittedAt",
                table: "tblShipmentLoadSessions");

            migrationBuilder.DropColumn(
                name: "ShipmentLoadSessionId",
                table: "tblOrders");

            migrationBuilder.RenameColumn(
                name: "ToyotaConfirmationNumber",
                table: "tblShipmentLoadSessions",
                newName: "ConfirmationNumber");

            migrationBuilder.AddColumn<string>(
                name: "CarrierName",
                table: "tblShipmentLoadSessions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentScreen",
                table: "tblShipmentLoadSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DriverName",
                table: "tblShipmentLoadSessions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "tblShipmentLoadSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "tblShipmentLoadSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WarehouseId",
                table: "tblShipmentLoadSessions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tblScannedSkids",
                columns: table => new
                {
                    ScannedSkidId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Destination = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PartCount = table.Column<int>(type: "int", nullable: true),
                    ScannedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SkidId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblScannedSkids", x => x.ScannedSkidId);
                    table.ForeignKey(
                        name: "FK_tblScannedSkids_tblShipmentLoadSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "tblShipmentLoadSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tblScannedSkids_SessionId",
                table: "tblScannedSkids",
                column: "SessionId");
        }
    }
}
