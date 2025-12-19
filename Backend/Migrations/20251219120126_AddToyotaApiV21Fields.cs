// Author: Hassan
// Date: 2025-12-19
// Description: Add ResourceUrl and XClientId columns for Toyota API V2.1 upgrade

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddToyotaApiV21Fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResourceUrl",
                table: "tblToyotaApiConfig",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "XClientId",
                table: "tblToyotaApiConfig",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResourceUrl",
                table: "tblToyotaApiConfig");

            migrationBuilder.DropColumn(
                name: "XClientId",
                table: "tblToyotaApiConfig");
        }
    }
}
