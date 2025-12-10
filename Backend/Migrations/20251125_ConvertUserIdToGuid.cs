// Author: Hassan
// Date: 2025-11-25
// Description: Migration to convert UserMaster.UserId from string to Guid and update all foreign key references

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class ConvertUserIdToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add temporary columns to store new Guid values
            migrationBuilder.AddColumn<Guid>(
                name: "UserId_New",
                table: "tblUserMaster",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId_New",
                table: "tblUserSessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UploadedBy_New",
                table: "tblOrderUploads",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId_New",
                table: "tblSkidBuildSessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId_New",
                table: "tblShipmentLoadSessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId_New",
                table: "tblPreShipmentShipments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId_New",
                table: "tblSkidBuildDrafts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId_New",
                table: "tblShipmentLoadDrafts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId_New",
                table: "tblSettings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId_New",
                table: "tblDockMonitorSettings",
                type: "uniqueidentifier",
                nullable: true);

            // Step 2: Migrate existing admin user to a known Guid
            migrationBuilder.Sql(@"
                -- Create a mapping for existing users
                DECLARE @AdminGuid UNIQUEIDENTIFIER = NEWID();

                -- Update UserMaster with new Guid for admin
                UPDATE tblUserMaster
                SET UserId_New = @AdminGuid
                WHERE UserId = 'admin';

                -- Update all foreign key references
                UPDATE tblUserSessions
                SET UserId_New = @AdminGuid
                WHERE UserId = 'admin';

                UPDATE tblOrderUploads
                SET UploadedBy_New = @AdminGuid
                WHERE UploadedBy = 'admin';

                UPDATE tblSkidBuildSessions
                SET UserId_New = @AdminGuid
                WHERE UserId = 'admin';

                UPDATE tblShipmentLoadSessions
                SET UserId_New = @AdminGuid
                WHERE UserId = 'admin';

                UPDATE tblPreShipmentShipments
                SET CreatedByUserId_New = @AdminGuid
                WHERE CreatedByUserId = 'admin';

                UPDATE tblSkidBuildDrafts
                SET UserId_New = @AdminGuid
                WHERE UserId = 'admin';

                UPDATE tblShipmentLoadDrafts
                SET UserId_New = @AdminGuid
                WHERE UserId = 'admin';

                UPDATE tblSettings
                SET UserId_New = @AdminGuid
                WHERE UserId = 'admin';

                UPDATE tblDockMonitorSettings
                SET UserId_New = @AdminGuid
                WHERE UserId = 'admin';
            ");

            // Step 3: Drop foreign key constraints
            migrationBuilder.DropForeignKey(
                name: "FK_tblUserSessions_tblUserMaster_UserId",
                table: "tblUserSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_tblOrderUploads_tblUserMaster_UploadedBy",
                table: "tblOrderUploads");

            migrationBuilder.DropForeignKey(
                name: "FK_tblSkidBuildSessions_tblUserMaster_UserId",
                table: "tblSkidBuildSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_tblShipmentLoadSessions_tblUserMaster_UserId",
                table: "tblShipmentLoadSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_tblPreShipmentShipments_tblUserMaster_CreatedByUserId",
                table: "tblPreShipmentShipments");

            migrationBuilder.DropForeignKey(
                name: "FK_tblSkidBuildDrafts_tblUserMaster_UserId",
                table: "tblSkidBuildDrafts");

            migrationBuilder.DropForeignKey(
                name: "FK_tblShipmentLoadDrafts_tblUserMaster_UserId",
                table: "tblShipmentLoadDrafts");

            migrationBuilder.DropForeignKey(
                name: "FK_tblSettings_tblUserMaster_UserId",
                table: "tblSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_tblDockMonitorSettings_tblUserMaster_UserId",
                table: "tblDockMonitorSettings");

            // Step 4: Drop old columns
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "tblUserMaster");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "tblUserSessions");

            migrationBuilder.DropColumn(
                name: "UploadedBy",
                table: "tblOrderUploads");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "tblSkidBuildSessions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "tblShipmentLoadSessions");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "tblPreShipmentShipments");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "tblSkidBuildDrafts");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "tblShipmentLoadDrafts");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "tblSettings");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "tblDockMonitorSettings");

            // Step 5: Rename new columns to original names
            migrationBuilder.RenameColumn(
                name: "UserId_New",
                table: "tblUserMaster",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "UserId_New",
                table: "tblUserSessions",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "UploadedBy_New",
                table: "tblOrderUploads",
                newName: "UploadedBy");

            migrationBuilder.RenameColumn(
                name: "UserId_New",
                table: "tblSkidBuildSessions",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "UserId_New",
                table: "tblShipmentLoadSessions",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId_New",
                table: "tblPreShipmentShipments",
                newName: "CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "UserId_New",
                table: "tblSkidBuildDrafts",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "UserId_New",
                table: "tblShipmentLoadDrafts",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "UserId_New",
                table: "tblSettings",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "UserId_New",
                table: "tblDockMonitorSettings",
                newName: "UserId");

            // Step 6: Make non-nullable columns NOT NULL
            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "tblUserSessions",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "tblSkidBuildSessions",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "tblShipmentLoadSessions",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedByUserId",
                table: "tblPreShipmentShipments",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "tblSkidBuildDrafts",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "tblShipmentLoadDrafts",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "tblDockMonitorSettings",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            // Step 7: Recreate foreign key constraints
            migrationBuilder.CreateIndex(
                name: "IX_tblUserSessions_UserId",
                table: "tblUserSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tblOrderUploads_UploadedBy",
                table: "tblOrderUploads",
                column: "UploadedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tblSkidBuildSessions_UserId",
                table: "tblSkidBuildSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tblShipmentLoadSessions_UserId",
                table: "tblShipmentLoadSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tblPreShipmentShipments_CreatedByUserId",
                table: "tblPreShipmentShipments",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_tblSkidBuildDrafts_UserId",
                table: "tblSkidBuildDrafts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tblShipmentLoadDrafts_UserId",
                table: "tblShipmentLoadDrafts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tblSettings_UserId",
                table: "tblSettings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tblDockMonitorSettings_UserId",
                table: "tblDockMonitorSettings",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_tblUserSessions_tblUserMaster_UserId",
                table: "tblUserSessions",
                column: "UserId",
                principalTable: "tblUserMaster",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tblOrderUploads_tblUserMaster_UploadedBy",
                table: "tblOrderUploads",
                column: "UploadedBy",
                principalTable: "tblUserMaster",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_tblSkidBuildSessions_tblUserMaster_UserId",
                table: "tblSkidBuildSessions",
                column: "UserId",
                principalTable: "tblUserMaster",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tblShipmentLoadSessions_tblUserMaster_UserId",
                table: "tblShipmentLoadSessions",
                column: "UserId",
                principalTable: "tblUserMaster",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tblPreShipmentShipments_tblUserMaster_CreatedByUserId",
                table: "tblPreShipmentShipments",
                column: "CreatedByUserId",
                principalTable: "tblUserMaster",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tblSkidBuildDrafts_tblUserMaster_UserId",
                table: "tblSkidBuildDrafts",
                column: "UserId",
                principalTable: "tblUserMaster",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tblShipmentLoadDrafts_tblUserMaster_UserId",
                table: "tblShipmentLoadDrafts",
                column: "UserId",
                principalTable: "tblUserMaster",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_tblSettings_tblUserMaster_UserId",
                table: "tblSettings",
                column: "UserId",
                principalTable: "tblUserMaster",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_tblDockMonitorSettings_tblUserMaster_UserId",
                table: "tblDockMonitorSettings",
                column: "UserId",
                principalTable: "tblUserMaster",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Note: Downgrade is complex as it requires converting Guid back to string
            // This is a destructive operation and should be carefully considered
            throw new NotSupportedException("Downgrading from Guid to string UserId is not supported.");
        }
    }
}
