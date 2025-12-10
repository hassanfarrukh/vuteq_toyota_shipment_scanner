-- ==============================================================================
-- VUTEQ Warehouse Scanning System - Database Initialization Script
-- ==============================================================================
-- Author: Hassan
-- Date: 2025-10-20
-- Description: Initializes VUTEQ WMS database with Scott's 3 core tables:
--              1. KanbanSerialMaster - Master data for Kanban serial tracking
--              2. ItemConfirmationWIP - Work-in-progress item confirmations
--              3. ItemConfirmationHistory - Historical item confirmation records
-- ==============================================================================

USE master;
GO

-- ==============================================================================
-- DATABASE CREATION
-- ==============================================================================
-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'VuteqWMS_Prod')
BEGIN
    CREATE DATABASE VuteqWMS_Prod
    COLLATE SQL_Latin1_General_CP1_CI_AS;
    PRINT 'Database VuteqWMS_Prod created successfully.';
END
ELSE
BEGIN
    PRINT 'Database VuteqWMS_Prod already exists.';
END
GO

-- Switch to the new database
USE VuteqWMS_Prod;
GO

-- ==============================================================================
-- SCHEMA CREATION
-- ==============================================================================
-- Create schemas for logical organization
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Kanban')
BEGIN
    EXEC('CREATE SCHEMA Kanban');
    PRINT 'Schema Kanban created successfully.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Confirmation')
BEGIN
    EXEC('CREATE SCHEMA Confirmation');
    PRINT 'Schema Confirmation created successfully.';
END
GO

-- ==============================================================================
-- TABLE 1: KanbanSerialMaster
-- ==============================================================================
-- Master table for tracking Kanban serial numbers and related information
-- Used for Toyota production system integration
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'KanbanSerialMaster' AND schema_id = SCHEMA_ID('Kanban'))
BEGIN
    CREATE TABLE Kanban.KanbanSerialMaster
    (
        -- Primary Key
        KanbanSerialID          BIGINT IDENTITY(1,1) NOT NULL,

        -- Kanban Information
        KanbanNumber            NVARCHAR(50) NOT NULL,
        SerialNumber            NVARCHAR(100) NOT NULL,

        -- Part Information
        PartNumber              NVARCHAR(50) NOT NULL,
        PartDescription         NVARCHAR(255) NULL,
        SupplierPartNumber      NVARCHAR(50) NULL,

        -- Quantity Information
        Quantity                DECIMAL(18, 4) NOT NULL DEFAULT 0,
        UnitOfMeasure           NVARCHAR(10) NULL,

        -- Location Information
        SiteCode                NVARCHAR(10) NOT NULL,
        LocationCode            NVARCHAR(50) NULL,
        WarehouseZone           NVARCHAR(50) NULL,

        -- Status Information
        Status                  NVARCHAR(20) NOT NULL DEFAULT 'Active',
        StatusReason            NVARCHAR(255) NULL,

        -- Supplier Information
        SupplierCode            NVARCHAR(50) NULL,
        SupplierName            NVARCHAR(255) NULL,

        -- Date/Time Information
        ManufactureDate         DATETIME NULL,
        ExpiryDate              DATETIME NULL,
        ReceivedDate            DATETIME NOT NULL DEFAULT GETUTCDATE(),
        ShippedDate             DATETIME NULL,

        -- Toyota API Integration
        ToyotaOrderNumber       NVARCHAR(50) NULL,
        ToyotaContractNumber    NVARCHAR(50) NULL,
        ToyotaSequenceNumber    NVARCHAR(50) NULL,

        -- Audit Fields
        CreatedBy               NVARCHAR(100) NOT NULL DEFAULT SYSTEM_USER,
        CreatedDate             DATETIME NOT NULL DEFAULT GETUTCDATE(),
        ModifiedBy              NVARCHAR(100) NULL,
        ModifiedDate            DATETIME NULL,

        -- Soft Delete
        IsDeleted               BIT NOT NULL DEFAULT 0,
        DeletedBy               NVARCHAR(100) NULL,
        DeletedDate             DATETIME NULL,

        -- Row Version for Concurrency
        RowVersion              ROWVERSION NOT NULL,

        -- Constraints
        CONSTRAINT PK_KanbanSerialMaster PRIMARY KEY CLUSTERED (KanbanSerialID),
        CONSTRAINT UK_KanbanSerialMaster_KanbanSerial UNIQUE NONCLUSTERED (KanbanNumber, SerialNumber, SiteCode),
        CONSTRAINT CK_KanbanSerialMaster_Status CHECK (Status IN ('Active', 'Consumed', 'Expired', 'Damaged', 'Returned')),
        CONSTRAINT CK_KanbanSerialMaster_Quantity CHECK (Quantity >= 0)
    );

    -- Create indexes for performance
    CREATE NONCLUSTERED INDEX IX_KanbanSerialMaster_PartNumber
        ON Kanban.KanbanSerialMaster(PartNumber, SiteCode)
        INCLUDE (Status, Quantity);

    CREATE NONCLUSTERED INDEX IX_KanbanSerialMaster_SiteCode
        ON Kanban.KanbanSerialMaster(SiteCode, Status)
        INCLUDE (KanbanNumber, PartNumber);

    CREATE NONCLUSTERED INDEX IX_KanbanSerialMaster_ReceivedDate
        ON Kanban.KanbanSerialMaster(ReceivedDate DESC)
        WHERE IsDeleted = 0;

    CREATE NONCLUSTERED INDEX IX_KanbanSerialMaster_ToyotaOrder
        ON Kanban.KanbanSerialMaster(ToyotaOrderNumber)
        WHERE ToyotaOrderNumber IS NOT NULL;

    PRINT 'Table Kanban.KanbanSerialMaster created successfully.';
END
GO

-- ==============================================================================
-- TABLE 2: ItemConfirmationWIP (Work-in-Progress)
-- ==============================================================================
-- Tracks items currently being scanned/confirmed (work-in-progress)
-- Data moves to history table upon completion
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ItemConfirmationWIP' AND schema_id = SCHEMA_ID('Confirmation'))
BEGIN
    CREATE TABLE Confirmation.ItemConfirmationWIP
    (
        -- Primary Key
        ConfirmationWIPID       BIGINT IDENTITY(1,1) NOT NULL,

        -- Reference to Kanban
        KanbanSerialID          BIGINT NOT NULL,

        -- Confirmation Session Information
        SessionID               UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        BatchNumber             NVARCHAR(50) NULL,

        -- Item Information
        ItemNumber              NVARCHAR(50) NOT NULL,
        ItemDescription         NVARCHAR(255) NULL,
        ScannedQuantity         DECIMAL(18, 4) NOT NULL DEFAULT 0,
        ExpectedQuantity        DECIMAL(18, 4) NOT NULL DEFAULT 0,
        VarianceQuantity        AS (ScannedQuantity - ExpectedQuantity) PERSISTED,

        -- Site Information
        SiteCode                NVARCHAR(10) NOT NULL,
        ScanLocation            NVARCHAR(100) NULL,

        -- Status Information
        ConfirmationStatus      NVARCHAR(20) NOT NULL DEFAULT 'InProgress',

        -- Scanner Information
        ScannerDeviceID         NVARCHAR(100) NULL,
        ScannerType             NVARCHAR(50) NULL,

        -- User Information
        ScannedByUserID         NVARCHAR(100) NOT NULL,
        ScannedByUserName       NVARCHAR(255) NOT NULL,

        -- Timestamp Information
        ScanStartTime           DATETIME NOT NULL DEFAULT GETUTCDATE(),
        LastScanTime            DATETIME NOT NULL DEFAULT GETUTCDATE(),

        -- Quality Check
        QualityCheckRequired    BIT NOT NULL DEFAULT 0,
        QualityCheckPassed      BIT NULL,
        QualityCheckNotes       NVARCHAR(500) NULL,

        -- Exception Handling
        HasException            BIT NOT NULL DEFAULT 0,
        ExceptionType           NVARCHAR(50) NULL,
        ExceptionNotes          NVARCHAR(500) NULL,

        -- Audit Fields
        CreatedDate             DATETIME NOT NULL DEFAULT GETUTCDATE(),
        ModifiedDate            DATETIME NOT NULL DEFAULT GETUTCDATE(),

        -- Row Version for Concurrency
        RowVersion              ROWVERSION NOT NULL,

        -- Constraints
        CONSTRAINT PK_ItemConfirmationWIP PRIMARY KEY CLUSTERED (ConfirmationWIPID),
        CONSTRAINT FK_ItemConfirmationWIP_KanbanSerialMaster
            FOREIGN KEY (KanbanSerialID) REFERENCES Kanban.KanbanSerialMaster(KanbanSerialID),
        CONSTRAINT CK_ItemConfirmationWIP_Status
            CHECK (ConfirmationStatus IN ('InProgress', 'Paused', 'PendingReview', 'ReadyToComplete')),
        CONSTRAINT CK_ItemConfirmationWIP_Quantity
            CHECK (ScannedQuantity >= 0 AND ExpectedQuantity >= 0)
    );

    -- Create indexes for performance
    CREATE NONCLUSTERED INDEX IX_ItemConfirmationWIP_SessionID
        ON Confirmation.ItemConfirmationWIP(SessionID)
        INCLUDE (ConfirmationStatus, ScannedQuantity);

    CREATE NONCLUSTERED INDEX IX_ItemConfirmationWIP_KanbanSerialID
        ON Confirmation.ItemConfirmationWIP(KanbanSerialID, ConfirmationStatus);

    CREATE NONCLUSTERED INDEX IX_ItemConfirmationWIP_SiteCode
        ON Confirmation.ItemConfirmationWIP(SiteCode, ConfirmationStatus)
        INCLUDE (SessionID, ItemNumber);

    CREATE NONCLUSTERED INDEX IX_ItemConfirmationWIP_User
        ON Confirmation.ItemConfirmationWIP(ScannedByUserID, ScanStartTime DESC);

    PRINT 'Table Confirmation.ItemConfirmationWIP created successfully.';
END
GO

-- ==============================================================================
-- TABLE 3: ItemConfirmationHistory
-- ==============================================================================
-- Historical record of completed item confirmations
-- Archive table for reporting and audit trail
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ItemConfirmationHistory' AND schema_id = SCHEMA_ID('Confirmation'))
BEGIN
    CREATE TABLE Confirmation.ItemConfirmationHistory
    (
        -- Primary Key
        ConfirmationHistoryID   BIGINT IDENTITY(1,1) NOT NULL,

        -- Reference to WIP (for tracking)
        ConfirmationWIPID       BIGINT NULL,

        -- Reference to Kanban
        KanbanSerialID          BIGINT NOT NULL,

        -- Confirmation Session Information
        SessionID               UNIQUEIDENTIFIER NOT NULL,
        BatchNumber             NVARCHAR(50) NULL,

        -- Item Information
        ItemNumber              NVARCHAR(50) NOT NULL,
        ItemDescription         NVARCHAR(255) NULL,
        ScannedQuantity         DECIMAL(18, 4) NOT NULL DEFAULT 0,
        ExpectedQuantity        DECIMAL(18, 4) NOT NULL DEFAULT 0,
        VarianceQuantity        DECIMAL(18, 4) NOT NULL DEFAULT 0,

        -- Site Information
        SiteCode                NVARCHAR(10) NOT NULL,
        ScanLocation            NVARCHAR(100) NULL,

        -- Status Information
        FinalStatus             NVARCHAR(20) NOT NULL,
        CompletionType          NVARCHAR(20) NOT NULL DEFAULT 'Normal',

        -- Scanner Information
        ScannerDeviceID         NVARCHAR(100) NULL,
        ScannerType             NVARCHAR(50) NULL,

        -- User Information
        ScannedByUserID         NVARCHAR(100) NOT NULL,
        ScannedByUserName       NVARCHAR(255) NOT NULL,
        CompletedByUserID       NVARCHAR(100) NULL,
        CompletedByUserName     NVARCHAR(255) NULL,

        -- Timestamp Information
        ScanStartTime           DATETIME NOT NULL,
        ScanCompletedTime       DATETIME NOT NULL DEFAULT GETUTCDATE(),
        TotalDurationMinutes    AS DATEDIFF(MINUTE, ScanStartTime, ScanCompletedTime) PERSISTED,

        -- Quality Check
        QualityCheckRequired    BIT NOT NULL DEFAULT 0,
        QualityCheckPassed      BIT NULL,
        QualityCheckNotes       NVARCHAR(500) NULL,
        QualityCheckByUserID    NVARCHAR(100) NULL,
        QualityCheckDate        DATETIME NULL,

        -- Exception Handling
        HasException            BIT NOT NULL DEFAULT 0,
        ExceptionType           NVARCHAR(50) NULL,
        ExceptionNotes          NVARCHAR(500) NULL,
        ExceptionResolvedBy     NVARCHAR(100) NULL,
        ExceptionResolvedDate   DATETIME NULL,

        -- Toyota API Integration
        ToyotaConfirmationID    NVARCHAR(100) NULL,
        ToyotaSyncStatus        NVARCHAR(20) NULL,
        ToyotaSyncDate          DATETIME NULL,

        -- Audit Fields
        CreatedDate             DATETIME NOT NULL DEFAULT GETUTCDATE(),
        ArchivedDate            DATETIME NOT NULL DEFAULT GETUTCDATE(),

        -- Constraints
        CONSTRAINT PK_ItemConfirmationHistory PRIMARY KEY CLUSTERED (ConfirmationHistoryID),
        CONSTRAINT FK_ItemConfirmationHistory_KanbanSerialMaster
            FOREIGN KEY (KanbanSerialID) REFERENCES Kanban.KanbanSerialMaster(KanbanSerialID),
        CONSTRAINT CK_ItemConfirmationHistory_FinalStatus
            CHECK (FinalStatus IN ('Completed', 'Cancelled', 'Exception')),
        CONSTRAINT CK_ItemConfirmationHistory_CompletionType
            CHECK (CompletionType IN ('Normal', 'Emergency', 'Override', 'Cancelled'))
    );

    -- Create indexes for performance and reporting
    CREATE NONCLUSTERED INDEX IX_ItemConfirmationHistory_SessionID
        ON Confirmation.ItemConfirmationHistory(SessionID)
        INCLUDE (FinalStatus, ScannedQuantity);

    CREATE NONCLUSTERED INDEX IX_ItemConfirmationHistory_KanbanSerialID
        ON Confirmation.ItemConfirmationHistory(KanbanSerialID);

    CREATE NONCLUSTERED INDEX IX_ItemConfirmationHistory_SiteCode
        ON Confirmation.ItemConfirmationHistory(SiteCode, ScanCompletedTime DESC)
        INCLUDE (ItemNumber, FinalStatus);

    CREATE NONCLUSTERED INDEX IX_ItemConfirmationHistory_CompletedDate
        ON Confirmation.ItemConfirmationHistory(ScanCompletedTime DESC)
        INCLUDE (SiteCode, FinalStatus, ScannedQuantity);

    CREATE NONCLUSTERED INDEX IX_ItemConfirmationHistory_User
        ON Confirmation.ItemConfirmationHistory(ScannedByUserID, ScanCompletedTime DESC);

    CREATE NONCLUSTERED INDEX IX_ItemConfirmationHistory_ToyotaSync
        ON Confirmation.ItemConfirmationHistory(ToyotaSyncStatus)
        WHERE ToyotaSyncStatus IS NOT NULL;

    PRINT 'Table Confirmation.ItemConfirmationHistory created successfully.';
END
GO

-- ==============================================================================
-- CREATE MULTI-SITE DATABASES
-- ==============================================================================
-- Create database for each of the 6 manufacturing sites
DECLARE @SiteCode NVARCHAR(10);
DECLARE @DatabaseName NVARCHAR(100);
DECLARE @SQL NVARCHAR(MAX);

DECLARE site_cursor CURSOR FOR
SELECT SiteCode, DatabaseName FROM (
    VALUES
        ('TMH', 'VuteqWMS_TMH'),
        ('TMI', 'VuteqWMS_TMI'),
        ('TMK', 'VuteqWMS_TMK'),
        ('TMT', 'VuteqWMS_TMT'),
        ('TMBC', 'VuteqWMS_TMBC'),
        ('TMMWV', 'VuteqWMS_TMMWV')
) AS Sites(SiteCode, DatabaseName);

OPEN site_cursor;
FETCH NEXT FROM site_cursor INTO @SiteCode, @DatabaseName;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Check if database exists
    IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = @DatabaseName)
    BEGIN
        SET @SQL = 'CREATE DATABASE ' + QUOTENAME(@DatabaseName) + ' COLLATE SQL_Latin1_General_CP1_CI_AS';
        EXEC sp_executesql @SQL;
        PRINT 'Database ' + @DatabaseName + ' created for site ' + @SiteCode;
    END

    FETCH NEXT FROM site_cursor INTO @SiteCode, @DatabaseName;
END

CLOSE site_cursor;
DEALLOCATE site_cursor;
GO

-- ==============================================================================
-- COMPLETION MESSAGE
-- ==============================================================================
PRINT '';
PRINT '==============================================================================';
PRINT 'VUTEQ WMS Database Initialization Complete';
PRINT '==============================================================================';
PRINT 'Created databases:';
PRINT '  - VuteqWMS_Prod (Production)';
PRINT '  - VuteqWMS_TMH (Toyota Mississippi)';
PRINT '  - VuteqWMS_TMI (Toyota Indiana)';
PRINT '  - VuteqWMS_TMK (Toyota Kentucky)';
PRINT '  - VuteqWMS_TMT (Toyota Texas)';
PRINT '  - VuteqWMS_TMBC (Toyota Canada)';
PRINT '  - VuteqWMS_TMMWV (Toyota West Virginia)';
PRINT '';
PRINT 'Created tables:';
PRINT '  - Kanban.KanbanSerialMaster';
PRINT '  - Confirmation.ItemConfirmationWIP';
PRINT '  - Confirmation.ItemConfirmationHistory';
PRINT '';
PRINT 'Author: Hassan';
PRINT 'Date: 2025-10-20';
PRINT '==============================================================================';
GO
