using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderAndPlannedItemStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tblOrders_OrderSeries_DockCode_OrderNumber",
                table: "tblOrders");

            migrationBuilder.DropIndex(
                name: "IX_tblOrders_RealOrderNumber",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "PartDescription",
                table: "tblPlannedItems");

            migrationBuilder.DropColumn(
                name: "ArriveDate",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "ArriveTime",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "DepartDate",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "DepartTime",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "OrderSeries",
                table: "tblOrders");

            migrationBuilder.RenameColumn(
                name: "SupplierName",
                table: "tblOrders",
                newName: "SubStatDescription");

            migrationBuilder.RenameColumn(
                name: "SkidId",
                table: "tblOrders",
                newName: "ShipmentSubStatus");

            migrationBuilder.AddColumn<long>(
                name: "ManifestNo",
                table: "tblPlannedItems",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "ShortOver",
                table: "tblPlannedItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AsnStatus",
                table: "tblOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AsnStatusDescription",
                table: "tblOrders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedPickup",
                table: "tblOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ScsProcessStage",
                table: "tblOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShipmentUpdateBy",
                table: "tblOrders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShipmentUpdateDate",
                table: "tblOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblOrders_ManifestNo",
                table: "tblOrders",
                column: "ManifestNo");

            migrationBuilder.CreateIndex(
                name: "IX_tblOrders_RealOrderNumber_DockCode",
                table: "tblOrders",
                columns: new[] { "RealOrderNumber", "DockCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tblOrders_ManifestNo",
                table: "tblOrders");

            migrationBuilder.DropIndex(
                name: "IX_tblOrders_RealOrderNumber_DockCode",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "ManifestNo",
                table: "tblPlannedItems");

            migrationBuilder.DropColumn(
                name: "ShortOver",
                table: "tblPlannedItems");

            migrationBuilder.DropColumn(
                name: "AsnStatus",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "AsnStatusDescription",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "PlannedPickup",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "ScsProcessStage",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "ShipmentUpdateBy",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "ShipmentUpdateDate",
                table: "tblOrders");

            migrationBuilder.RenameColumn(
                name: "SubStatDescription",
                table: "tblOrders",
                newName: "SupplierName");

            migrationBuilder.RenameColumn(
                name: "ShipmentSubStatus",
                table: "tblOrders",
                newName: "SkidId");

            migrationBuilder.AddColumn<string>(
                name: "PartDescription",
                table: "tblPlannedItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ArriveDate",
                table: "tblOrders",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "ArriveTime",
                table: "tblOrders",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DepartDate",
                table: "tblOrders",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "DepartTime",
                table: "tblOrders",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderNumber",
                table: "tblOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OrderSeries",
                table: "tblOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_tblOrders_OrderSeries_DockCode_OrderNumber",
                table: "tblOrders",
                columns: new[] { "OrderSeries", "DockCode", "OrderNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblOrders_RealOrderNumber",
                table: "tblOrders",
                column: "RealOrderNumber");
        }
    }
}
