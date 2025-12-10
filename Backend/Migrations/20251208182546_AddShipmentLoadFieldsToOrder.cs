using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddShipmentLoadFieldsToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CarrierName",
                table: "tblOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverName",
                table: "tblOrders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SealNumber",
                table: "tblOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipmentConfirmation",
                table: "tblOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShipmentLoadedAt",
                table: "tblOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipmentNotes",
                table: "tblOrders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarrierName",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "DriverName",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "SealNumber",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "ShipmentConfirmation",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "ShipmentLoadedAt",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "ShipmentNotes",
                table: "tblOrders");
        }
    }
}
