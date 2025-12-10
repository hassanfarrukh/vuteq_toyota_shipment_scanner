using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "tblUserMaster",
                columns: new[] { "UserId", "Code", "CreatedAt", "CreatedBy", "Email", "IsActive", "IsSupervisor", "LastLoginAt", "LocationId", "MenuLevel", "Name", "Operation", "PasswordHash", "Role", "UpdatedAt", "UpdatedBy", "Username" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), null, new DateTime(2025, 11, 25, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, true, true, null, null, "Admin", "CISG", "Administration", "0FdNYlEZUSnULgc/q4ufuRnNQMRBW2eJwch7tEAkcho=", "Admin", null, null, "cisg" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "tblUserMaster",
                keyColumn: "UserId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));
        }
    }
}
