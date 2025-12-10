using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class ExcelOrderUploadFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LotQty",
                table: "tblPlannedItems",
                newName: "Qpc");

            migrationBuilder.RenameColumn(
                name: "LotOrdered",
                table: "tblPlannedItems",
                newName: "TotalBoxPlanned");

            migrationBuilder.AddColumn<long>(
                name: "ExternalOrderId",
                table: "tblPlannedItems",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "PalletizationCode",
                table: "tblPlannedItems",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SkidUid",
                table: "tblPlannedItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlantCode",
                table: "tblOrderUploads",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplierCode",
                table: "tblOrderUploads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalLate",
                table: "tblOrderUploads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalPending",
                table: "tblOrderUploads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalPlanned",
                table: "tblOrderUploads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalShipped",
                table: "tblOrderUploads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalShorted",
                table: "tblOrderUploads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualPickupDate",
                table: "tblOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActualRoute",
                table: "tblOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MainRoute",
                table: "tblOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ManifestNo",
                table: "tblOrders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "Mros",
                table: "tblOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlannedRoute",
                table: "tblOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlantCode",
                table: "tblOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SpecialistCode",
                table: "tblOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Trailer",
                table: "tblOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalOrderId",
                table: "tblPlannedItems");

            migrationBuilder.DropColumn(
                name: "PalletizationCode",
                table: "tblPlannedItems");

            migrationBuilder.DropColumn(
                name: "SkidUid",
                table: "tblPlannedItems");

            migrationBuilder.DropColumn(
                name: "PlantCode",
                table: "tblOrderUploads");

            migrationBuilder.DropColumn(
                name: "SupplierCode",
                table: "tblOrderUploads");

            migrationBuilder.DropColumn(
                name: "TotalLate",
                table: "tblOrderUploads");

            migrationBuilder.DropColumn(
                name: "TotalPending",
                table: "tblOrderUploads");

            migrationBuilder.DropColumn(
                name: "TotalPlanned",
                table: "tblOrderUploads");

            migrationBuilder.DropColumn(
                name: "TotalShipped",
                table: "tblOrderUploads");

            migrationBuilder.DropColumn(
                name: "TotalShorted",
                table: "tblOrderUploads");

            migrationBuilder.DropColumn(
                name: "ActualPickupDate",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "ActualRoute",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "MainRoute",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "ManifestNo",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "Mros",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "PlannedRoute",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "PlantCode",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "SpecialistCode",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "Trailer",
                table: "tblOrders");

            migrationBuilder.RenameColumn(
                name: "Qpc",
                table: "tblPlannedItems",
                newName: "LotQty");

            migrationBuilder.RenameColumn(
                name: "TotalBoxPlanned",
                table: "tblPlannedItems",
                newName: "LotOrdered");
        }
    }
}
