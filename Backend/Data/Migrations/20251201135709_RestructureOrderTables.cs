using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class RestructureOrderTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tblPlannedItems_tblOrders_OwkNumber",
                table: "tblPlannedItems");

            migrationBuilder.DropForeignKey(
                name: "FK_tblSkidBuildSessions_tblOrders_OwkNumber",
                table: "tblSkidBuildSessions");

            migrationBuilder.DropIndex(
                name: "IX_tblSkidBuildSessions_OwkNumber",
                table: "tblSkidBuildSessions");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_tblOrders_OwkNumber",
                table: "tblOrders");

            migrationBuilder.DropIndex(
                name: "IX_tblOrders_OwkNumber",
                table: "tblOrders");

            migrationBuilder.DropIndex(
                name: "IX_tblOrders_Status",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "OwkNumber",
                table: "tblSkidBuildSessions");

            migrationBuilder.DropColumn(
                name: "RawKanbanValue",
                table: "tblPlannedItems");

            migrationBuilder.DropColumn(
                name: "SupplierCode",
                table: "tblPlannedItems");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "LoadId",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "PlantCode",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "TotalSkids",
                table: "tblOrders");

            migrationBuilder.RenameColumn(
                name: "PlannedQty",
                table: "tblPlannedItems",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "OwkNumber",
                table: "tblPlannedItems",
                newName: "OrderNumber");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "tblPlannedItems",
                newName: "PartDescription");

            migrationBuilder.RenameIndex(
                name: "IX_tblPlannedItems_OwkNumber",
                table: "tblPlannedItems",
                newName: "IX_tblPlannedItems_OrderNumber");

            migrationBuilder.RenameColumn(
                name: "OwkNumber",
                table: "tblOrders",
                newName: "OrderSeries");

            migrationBuilder.RenameColumn(
                name: "OrderDate",
                table: "tblOrders",
                newName: "TransmitDate");

            migrationBuilder.RenameColumn(
                name: "Destination",
                table: "tblOrders",
                newName: "SupplierName");

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "tblSkidBuildSessions",
                type: "uniqueidentifier",
                nullable: true);

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
                name: "KanbanNumber",
                table: "tblPlannedItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LotOrdered",
                table: "tblPlannedItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LotQty",
                table: "tblPlannedItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "tblPlannedItems",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "SkidId",
                table: "tblPlannedItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

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

            migrationBuilder.AlterColumn<string>(
                name: "DockCode",
                table: "tblOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "tblOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_tblSkidBuildSessions_OrderId",
                table: "tblSkidBuildSessions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_tblPlannedItems_IsActive",
                table: "tblPlannedItems",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_tblPlannedItems_OrderId",
                table: "tblPlannedItems",
                column: "OrderId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_tblPlannedItems_tblOrders_OrderId",
                table: "tblPlannedItems",
                column: "OrderId",
                principalTable: "tblOrders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tblSkidBuildSessions_tblOrders_OrderId",
                table: "tblSkidBuildSessions",
                column: "OrderId",
                principalTable: "tblOrders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tblPlannedItems_tblOrders_OrderId",
                table: "tblPlannedItems");

            migrationBuilder.DropForeignKey(
                name: "FK_tblSkidBuildSessions_tblOrders_OrderId",
                table: "tblSkidBuildSessions");

            migrationBuilder.DropIndex(
                name: "IX_tblSkidBuildSessions_OrderId",
                table: "tblSkidBuildSessions");

            migrationBuilder.DropIndex(
                name: "IX_tblPlannedItems_IsActive",
                table: "tblPlannedItems");

            migrationBuilder.DropIndex(
                name: "IX_tblPlannedItems_OrderId",
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
                name: "OrderId",
                table: "tblSkidBuildSessions");

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
                name: "KanbanNumber",
                table: "tblPlannedItems");

            migrationBuilder.DropColumn(
                name: "LotOrdered",
                table: "tblPlannedItems");

            migrationBuilder.DropColumn(
                name: "LotQty",
                table: "tblPlannedItems");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "tblPlannedItems");

            migrationBuilder.DropColumn(
                name: "SkidId",
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

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "tblPlannedItems",
                newName: "PlannedQty");

            migrationBuilder.RenameColumn(
                name: "PartDescription",
                table: "tblPlannedItems",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "OrderNumber",
                table: "tblPlannedItems",
                newName: "OwkNumber");

            migrationBuilder.RenameIndex(
                name: "IX_tblPlannedItems_OrderNumber",
                table: "tblPlannedItems",
                newName: "IX_tblPlannedItems_OwkNumber");

            migrationBuilder.RenameColumn(
                name: "TransmitDate",
                table: "tblOrders",
                newName: "OrderDate");

            migrationBuilder.RenameColumn(
                name: "SupplierName",
                table: "tblOrders",
                newName: "Destination");

            migrationBuilder.RenameColumn(
                name: "OrderSeries",
                table: "tblOrders",
                newName: "OwkNumber");

            migrationBuilder.AddColumn<string>(
                name: "OwkNumber",
                table: "tblSkidBuildSessions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RawKanbanValue",
                table: "tblPlannedItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierCode",
                table: "tblPlannedItems",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DockCode",
                table: "tblOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "tblOrders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoadId",
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

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "tblOrders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalSkids",
                table: "tblOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_tblOrders_OwkNumber",
                table: "tblOrders",
                column: "OwkNumber");

            migrationBuilder.CreateIndex(
                name: "IX_tblSkidBuildSessions_OwkNumber",
                table: "tblSkidBuildSessions",
                column: "OwkNumber");

            migrationBuilder.CreateIndex(
                name: "IX_tblOrders_OwkNumber",
                table: "tblOrders",
                column: "OwkNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblOrders_Status",
                table: "tblOrders",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_tblPlannedItems_tblOrders_OwkNumber",
                table: "tblPlannedItems",
                column: "OwkNumber",
                principalTable: "tblOrders",
                principalColumn: "OwkNumber",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tblSkidBuildSessions_tblOrders_OwkNumber",
                table: "tblSkidBuildSessions",
                column: "OwkNumber",
                principalTable: "tblOrders",
                principalColumn: "OwkNumber",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
