using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddInternalKanbanExclusionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tblInternalKanbanExclusions",
                columns: table => new
                {
                    ExclusionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsExcluded = table.Column<bool>(type: "bit", nullable: false),
                    Mode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblInternalKanbanExclusions", x => x.ExclusionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tblInternalKanbanExclusions_CreatedAt",
                table: "tblInternalKanbanExclusions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_tblInternalKanbanExclusions_CreatedBy",
                table: "tblInternalKanbanExclusions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tblInternalKanbanExclusions_IsExcluded",
                table: "tblInternalKanbanExclusions",
                column: "IsExcluded");

            migrationBuilder.CreateIndex(
                name: "IX_tblInternalKanbanExclusions_PartNumber",
                table: "tblInternalKanbanExclusions",
                column: "PartNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblInternalKanbanExclusions");
        }
    }
}
