using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionIdToSkidBuildException : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tblSkidBuildExceptions_tblSkidBuildSessions_SkidBuildSessionSessionId",
                table: "tblSkidBuildExceptions");

            migrationBuilder.RenameColumn(
                name: "SkidBuildSessionSessionId",
                table: "tblSkidBuildExceptions",
                newName: "SessionId");

            migrationBuilder.RenameIndex(
                name: "IX_tblSkidBuildExceptions_SkidBuildSessionSessionId",
                table: "tblSkidBuildExceptions",
                newName: "IX_tblSkidBuildExceptions_SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_tblSkidBuildExceptions_tblSkidBuildSessions_SessionId",
                table: "tblSkidBuildExceptions",
                column: "SessionId",
                principalTable: "tblSkidBuildSessions",
                principalColumn: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tblSkidBuildExceptions_tblSkidBuildSessions_SessionId",
                table: "tblSkidBuildExceptions");

            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "tblSkidBuildExceptions",
                newName: "SkidBuildSessionSessionId");

            migrationBuilder.RenameIndex(
                name: "IX_tblSkidBuildExceptions_SessionId",
                table: "tblSkidBuildExceptions",
                newName: "IX_tblSkidBuildExceptions_SkidBuildSessionSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_tblSkidBuildExceptions_tblSkidBuildSessions_SkidBuildSessionSessionId",
                table: "tblSkidBuildExceptions",
                column: "SkidBuildSessionSessionId",
                principalTable: "tblSkidBuildSessions",
                principalColumn: "SessionId");
        }
    }
}
