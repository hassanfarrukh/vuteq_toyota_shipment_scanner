using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddInternalKanbanSerialToSkidScan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblInternalKanbanSettings");

            migrationBuilder.AddColumn<string>(
                name: "InternalKanbanSerial",
                table: "tblSkidScans",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InternalKanbanSerial",
                table: "tblSkidScans");

            migrationBuilder.CreateTable(
                name: "tblInternalKanbanSettings",
                columns: table => new
                {
                    SettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlertOnDuplicate = table.Column<bool>(type: "bit", nullable: false),
                    AllowDuplicates = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DuplicateWindow = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblInternalKanbanSettings", x => x.SettingId);
                });
        }
    }
}
