using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class RestructureOrdersV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tblPlannedItems_IsActive",
                table: "tblPlannedItems");

            migrationBuilder.DropIndex(
                name: "IX_tblPlannedItems_OrderNumber",
                table: "tblPlannedItems");

            migrationBuilder.DropIndex(
                name: "IX_tblPlannedItems_Status",
                table: "tblPlannedItems");

            migrationBuilder.DropIndex(
                name: "IX_tblOrders_IsActive",
                table: "tblOrders");

            migrationBuilder.DropIndex(
                name: "IX_tblOrders_OrderSeries_DockCode",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "ArriveDate",
                table: "tblPlannedItems");

            migrationBuilder.DropColumn(
                name: "ArriveTime",
                table: "tblPlannedItems");

            migrationBuilder.DropColumn(
                name: "DepartDate",
                table: "tblPlannedItems");

            migrationBuilder.DropColumn(
                name: "DepartTime",
                table: "tblPlannedItems");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "tblPlannedItems");

            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "tblPlannedItems");

            migrationBuilder.DropColumn(
                name: "SkidId",
                table: "tblPlannedItems");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "tblPlannedItems");

            migrationBuilder.DropColumn(
                name: "UnloadDate",
                table: "tblPlannedItems");

            migrationBuilder.DropColumn(
                name: "UnloadTime",
                table: "tblPlannedItems");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "tblOrders");

            migrationBuilder.AddColumn<int>(
                name: "OrdersCreated",
                table: "tblOrderUploads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalItemsCreated",
                table: "tblOrderUploads",
                type: "int",
                nullable: false,
                defaultValue: 0);

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
                name: "RealOrderNumber",
                table: "tblOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SkidId",
                table: "tblOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "tblOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "UnloadDate",
                table: "tblOrders",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "UnloadTime",
                table: "tblOrders",
                type: "time",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblOrders_OrderSeries_DockCode_OrderNumber",
                table: "tblOrders",
                columns: new[] { "OrderSeries", "DockCode", "OrderNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblOrders_RealOrderNumber",
                table: "tblOrders",
                column: "RealOrderNumber");

            migrationBuilder.CreateIndex(
                name: "IX_tblOrders_Status",
                table: "tblOrders",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tblOrders_OrderSeries_DockCode_OrderNumber",
                table: "tblOrders");

            migrationBuilder.DropIndex(
                name: "IX_tblOrders_RealOrderNumber",
                table: "tblOrders");

            migrationBuilder.DropIndex(
                name: "IX_tblOrders_Status",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "OrdersCreated",
                table: "tblOrderUploads");

            migrationBuilder.DropColumn(
                name: "TotalItemsCreated",
                table: "tblOrderUploads");

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
                name: "RealOrderNumber",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "SkidId",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "UnloadDate",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "UnloadTime",
                table: "tblOrders");

            migrationBuilder.AddColumn<DateOnly>(
                name: "ArriveDate",
                table: "tblPlannedItems",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "ArriveTime",
                table: "tblPlannedItems",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DepartDate",
                table: "tblPlannedItems",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "DepartTime",
                table: "tblPlannedItems",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "tblPlannedItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OrderNumber",
                table: "tblPlannedItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SkidId",
                table: "tblPlannedItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "tblPlannedItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "UnloadDate",
                table: "tblPlannedItems",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "UnloadTime",
                table: "tblPlannedItems",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "tblOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_tblPlannedItems_IsActive",
                table: "tblPlannedItems",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_tblPlannedItems_OrderNumber",
                table: "tblPlannedItems",
                column: "OrderNumber");

            migrationBuilder.CreateIndex(
                name: "IX_tblPlannedItems_Status",
                table: "tblPlannedItems",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_tblOrders_IsActive",
                table: "tblOrders",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_tblOrders_OrderSeries_DockCode",
                table: "tblOrders",
                columns: new[] { "OrderSeries", "DockCode" },
                unique: true);
        }
    }
}
