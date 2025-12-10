using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class SkidBuildDatabaseChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tblSkidBuildExceptions_tblSkidBuildSessions_SessionId",
                table: "tblSkidBuildExceptions");

            migrationBuilder.DropIndex(
                name: "IX_tblOrders_ManifestNo",
                table: "tblOrders");

            migrationBuilder.DropColumn(
                name: "CreatedByUser",
                table: "tblSkidBuildExceptions");

            migrationBuilder.DropColumn(
                name: "ExceptionType",
                table: "tblSkidBuildExceptions");

            migrationBuilder.DropColumn(
                name: "OwkNumber",
                table: "tblSkidBuildExceptions");

            migrationBuilder.DropColumn(
                name: "InternalKanban",
                table: "tblPlannedItems");

            migrationBuilder.DropColumn(
                name: "SkidUid",
                table: "tblPlannedItems");

            migrationBuilder.DropColumn(
                name: "ManifestNo",
                table: "tblOrders");

            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "tblSkidBuildExceptions",
                newName: "OrderId");

            migrationBuilder.RenameIndex(
                name: "IX_tblSkidBuildExceptions_SessionId",
                table: "tblSkidBuildExceptions",
                newName: "IX_tblSkidBuildExceptions_OrderId");

            migrationBuilder.AlterColumn<string>(
                name: "Comments",
                table: "tblSkidBuildExceptions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "tblSkidBuildExceptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExceptionCode",
                table: "tblSkidBuildExceptions",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "SkidBuildSessionSessionId",
                table: "tblSkidBuildExceptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SkidNumber",
                table: "tblSkidBuildExceptions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tblSkidScans",
                columns: table => new
                {
                    ScanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlannedItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkidNumber = table.Column<int>(type: "int", nullable: false),
                    BoxNumber = table.Column<int>(type: "int", nullable: false),
                    LineSideAddress = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    InternalKanban = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ScannedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScannedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblSkidScans", x => x.ScanId);
                    table.ForeignKey(
                        name: "FK_tblSkidScans_tblPlannedItems_PlannedItemId",
                        column: x => x.PlannedItemId,
                        principalTable: "tblPlannedItems",
                        principalColumn: "PlannedItemId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tblSkidScans_tblUserMaster_ScannedBy",
                        column: x => x.ScannedBy,
                        principalTable: "tblUserMaster",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tblSkidBuildExceptions_CreatedByUserId",
                table: "tblSkidBuildExceptions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_tblSkidBuildExceptions_ExceptionCode",
                table: "tblSkidBuildExceptions",
                column: "ExceptionCode");

            migrationBuilder.CreateIndex(
                name: "IX_tblSkidBuildExceptions_SkidBuildSessionSessionId",
                table: "tblSkidBuildExceptions",
                column: "SkidBuildSessionSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_tblSkidScans_PlannedItemId",
                table: "tblSkidScans",
                column: "PlannedItemId");

            migrationBuilder.CreateIndex(
                name: "IX_tblSkidScans_ScannedAt",
                table: "tblSkidScans",
                column: "ScannedAt");

            migrationBuilder.CreateIndex(
                name: "IX_tblSkidScans_ScannedBy",
                table: "tblSkidScans",
                column: "ScannedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tblSkidScans_SkidNumber",
                table: "tblSkidScans",
                column: "SkidNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_tblSkidBuildExceptions_tblOrders_OrderId",
                table: "tblSkidBuildExceptions",
                column: "OrderId",
                principalTable: "tblOrders",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tblSkidBuildExceptions_tblSkidBuildSessions_SkidBuildSessionSessionId",
                table: "tblSkidBuildExceptions",
                column: "SkidBuildSessionSessionId",
                principalTable: "tblSkidBuildSessions",
                principalColumn: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_tblSkidBuildExceptions_tblUserMaster_CreatedByUserId",
                table: "tblSkidBuildExceptions",
                column: "CreatedByUserId",
                principalTable: "tblUserMaster",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tblSkidBuildExceptions_tblOrders_OrderId",
                table: "tblSkidBuildExceptions");

            migrationBuilder.DropForeignKey(
                name: "FK_tblSkidBuildExceptions_tblSkidBuildSessions_SkidBuildSessionSessionId",
                table: "tblSkidBuildExceptions");

            migrationBuilder.DropForeignKey(
                name: "FK_tblSkidBuildExceptions_tblUserMaster_CreatedByUserId",
                table: "tblSkidBuildExceptions");

            migrationBuilder.DropTable(
                name: "tblSkidScans");

            migrationBuilder.DropIndex(
                name: "IX_tblSkidBuildExceptions_CreatedByUserId",
                table: "tblSkidBuildExceptions");

            migrationBuilder.DropIndex(
                name: "IX_tblSkidBuildExceptions_ExceptionCode",
                table: "tblSkidBuildExceptions");

            migrationBuilder.DropIndex(
                name: "IX_tblSkidBuildExceptions_SkidBuildSessionSessionId",
                table: "tblSkidBuildExceptions");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "tblSkidBuildExceptions");

            migrationBuilder.DropColumn(
                name: "ExceptionCode",
                table: "tblSkidBuildExceptions");

            migrationBuilder.DropColumn(
                name: "SkidBuildSessionSessionId",
                table: "tblSkidBuildExceptions");

            migrationBuilder.DropColumn(
                name: "SkidNumber",
                table: "tblSkidBuildExceptions");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "tblSkidBuildExceptions",
                newName: "SessionId");

            migrationBuilder.RenameIndex(
                name: "IX_tblSkidBuildExceptions_OrderId",
                table: "tblSkidBuildExceptions",
                newName: "IX_tblSkidBuildExceptions_SessionId");

            migrationBuilder.AlterColumn<string>(
                name: "Comments",
                table: "tblSkidBuildExceptions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUser",
                table: "tblSkidBuildExceptions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExceptionType",
                table: "tblSkidBuildExceptions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwkNumber",
                table: "tblSkidBuildExceptions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InternalKanban",
                table: "tblPlannedItems",
                type: "nvarchar(24)",
                maxLength: 24,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SkidUid",
                table: "tblPlannedItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ManifestNo",
                table: "tblOrders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_tblOrders_ManifestNo",
                table: "tblOrders",
                column: "ManifestNo");

            migrationBuilder.AddForeignKey(
                name: "FK_tblSkidBuildExceptions_tblSkidBuildSessions_SessionId",
                table: "tblSkidBuildExceptions",
                column: "SessionId",
                principalTable: "tblSkidBuildSessions",
                principalColumn: "SessionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
