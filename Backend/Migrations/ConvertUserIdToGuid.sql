-- Author: Hassan
-- Date: 2025-11-25
-- Description: SQL Migration to convert UserMaster.UserId from NVARCHAR(50) to UNIQUEIDENTIFIER
-- WARNING: This migration will convert existing user IDs. Backup your database before running!

BEGIN TRANSACTION;

PRINT 'Starting UserId conversion from NVARCHAR to UNIQUEIDENTIFIER...';

-- Step 1: Add temporary columns to store new Guid values
PRINT 'Step 1: Adding temporary columns...';

ALTER TABLE tblUserMaster ADD UserId_New UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL;
ALTER TABLE tblUserSessions ADD UserId_New UNIQUEIDENTIFIER NULL;
ALTER TABLE tblOrderUploads ADD UploadedBy_New UNIQUEIDENTIFIER NULL;
ALTER TABLE tblSkidBuildSessions ADD UserId_New UNIQUEIDENTIFIER NULL;
ALTER TABLE tblShipmentLoadSessions ADD UserId_New UNIQUEIDENTIFIER NULL;
ALTER TABLE tblPreShipmentShipments ADD CreatedByUserId_New UNIQUEIDENTIFIER NULL;
ALTER TABLE tblSkidBuildDrafts ADD UserId_New UNIQUEIDENTIFIER NULL;
ALTER TABLE tblShipmentLoadDrafts ADD UserId_New UNIQUEIDENTIFIER NULL;
ALTER TABLE tblSettings ADD UserId_New UNIQUEIDENTIFIER NULL;
ALTER TABLE tblDockMonitorSettings ADD UserId_New UNIQUEIDENTIFIER NULL;

-- Step 2: Migrate existing data
PRINT 'Step 2: Migrating existing data...';

-- Create a mapping table for existing users
CREATE TABLE #UserIdMapping (
    OldUserId NVARCHAR(50),
    NewUserId UNIQUEIDENTIFIER
);

-- Insert mappings for existing users
INSERT INTO #UserIdMapping (OldUserId, NewUserId)
SELECT UserId, UserId_New FROM tblUserMaster;

-- Update all foreign key references using the mapping
UPDATE us
SET us.UserId_New = m.NewUserId
FROM tblUserSessions us
INNER JOIN #UserIdMapping m ON us.UserId = m.OldUserId;

UPDATE ou
SET ou.UploadedBy_New = m.NewUserId
FROM tblOrderUploads ou
INNER JOIN #UserIdMapping m ON ou.UploadedBy = m.OldUserId;

UPDATE sbs
SET sbs.UserId_New = m.NewUserId
FROM tblSkidBuildSessions sbs
INNER JOIN #UserIdMapping m ON sbs.UserId = m.OldUserId;

UPDATE sls
SET sls.UserId_New = m.NewUserId
FROM tblShipmentLoadSessions sls
INNER JOIN #UserIdMapping m ON sls.UserId = m.OldUserId;

UPDATE pss
SET pss.CreatedByUserId_New = m.NewUserId
FROM tblPreShipmentShipments pss
INNER JOIN #UserIdMapping m ON pss.CreatedByUserId = m.OldUserId;

UPDATE sbd
SET sbd.UserId_New = m.NewUserId
FROM tblSkidBuildDrafts sbd
INNER JOIN #UserIdMapping m ON sbd.UserId = m.OldUserId;

UPDATE sld
SET sld.UserId_New = m.NewUserId
FROM tblShipmentLoadDrafts sld
INNER JOIN #UserIdMapping m ON sld.UserId = m.OldUserId;

UPDATE s
SET s.UserId_New = m.NewUserId
FROM tblSettings s
INNER JOIN #UserIdMapping m ON s.UserId = m.OldUserId;

UPDATE dms
SET dms.UserId_New = m.NewUserId
FROM tblDockMonitorSettings dms
INNER JOIN #UserIdMapping m ON dms.UserId = m.OldUserId;

-- Step 3: Drop foreign key constraints
PRINT 'Step 3: Dropping foreign key constraints...';

-- Drop FK constraints if they exist
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_tblUserSessions_tblUserMaster_UserId')
    ALTER TABLE tblUserSessions DROP CONSTRAINT FK_tblUserSessions_tblUserMaster_UserId;

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_tblOrderUploads_tblUserMaster_UploadedBy')
    ALTER TABLE tblOrderUploads DROP CONSTRAINT FK_tblOrderUploads_tblUserMaster_UploadedBy;

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_tblSkidBuildSessions_tblUserMaster_UserId')
    ALTER TABLE tblSkidBuildSessions DROP CONSTRAINT FK_tblSkidBuildSessions_tblUserMaster_UserId;

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_tblShipmentLoadSessions_tblUserMaster_UserId')
    ALTER TABLE tblShipmentLoadSessions DROP CONSTRAINT FK_tblShipmentLoadSessions_tblUserMaster_UserId;

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_tblPreShipmentShipments_tblUserMaster_CreatedByUserId')
    ALTER TABLE tblPreShipmentShipments DROP CONSTRAINT FK_tblPreShipmentShipments_tblUserMaster_CreatedByUserId;

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_tblSkidBuildDrafts_tblUserMaster_UserId')
    ALTER TABLE tblSkidBuildDrafts DROP CONSTRAINT FK_tblSkidBuildDrafts_tblUserMaster_UserId;

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_tblShipmentLoadDrafts_tblUserMaster_UserId')
    ALTER TABLE tblShipmentLoadDrafts DROP CONSTRAINT FK_tblShipmentLoadDrafts_tblUserMaster_UserId;

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_tblSettings_tblUserMaster_UserId')
    ALTER TABLE tblSettings DROP CONSTRAINT FK_tblSettings_tblUserMaster_UserId;

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_tblDockMonitorSettings_tblUserMaster_UserId')
    ALTER TABLE tblDockMonitorSettings DROP CONSTRAINT FK_tblDockMonitorSettings_tblUserMaster_UserId;

-- Step 4: Drop indexes
PRINT 'Step 4: Dropping indexes...';

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tblUserSessions_UserId')
    DROP INDEX IX_tblUserSessions_UserId ON tblUserSessions;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tblOrderUploads_UploadedBy')
    DROP INDEX IX_tblOrderUploads_UploadedBy ON tblOrderUploads;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tblSkidBuildSessions_UserId')
    DROP INDEX IX_tblSkidBuildSessions_UserId ON tblSkidBuildSessions;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tblShipmentLoadSessions_UserId')
    DROP INDEX IX_tblShipmentLoadSessions_UserId ON tblShipmentLoadSessions;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tblPreShipmentShipments_CreatedByUserId')
    DROP INDEX IX_tblPreShipmentShipments_CreatedByUserId ON tblPreShipmentShipments;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tblSkidBuildDrafts_UserId')
    DROP INDEX IX_tblSkidBuildDrafts_UserId ON tblSkidBuildDrafts;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tblShipmentLoadDrafts_UserId')
    DROP INDEX IX_tblShipmentLoadDrafts_UserId ON tblShipmentLoadDrafts;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tblSettings_UserId')
    DROP INDEX IX_tblSettings_UserId ON tblSettings;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_tblDockMonitorSettings_UserId')
    DROP INDEX IX_tblDockMonitorSettings_UserId ON tblDockMonitorSettings;

-- Step 5: Drop old columns
PRINT 'Step 5: Dropping old columns...';

ALTER TABLE tblUserMaster DROP CONSTRAINT PK_tblUserMaster;
ALTER TABLE tblUserMaster DROP COLUMN UserId;
ALTER TABLE tblUserSessions DROP COLUMN UserId;
ALTER TABLE tblOrderUploads DROP COLUMN UploadedBy;
ALTER TABLE tblSkidBuildSessions DROP COLUMN UserId;
ALTER TABLE tblShipmentLoadSessions DROP COLUMN UserId;
ALTER TABLE tblPreShipmentShipments DROP COLUMN CreatedByUserId;
ALTER TABLE tblSkidBuildDrafts DROP COLUMN UserId;
ALTER TABLE tblShipmentLoadDrafts DROP COLUMN UserId;
ALTER TABLE tblSettings DROP COLUMN UserId;
ALTER TABLE tblDockMonitorSettings DROP COLUMN UserId;

-- Step 6: Rename new columns to original names
PRINT 'Step 6: Renaming new columns...';

EXEC sp_rename 'tblUserMaster.UserId_New', 'UserId', 'COLUMN';
EXEC sp_rename 'tblUserSessions.UserId_New', 'UserId', 'COLUMN';
EXEC sp_rename 'tblOrderUploads.UploadedBy_New', 'UploadedBy', 'COLUMN';
EXEC sp_rename 'tblSkidBuildSessions.UserId_New', 'UserId', 'COLUMN';
EXEC sp_rename 'tblShipmentLoadSessions.UserId_New', 'UserId', 'COLUMN';
EXEC sp_rename 'tblPreShipmentShipments.CreatedByUserId_New', 'CreatedByUserId', 'COLUMN';
EXEC sp_rename 'tblSkidBuildDrafts.UserId_New', 'UserId', 'COLUMN';
EXEC sp_rename 'tblShipmentLoadDrafts.UserId_New', 'UserId', 'COLUMN';
EXEC sp_rename 'tblSettings.UserId_New', 'UserId', 'COLUMN';
EXEC sp_rename 'tblDockMonitorSettings.UserId_New', 'UserId', 'COLUMN';

-- Step 7: Make non-nullable columns NOT NULL
PRINT 'Step 7: Setting NOT NULL constraints...';

ALTER TABLE tblUserSessions ALTER COLUMN UserId UNIQUEIDENTIFIER NOT NULL;
ALTER TABLE tblSkidBuildSessions ALTER COLUMN UserId UNIQUEIDENTIFIER NOT NULL;
ALTER TABLE tblShipmentLoadSessions ALTER COLUMN UserId UNIQUEIDENTIFIER NOT NULL;
ALTER TABLE tblPreShipmentShipments ALTER COLUMN CreatedByUserId UNIQUEIDENTIFIER NOT NULL;
ALTER TABLE tblSkidBuildDrafts ALTER COLUMN UserId UNIQUEIDENTIFIER NOT NULL;
ALTER TABLE tblShipmentLoadDrafts ALTER COLUMN UserId UNIQUEIDENTIFIER NOT NULL;
ALTER TABLE tblDockMonitorSettings ALTER COLUMN UserId UNIQUEIDENTIFIER NOT NULL;

-- Step 8: Add primary key back to UserMaster
PRINT 'Step 8: Adding primary key...';

ALTER TABLE tblUserMaster ADD CONSTRAINT PK_tblUserMaster PRIMARY KEY (UserId);

-- Step 9: Recreate indexes
PRINT 'Step 9: Recreating indexes...';

CREATE INDEX IX_tblUserSessions_UserId ON tblUserSessions(UserId);
CREATE INDEX IX_tblOrderUploads_UploadedBy ON tblOrderUploads(UploadedBy);
CREATE INDEX IX_tblSkidBuildSessions_UserId ON tblSkidBuildSessions(UserId);
CREATE INDEX IX_tblShipmentLoadSessions_UserId ON tblShipmentLoadSessions(UserId);
CREATE INDEX IX_tblPreShipmentShipments_CreatedByUserId ON tblPreShipmentShipments(CreatedByUserId);
CREATE INDEX IX_tblSkidBuildDrafts_UserId ON tblSkidBuildDrafts(UserId);
CREATE INDEX IX_tblShipmentLoadDrafts_UserId ON tblShipmentLoadDrafts(UserId);
CREATE INDEX IX_tblSettings_UserId ON tblSettings(UserId);
CREATE INDEX IX_tblDockMonitorSettings_UserId ON tblDockMonitorSettings(UserId);

-- Step 10: Recreate foreign key constraints
PRINT 'Step 10: Recreating foreign key constraints...';

ALTER TABLE tblUserSessions ADD CONSTRAINT FK_tblUserSessions_tblUserMaster_UserId
    FOREIGN KEY (UserId) REFERENCES tblUserMaster(UserId) ON DELETE CASCADE;

ALTER TABLE tblOrderUploads ADD CONSTRAINT FK_tblOrderUploads_tblUserMaster_UploadedBy
    FOREIGN KEY (UploadedBy) REFERENCES tblUserMaster(UserId) ON DELETE SET NULL;

ALTER TABLE tblSkidBuildSessions ADD CONSTRAINT FK_tblSkidBuildSessions_tblUserMaster_UserId
    FOREIGN KEY (UserId) REFERENCES tblUserMaster(UserId) ON DELETE CASCADE;

ALTER TABLE tblShipmentLoadSessions ADD CONSTRAINT FK_tblShipmentLoadSessions_tblUserMaster_UserId
    FOREIGN KEY (UserId) REFERENCES tblUserMaster(UserId) ON DELETE CASCADE;

ALTER TABLE tblPreShipmentShipments ADD CONSTRAINT FK_tblPreShipmentShipments_tblUserMaster_CreatedByUserId
    FOREIGN KEY (CreatedByUserId) REFERENCES tblUserMaster(UserId) ON DELETE CASCADE;

ALTER TABLE tblSkidBuildDrafts ADD CONSTRAINT FK_tblSkidBuildDrafts_tblUserMaster_UserId
    FOREIGN KEY (UserId) REFERENCES tblUserMaster(UserId) ON DELETE CASCADE;

ALTER TABLE tblShipmentLoadDrafts ADD CONSTRAINT FK_tblShipmentLoadDrafts_tblUserMaster_UserId
    FOREIGN KEY (UserId) REFERENCES tblUserMaster(UserId) ON DELETE CASCADE;

ALTER TABLE tblSettings ADD CONSTRAINT FK_tblSettings_tblUserMaster_UserId
    FOREIGN KEY (UserId) REFERENCES tblUserMaster(UserId) ON DELETE SET NULL;

ALTER TABLE tblDockMonitorSettings ADD CONSTRAINT FK_tblDockMonitorSettings_tblUserMaster_UserId
    FOREIGN KEY (UserId) REFERENCES tblUserMaster(UserId) ON DELETE CASCADE;

-- Clean up
DROP TABLE #UserIdMapping;

COMMIT TRANSACTION;

PRINT 'UserId conversion completed successfully!';
PRINT 'All user IDs have been converted from NVARCHAR(50) to UNIQUEIDENTIFIER.';
