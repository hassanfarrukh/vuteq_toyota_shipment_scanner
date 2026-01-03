using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteSettingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tblSiteSettings",
                columns: table => new
                {
                    SettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlantLocation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PlantOpeningTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    PlantClosingTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    EnablePreShipmentScan = table.Column<bool>(type: "bit", nullable: false),
                    DockBehindThreshold = table.Column<int>(type: "int", nullable: false),
                    DockCriticalThreshold = table.Column<int>(type: "int", nullable: false),
                    DockDisplayMode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DockRefreshInterval = table.Column<int>(type: "int", nullable: false),
                    DockOrderLookbackHours = table.Column<int>(type: "int", nullable: false),
                    KanbanAllowDuplicates = table.Column<bool>(type: "bit", nullable: false),
                    KanbanDuplicateWindowHours = table.Column<int>(type: "int", nullable: false),
                    KanbanAlertOnDuplicate = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblSiteSettings", x => x.SettingId);
                    table.ForeignKey(
                        name: "FK_tblSiteSettings_tblUserMaster_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "tblUserMaster",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_tblSiteSettings_tblUserMaster_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "tblUserMaster",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_tblSiteSettings_CreatedBy",
                table: "tblSiteSettings",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tblSiteSettings_UpdatedBy",
                table: "tblSiteSettings",
                column: "UpdatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblSiteSettings");
        }
    }
}
