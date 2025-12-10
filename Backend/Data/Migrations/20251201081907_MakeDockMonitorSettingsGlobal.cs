using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeDockMonitorSettingsGlobal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tblDockMonitorSettings_tblUserMaster_UserId",
                table: "tblDockMonitorSettings");

            migrationBuilder.DropIndex(
                name: "IX_tblDockMonitorSettings_UserId",
                table: "tblDockMonitorSettings");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "tblDockMonitorSettings",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "tblDockMonitorSettings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblDockMonitorSettings_UserId",
                table: "tblDockMonitorSettings",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tblDockMonitorSettings_tblUserMaster_UserId",
                table: "tblDockMonitorSettings",
                column: "UserId",
                principalTable: "tblUserMaster",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
