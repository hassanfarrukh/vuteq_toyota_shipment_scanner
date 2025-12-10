-- =============================================
-- VUTEQ Scanner Application Database - CORRECTED NAMING CONVENTIONS
-- Author: Hassan
-- Date: 2025-11-11
-- Purpose: Match naming conventions from original vuteq_wms.sql database
-- CRITICAL: This file matches the original tblTableName naming pattern
-- =============================================

USE master;
GO

-- Drop existing database if exists
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'VUTEQ_Scanner')
BEGIN
    ALTER DATABASE VUTEQ_Scanner SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE VUTEQ_Scanner;
END
GO

-- Create database
CREATE DATABASE VUTEQ_Scanner;
GO

USE VUTEQ_Scanner;
GO

-- =============================================
-- SECTION 1: AUTHENTICATION & USERS
-- =============================================

-- Table: tblUserMaster (matches original naming convention)
CREATE TABLE [dbo].[tblUserMaster] (
    [UserId] NVARCHAR(50) PRIMARY KEY,
    [Username] NVARCHAR(100) NOT NULL UNIQUE,
    [PasswordHash] NVARCHAR(255) NOT NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [Email] NVARCHAR(200),
    [Role] NVARCHAR(20) NOT NULL CHECK ([Role] IN ('ADMIN', 'SUPERVISOR', 'OPERATOR')),
    [MenuLevel] NVARCHAR(20) CHECK ([MenuLevel] IN ('Admin', 'Scanner', 'Operation')),
    [Operation] NVARCHAR(50) CHECK ([Operation] IN ('Warehouse', 'Office', 'Administration')),
    [LocationId] NVARCHAR(50),
    [Code] NVARCHAR(50),
    [IsSupervisor] BIT DEFAULT 0,
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME2 DEFAULT GETDATE(),
    [ModifiedAt] DATETIME2 DEFAULT GETDATE(),
    [LastLoginAt] DATETIME2
);
GO

-- Table: tblUserSessions (JWT Token Management)
CREATE TABLE [dbo].[tblUserSessions] (
    [SessionId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [UserId] NVARCHAR(50) NOT NULL,
    [Token] NVARCHAR(MAX) NOT NULL,
    [RefreshToken] NVARCHAR(MAX),
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME2 DEFAULT GETDATE(),
    [ExpiresAt] DATETIME2 NOT NULL,
    [LastActivityAt] DATETIME2 DEFAULT GETDATE(),
    [IpAddress] NVARCHAR(50),
    [UserAgent] NVARCHAR(500),
    CONSTRAINT [FK_tblUserSessions_tblUserMaster] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tblUserMaster]([UserId])
);
GO

-- =============================================
-- SECTION 2: ORDERS & UPLOADS
-- =============================================

-- Table: tblOrderUploads
CREATE TABLE [dbo].[tblOrderUploads] (
    [Id] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [FileName] NVARCHAR(500) NOT NULL,
    [FileSize] BIGINT NOT NULL,
    [FilePath] NVARCHAR(1000),
    [Status] NVARCHAR(20) CHECK ([Status] IN ('success', 'pending', 'error')) DEFAULT 'pending',
    [UploadedBy] NVARCHAR(50),
    [UploadDate] DATETIME2 DEFAULT GETDATE(),
    [ErrorMessage] NVARCHAR(MAX),
    CONSTRAINT [FK_tblOrderUploads_tblUserMaster] FOREIGN KEY ([UploadedBy]) REFERENCES [dbo].[tblUserMaster]([UserId])
);
GO

-- Table: tblOrders
CREATE TABLE [dbo].[tblOrders] (
    [OrderId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [OwkNumber] NVARCHAR(50) NOT NULL UNIQUE,
    [CustomerName] NVARCHAR(200),
    [Destination] NVARCHAR(200),
    [TotalSkids] INT,
    [PlantCode] NVARCHAR(20),
    [SupplierCode] NVARCHAR(20),
    [DockCode] NVARCHAR(20),
    [LoadId] NVARCHAR(50),
    [OrderDate] DATETIME2,
    [Status] NVARCHAR(50),
    [CreatedAt] DATETIME2 DEFAULT GETDATE(),
    [ModifiedAt] DATETIME2 DEFAULT GETDATE()
);
GO

-- Table: tblPlannedItems (Expected parts for orders)
CREATE TABLE [dbo].[tblPlannedItems] (
    [PlannedItemId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [OwkNumber] NVARCHAR(50) NOT NULL,
    [PartNumber] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500),
    [PlannedQty] INT NOT NULL,
    [RawKanbanValue] NVARCHAR(MAX),
    [SupplierCode] NVARCHAR(20),
    [CreatedAt] DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT [FK_tblPlannedItems_tblOrders] FOREIGN KEY ([OwkNumber]) REFERENCES [dbo].[tblOrders]([OwkNumber])
);
GO

-- =============================================
-- SECTION 3: SKID BUILD WORKFLOW
-- =============================================

-- Table: tblSkidBuildSessions
CREATE TABLE [dbo].[tblSkidBuildSessions] (
    [SessionId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [UserId] NVARCHAR(50) NOT NULL,
    [WarehouseId] NVARCHAR(50),
    [OwkNumber] NVARCHAR(50),
    [SupplierCode] NVARCHAR(20),
    [Token] NVARCHAR(MAX),
    [Status] NVARCHAR(20) CHECK ([Status] IN ('active', 'completed', 'cancelled', 'draft')) DEFAULT 'active',
    [CurrentScreen] INT DEFAULT 1,
    [CreatedAt] DATETIME2 DEFAULT GETDATE(),
    [ExpiresAt] DATETIME2,
    [CompletedAt] DATETIME2,
    [ConfirmationNumber] NVARCHAR(100),
    CONSTRAINT [FK_tblSkidBuildSessions_tblUserMaster] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tblUserMaster]([UserId]),
    CONSTRAINT [FK_tblSkidBuildSessions_tblOrders] FOREIGN KEY ([OwkNumber]) REFERENCES [dbo].[tblOrders]([OwkNumber])
);
GO

-- Table: tblToyotaManifests (44-character QR codes)
CREATE TABLE [dbo].[tblToyotaManifests] (
    [ManifestId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [QrCode] NVARCHAR(100) NOT NULL,
    [PlantPrefix] NVARCHAR(10),
    [PlantCode] NVARCHAR(10),
    [SupplierCode] NVARCHAR(20),
    [DockCode] NVARCHAR(10),
    [OrderNumber] NVARCHAR(50),
    [LoadId] NVARCHAR(50),
    [PalletizationCode] NVARCHAR(10),
    [Mros] NVARCHAR(10),
    [SkidId] NVARCHAR(20),
    [FormattedSkidId] NVARCHAR(20), -- Format: LB-05-001A
    [ScannedAt] DATETIME2 DEFAULT GETDATE(),
    [ScannedBy] NVARCHAR(50),
    [SessionId] UNIQUEIDENTIFIER,
    CONSTRAINT [FK_tblToyotaManifests_tblSkidBuildSessions] FOREIGN KEY ([SessionId]) REFERENCES [dbo].[tblSkidBuildSessions]([SessionId])
);
GO

-- Table: tblToyotaKanbans (200+ character QR codes with 24 fields)
CREATE TABLE [dbo].[tblToyotaKanbans] (
    [KanbanId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [QrCode] NVARCHAR(MAX) NOT NULL,
    [PartNumber] NVARCHAR(100),
    [Description] NVARCHAR(500),
    [SupplierCode] NVARCHAR(20),
    [DockCode] NVARCHAR(10),
    [Quantity] NVARCHAR(20),
    [KanbanNumber] NVARCHAR(100),
    [ShipToAddress1] NVARCHAR(200),
    [ShipToAddress2] NVARCHAR(200),
    [DeliveryDate] NVARCHAR(50),
    [DeliveryTime] NVARCHAR(50),
    [PlantCode] NVARCHAR(20),
    [Route] NVARCHAR(50),
    [ContainerType] NVARCHAR(50),
    [PalletCode] NVARCHAR(20),
    [StorageLocation] NVARCHAR(50),
    [OrderNumber] NVARCHAR(50),
    [SequenceNumber] NVARCHAR(20),
    [BatchNumber] NVARCHAR(50),
    [ManufacturingDate] NVARCHAR(50),
    [ExpiryDate] NVARCHAR(50),
    [LotNumber] NVARCHAR(50),
    [SerialNumber] NVARCHAR(50),
    [RevisionLevel] NVARCHAR(20),
    [QualityStatus] NVARCHAR(50),
    [SessionId] UNIQUEIDENTIFIER,
    [ScannedAt] DATETIME2 DEFAULT GETDATE(),
    [ScannedBy] NVARCHAR(50),
    CONSTRAINT [FK_tblToyotaKanbans_tblSkidBuildSessions] FOREIGN KEY ([SessionId]) REFERENCES [dbo].[tblSkidBuildSessions]([SessionId])
);
GO

-- Table: tblInternalKanbans (PART/KANBAN/SERIAL format)
CREATE TABLE [dbo].[tblInternalKanbans] (
    [InternalKanbanId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [ScanValue] NVARCHAR(500) NOT NULL,
    [ToyotaKanban] NVARCHAR(100),
    [InternalKanban] NVARCHAR(100),
    [SerialNumber] NVARCHAR(100),
    [SessionId] UNIQUEIDENTIFIER,
    [ToyotaKanbanId] UNIQUEIDENTIFIER,
    [ScannedAt] DATETIME2 DEFAULT GETDATE(),
    [ScannedBy] NVARCHAR(50),
    CONSTRAINT [FK_tblInternalKanbans_tblSkidBuildSessions] FOREIGN KEY ([SessionId]) REFERENCES [dbo].[tblSkidBuildSessions]([SessionId]),
    CONSTRAINT [FK_tblInternalKanbans_tblToyotaKanbans] FOREIGN KEY ([ToyotaKanbanId]) REFERENCES [dbo].[tblToyotaKanbans]([KanbanId])
);
GO

-- Table: tblScannedItems (Items scanned during skid build)
CREATE TABLE [dbo].[tblScannedItems] (
    [ScannedItemId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [SessionId] UNIQUEIDENTIFIER NOT NULL,
    [PartNumber] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500),
    [Quantity] INT,
    [KanbanNumber] NVARCHAR(100),
    [InternalKanban] NVARCHAR(100),
    [SerialNumber] NVARCHAR(100),
    [ScannedAt] DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT [FK_tblScannedItems_tblSkidBuildSessions] FOREIGN KEY ([SessionId]) REFERENCES [dbo].[tblSkidBuildSessions]([SessionId])
);
GO

-- Table: tblSkidBuildExceptions
CREATE TABLE [dbo].[tblSkidBuildExceptions] (
    [ExceptionId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [SessionId] UNIQUEIDENTIFIER NOT NULL,
    [OwkNumber] NVARCHAR(50),
    [ExceptionType] NVARCHAR(200) CHECK ([ExceptionType] IN (
        'Revised Quantity (Toyota Quantity Reduction)',
        'Modified Quantity per Box',
        'Supplier Revised Shortage (Short Shipment)',
        'Non-Standard Packaging (Expendable)'
    )),
    [Comments] NVARCHAR(500),
    [CreatedAt] DATETIME2 DEFAULT GETDATE(),
    [CreatedBy] NVARCHAR(50),
    CONSTRAINT [FK_tblSkidBuildExceptions_tblSkidBuildSessions] FOREIGN KEY ([SessionId]) REFERENCES [dbo].[tblSkidBuildSessions]([SessionId])
);
GO

-- Table: tblSkidBuildDrafts
CREATE TABLE [dbo].[tblSkidBuildDrafts] (
    [DraftId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [SessionId] UNIQUEIDENTIFIER NOT NULL,
    [UserId] NVARCHAR(50) NOT NULL,
    [OwkNumber] NVARCHAR(50),
    [DraftData] NVARCHAR(MAX), -- JSON format
    [CurrentScreen] INT,
    [SavedAt] DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT [FK_tblSkidBuildDrafts_tblSkidBuildSessions] FOREIGN KEY ([SessionId]) REFERENCES [dbo].[tblSkidBuildSessions]([SessionId]),
    CONSTRAINT [FK_tblSkidBuildDrafts_tblUserMaster] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tblUserMaster]([UserId])
);
GO

-- =============================================
-- SECTION 4: SHIPMENT LOAD WORKFLOW
-- =============================================

-- Table: tblPickupRoutes (50-character QR codes)
CREATE TABLE [dbo].[tblPickupRoutes] (
    [RouteId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [QrCode] NVARCHAR(100) NOT NULL,
    [RouteNumber] NVARCHAR(50),
    [Plant] NVARCHAR(20),
    [SupplierCode] NVARCHAR(20),
    [DockCode] NVARCHAR(10),
    [EstimatedSkids] INT,
    [OrderDate] NVARCHAR(50),
    [PickupDate] NVARCHAR(50),
    [PickupTime] NVARCHAR(50),
    [CreatedAt] DATETIME2 DEFAULT GETDATE()
);
GO

-- Table: tblShipmentLoadSessions
CREATE TABLE [dbo].[tblShipmentLoadSessions] (
    [SessionId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [RouteNumber] NVARCHAR(50) NOT NULL,
    [UserId] NVARCHAR(50) NOT NULL,
    [WarehouseId] NVARCHAR(50),
    [Token] NVARCHAR(MAX),
    [TrailerNumber] NVARCHAR(50),
    [SealNumber] NVARCHAR(50),
    [CarrierName] NVARCHAR(200),
    [DriverName] NVARCHAR(200),
    [Notes] NVARCHAR(MAX),
    [Status] NVARCHAR(20) CHECK ([Status] IN ('active', 'completed', 'cancelled', 'draft')) DEFAULT 'active',
    [CurrentScreen] INT DEFAULT 1,
    [CreatedAt] DATETIME2 DEFAULT GETDATE(),
    [CompletedAt] DATETIME2,
    [ConfirmationNumber] NVARCHAR(100),
    CONSTRAINT [FK_tblShipmentLoadSessions_tblUserMaster] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tblUserMaster]([UserId])
);
GO

-- Table: tblPlannedSkids
CREATE TABLE [dbo].[tblPlannedSkids] (
    [PlannedSkidId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [RouteNumber] NVARCHAR(50) NOT NULL,
    [SkidId] NVARCHAR(50) NOT NULL,
    [OrderNumber] NVARCHAR(50),
    [PartCount] INT,
    [Destination] NVARCHAR(200),
    [Plant] NVARCHAR(20),
    [SupplierCode] NVARCHAR(20),
    [CreatedAt] DATETIME2 DEFAULT GETDATE()
);
GO

-- Table: tblScannedSkids
CREATE TABLE [dbo].[tblScannedSkids] (
    [ScannedSkidId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [SessionId] UNIQUEIDENTIFIER NOT NULL,
    [SkidId] NVARCHAR(50) NOT NULL,
    [OrderNumber] NVARCHAR(50),
    [PartCount] INT,
    [Destination] NVARCHAR(200),
    [ScannedAt] DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT [FK_tblScannedSkids_tblShipmentLoadSessions] FOREIGN KEY ([SessionId]) REFERENCES [dbo].[tblShipmentLoadSessions]([SessionId])
);
GO

-- Table: tblShipmentLoadExceptions
CREATE TABLE [dbo].[tblShipmentLoadExceptions] (
    [ExceptionId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [SessionId] UNIQUEIDENTIFIER NOT NULL,
    [ExceptionType] NVARCHAR(200) CHECK ([ExceptionType] IN (
        'Revised Quantity (Toyota Quantity Reduction)',
        'Modified Quantity per Box',
        'Supplier Revised Shortage (Short Shipment)',
        'Non-Standard Packaging (Expendable)'
    )),
    [Comments] NVARCHAR(500),
    [RelatedSkidId] NVARCHAR(50),
    [CreatedAt] DATETIME2 DEFAULT GETDATE(),
    [CreatedBy] NVARCHAR(50),
    CONSTRAINT [FK_tblShipmentLoadExceptions_tblShipmentLoadSessions] FOREIGN KEY ([SessionId]) REFERENCES [dbo].[tblShipmentLoadSessions]([SessionId])
);
GO

-- Table: tblShipmentLoadDrafts
CREATE TABLE [dbo].[tblShipmentLoadDrafts] (
    [DraftId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [SessionId] UNIQUEIDENTIFIER NOT NULL,
    [UserId] NVARCHAR(50) NOT NULL,
    [RouteNumber] NVARCHAR(50),
    [DraftData] NVARCHAR(MAX), -- JSON format
    [CurrentScreen] INT,
    [SavedAt] DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT [FK_tblShipmentLoadDrafts_tblShipmentLoadSessions] FOREIGN KEY ([SessionId]) REFERENCES [dbo].[tblShipmentLoadSessions]([SessionId]),
    CONSTRAINT [FK_tblShipmentLoadDrafts_tblUserMaster] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tblUserMaster]([UserId])
);
GO

-- =============================================
-- SECTION 5: PRE-SHIPMENT SCAN WORKFLOW
-- =============================================

-- Table: tblPreShipmentShipments
CREATE TABLE [dbo].[tblPreShipmentShipments] (
    [ShipmentId] NVARCHAR(50) PRIMARY KEY, -- Format: SHP{timestamp}
    [CreatedBy] NVARCHAR(50) NOT NULL,
    [Status] NVARCHAR(20) CHECK ([Status] IN ('in-progress', 'completed', 'cancelled')) DEFAULT 'in-progress',
    [CurrentScreen] INT DEFAULT 1,
    [TrailerNumber] NVARCHAR(50),
    [SealNumber] NVARCHAR(50),
    [CarrierName] NVARCHAR(200),
    [DriverName] NVARCHAR(200),
    [Notes] NVARCHAR(MAX),
    [CreatedAt] DATETIME2 DEFAULT GETDATE(),
    [CompletedAt] DATETIME2,
    [ConfirmationNumber] NVARCHAR(100),
    CONSTRAINT [FK_tblPreShipmentShipments_tblUserMaster] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[tblUserMaster]([UserId])
);
GO

-- Table: tblPreShipmentManifests
CREATE TABLE [dbo].[tblPreShipmentManifests] (
    [ManifestRecordId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [ShipmentId] NVARCHAR(50) NOT NULL,
    [ManifestId] NVARCHAR(20), -- Last 8 chars
    [ScannedValue] NVARCHAR(500),
    [ScannedAt] DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT [FK_tblPreShipmentManifests_tblPreShipmentShipments] FOREIGN KEY ([ShipmentId]) REFERENCES [dbo].[tblPreShipmentShipments]([ShipmentId])
);
GO

-- Table: tblPreShipmentScannedSkids
CREATE TABLE [dbo].[tblPreShipmentScannedSkids] (
    [ScannedSkidId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [ShipmentId] NVARCHAR(50) NOT NULL,
    [SkidId] NVARCHAR(50) NOT NULL,
    [OrderNumber] NVARCHAR(50),
    [PartCount] INT,
    [Destination] NVARCHAR(200),
    [ScannedValue] NVARCHAR(500),
    [ScannedAt] DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT [FK_tblPreShipmentScannedSkids_tblPreShipmentShipments] FOREIGN KEY ([ShipmentId]) REFERENCES [dbo].[tblPreShipmentShipments]([ShipmentId])
);
GO

-- Table: tblPreShipmentExceptions
CREATE TABLE [dbo].[tblPreShipmentExceptions] (
    [ExceptionId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [ShipmentId] NVARCHAR(50) NOT NULL,
    [ExceptionType] NVARCHAR(200) CHECK ([ExceptionType] IN (
        'Revised Quantity (Toyota Quantity Reduction)',
        'Modified Quantity per Box',
        'Supplier Revised Shortage (Short Shipment)',
        'Non-Standard Packaging (Expendable)'
    )),
    [Comments] NVARCHAR(500),
    [RelatedSkidId] NVARCHAR(50),
    [CreatedAt] DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT [FK_tblPreShipmentExceptions_tblPreShipmentShipments] FOREIGN KEY ([ShipmentId]) REFERENCES [dbo].[tblPreShipmentShipments]([ShipmentId])
);
GO

-- =============================================
-- SECTION 6: DOCK MONITOR
-- =============================================

-- Table: tblDockOrders
CREATE TABLE [dbo].[tblDockOrders] (
    [DockOrderId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [OrderNumber] NVARCHAR(50) NOT NULL,
    [Route] NVARCHAR(50),
    [Destination] NVARCHAR(200),
    [Supplier] NVARCHAR(200),
    [Location] NVARCHAR(50) CHECK ([Location] IN ('INDIANA', 'MICHIGAN', 'OHIO', 'KENTUCKY', 'TENNESSEE', 'ALABAMA')),
    [PlannedSkidBuild] DATETIME2,
    [CompletedSkidBuild] DATETIME2,
    [PlannedShipmentLoad] DATETIME2,
    [CompletedShipmentLoad] DATETIME2,
    [Status] NVARCHAR(50) CHECK ([Status] IN ('COMPLETED', 'ON_TIME', 'BEHIND', 'CRITICAL', 'PROJECT_SHORT', 'SHORT_SHIPPED')),
    [IsSupplementOrder] BIT DEFAULT 0,
    [CreatedAt] DATETIME2 DEFAULT GETDATE(),
    [ModifiedAt] DATETIME2 DEFAULT GETDATE()
);
GO

-- Table: tblDockOrderStatusHistory
CREATE TABLE [dbo].[tblDockOrderStatusHistory] (
    [HistoryId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [DockOrderId] UNIQUEIDENTIFIER NOT NULL,
    [OldStatus] NVARCHAR(50),
    [NewStatus] NVARCHAR(50),
    [ChangedAt] DATETIME2 DEFAULT GETDATE(),
    [ChangedBy] NVARCHAR(50),
    CONSTRAINT [FK_tblDockOrderStatusHistory_tblDockOrders] FOREIGN KEY ([DockOrderId]) REFERENCES [dbo].[tblDockOrders]([DockOrderId])
);
GO

-- =============================================
-- SECTION 7: ADMINISTRATION
-- =============================================

-- Table: tblOfficeMaster (matches original naming convention)
CREATE TABLE [dbo].[tblOfficeMaster] (
    [OfficeId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Code] NVARCHAR(20) NOT NULL UNIQUE,
    [Name] NVARCHAR(200) NOT NULL,
    [Address] NVARCHAR(500),
    [City] NVARCHAR(100),
    [State] NVARCHAR(2), -- US State codes
    [Zip] NVARCHAR(20),
    [Phone] NVARCHAR(50),
    [Contact] NVARCHAR(200),
    [Email] NVARCHAR(200),
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME2 DEFAULT GETDATE(),
    [ModifiedAt] DATETIME2 DEFAULT GETDATE()
);
GO

-- Table: tblWarehouseMaster (matches original naming convention)
CREATE TABLE [dbo].[tblWarehouseMaster] (
    [WarehouseId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Code] NVARCHAR(20) NOT NULL UNIQUE,
    [Name] NVARCHAR(200) NOT NULL,
    [Address] NVARCHAR(500),
    [City] NVARCHAR(100),
    [State] NVARCHAR(2),
    [Zip] NVARCHAR(20),
    [Phone] NVARCHAR(50),
    [ContactName] NVARCHAR(200),
    [ContactEmail] NVARCHAR(200),
    [OfficeCode] NVARCHAR(20),
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME2 DEFAULT GETDATE(),
    [ModifiedAt] DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT [FK_tblWarehouseMaster_tblOfficeMaster] FOREIGN KEY ([OfficeCode]) REFERENCES [dbo].[tblOfficeMaster]([Code])
);
GO

-- Table: tblPartMaster (matches original naming convention)
CREATE TABLE [dbo].[tblPartMaster] (
    [PartId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [PartNumber] NVARCHAR(100) NOT NULL UNIQUE,
    [Description] NVARCHAR(500),
    [SupplierCode] NVARCHAR(20),
    [Category] NVARCHAR(100),
    [UnitOfMeasure] NVARCHAR(20),
    [StandardPackQuantity] INT,
    [Weight] DECIMAL(10, 2),
    [WeightUnit] NVARCHAR(10),
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME2 DEFAULT GETDATE(),
    [ModifiedAt] DATETIME2 DEFAULT GETDATE()
);
GO

-- Table: tblSupplierMaster (matches original naming convention)
CREATE TABLE [dbo].[tblSupplierMaster] (
    [SupplierId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Code] NVARCHAR(20) NOT NULL UNIQUE,
    [Name] NVARCHAR(200) NOT NULL,
    [Address] NVARCHAR(500),
    [City] NVARCHAR(100),
    [State] NVARCHAR(2),
    [Zip] NVARCHAR(20),
    [Country] NVARCHAR(50),
    [Phone] NVARCHAR(50),
    [ContactName] NVARCHAR(200),
    [ContactEmail] NVARCHAR(200),
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME2 DEFAULT GETDATE(),
    [ModifiedAt] DATETIME2 DEFAULT GETDATE()
);
GO

-- Table: tblPlantMaster (Toyota Plants)
CREATE TABLE [dbo].[tblPlantMaster] (
    [PlantId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Code] NVARCHAR(20) NOT NULL UNIQUE,
    [Name] NVARCHAR(200) NOT NULL,
    [Address] NVARCHAR(500),
    [City] NVARCHAR(100),
    [State] NVARCHAR(2),
    [Zip] NVARCHAR(20),
    [Phone] NVARCHAR(50),
    [ContactName] NVARCHAR(200),
    [ContactEmail] NVARCHAR(200),
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME2 DEFAULT GETDATE(),
    [ModifiedAt] DATETIME2 DEFAULT GETDATE()
);
GO

-- Table: tblDockMaster
CREATE TABLE [dbo].[tblDockMaster] (
    [DockId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Code] NVARCHAR(20) NOT NULL UNIQUE,
    [Name] NVARCHAR(200) NOT NULL,
    [PlantCode] NVARCHAR(20),
    [WarehouseCode] NVARCHAR(20),
    [DockType] NVARCHAR(50) CHECK ([DockType] IN ('INBOUND', 'OUTBOUND', 'BOTH')),
    [IsActive] BIT DEFAULT 1,
    [CreatedAt] DATETIME2 DEFAULT GETDATE(),
    [ModifiedAt] DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT [FK_tblDockMaster_tblPlantMaster] FOREIGN KEY ([PlantCode]) REFERENCES [dbo].[tblPlantMaster]([Code]),
    CONSTRAINT [FK_tblDockMaster_tblWarehouseMaster] FOREIGN KEY ([WarehouseCode]) REFERENCES [dbo].[tblWarehouseMaster]([Code])
);
GO

-- =============================================
-- SECTION 8: AUDIT & LOGGING
-- =============================================

-- Table: tblAuditLog
CREATE TABLE [dbo].[tblAuditLog] (
    [AuditId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [TableName] NVARCHAR(100) NOT NULL,
    [RecordId] NVARCHAR(100),
    [Action] NVARCHAR(20) CHECK ([Action] IN ('INSERT', 'UPDATE', 'DELETE')),
    [OldValue] NVARCHAR(MAX),
    [NewValue] NVARCHAR(MAX),
    [ChangedBy] NVARCHAR(50),
    [ChangedAt] DATETIME2 DEFAULT GETDATE(),
    [IpAddress] NVARCHAR(50),
    [UserAgent] NVARCHAR(500)
);
GO

-- Table: tblSystemLog
CREATE TABLE [dbo].[tblSystemLog] (
    [LogId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [Level] NVARCHAR(20) CHECK ([Level] IN ('DEBUG', 'INFO', 'WARNING', 'ERROR', 'CRITICAL')),
    [Source] NVARCHAR(200),
    [Message] NVARCHAR(MAX),
    [Exception] NVARCHAR(MAX),
    [StackTrace] NVARCHAR(MAX),
    [UserId] NVARCHAR(50),
    [CreatedAt] DATETIME2 DEFAULT GETDATE()
);
GO

-- Table: tblErrorLog
CREATE TABLE [dbo].[tblErrorLog] (
    [ErrorId] UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    [ErrorCode] NVARCHAR(50),
    [ErrorMessage] NVARCHAR(MAX),
    [Source] NVARCHAR(200),
    [StackTrace] NVARCHAR(MAX),
    [UserId] NVARCHAR(50),
    [SessionId] UNIQUEIDENTIFIER,
    [IpAddress] NVARCHAR(50),
    [CreatedAt] DATETIME2 DEFAULT GETDATE()
);
GO

-- =============================================
-- SECTION 9: INDEXES FOR PERFORMANCE
-- =============================================

-- User indexes
CREATE NONCLUSTERED INDEX [IX_tblUserMaster_Username] ON [dbo].[tblUserMaster]([Username]);
CREATE NONCLUSTERED INDEX [IX_tblUserMaster_Email] ON [dbo].[tblUserMaster]([Email]);
CREATE NONCLUSTERED INDEX [IX_tblUserMaster_IsActive] ON [dbo].[tblUserMaster]([IsActive]);

-- Session indexes
CREATE NONCLUSTERED INDEX [IX_tblUserSessions_UserId] ON [dbo].[tblUserSessions]([UserId]);
CREATE NONCLUSTERED INDEX [IX_tblUserSessions_IsActive] ON [dbo].[tblUserSessions]([IsActive]);

-- Order indexes
CREATE NONCLUSTERED INDEX [IX_tblOrders_OwkNumber] ON [dbo].[tblOrders]([OwkNumber]);
CREATE NONCLUSTERED INDEX [IX_tblOrders_Status] ON [dbo].[tblOrders]([Status]);
CREATE NONCLUSTERED INDEX [IX_tblOrders_OrderDate] ON [dbo].[tblOrders]([OrderDate]);

-- Planned items indexes
CREATE NONCLUSTERED INDEX [IX_tblPlannedItems_OwkNumber] ON [dbo].[tblPlannedItems]([OwkNumber]);
CREATE NONCLUSTERED INDEX [IX_tblPlannedItems_PartNumber] ON [dbo].[tblPlannedItems]([PartNumber]);

-- Skid build session indexes
CREATE NONCLUSTERED INDEX [IX_tblSkidBuildSessions_UserId] ON [dbo].[tblSkidBuildSessions]([UserId]);
CREATE NONCLUSTERED INDEX [IX_tblSkidBuildSessions_OwkNumber] ON [dbo].[tblSkidBuildSessions]([OwkNumber]);
CREATE NONCLUSTERED INDEX [IX_tblSkidBuildSessions_Status] ON [dbo].[tblSkidBuildSessions]([Status]);

-- Toyota Manifest indexes
CREATE NONCLUSTERED INDEX [IX_tblToyotaManifests_SessionId] ON [dbo].[tblToyotaManifests]([SessionId]);
CREATE NONCLUSTERED INDEX [IX_tblToyotaManifests_SkidId] ON [dbo].[tblToyotaManifests]([SkidId]);

-- Toyota Kanban indexes
CREATE NONCLUSTERED INDEX [IX_tblToyotaKanbans_SessionId] ON [dbo].[tblToyotaKanbans]([SessionId]);
CREATE NONCLUSTERED INDEX [IX_tblToyotaKanbans_PartNumber] ON [dbo].[tblToyotaKanbans]([PartNumber]);

-- Internal Kanban indexes
CREATE NONCLUSTERED INDEX [IX_tblInternalKanbans_SessionId] ON [dbo].[tblInternalKanbans]([SessionId]);
CREATE NONCLUSTERED INDEX [IX_tblInternalKanbans_ToyotaKanbanId] ON [dbo].[tblInternalKanbans]([ToyotaKanbanId]);

-- Scanned items indexes
CREATE NONCLUSTERED INDEX [IX_tblScannedItems_SessionId] ON [dbo].[tblScannedItems]([SessionId]);
CREATE NONCLUSTERED INDEX [IX_tblScannedItems_PartNumber] ON [dbo].[tblScannedItems]([PartNumber]);

-- Shipment load session indexes
CREATE NONCLUSTERED INDEX [IX_tblShipmentLoadSessions_RouteNumber] ON [dbo].[tblShipmentLoadSessions]([RouteNumber]);
CREATE NONCLUSTERED INDEX [IX_tblShipmentLoadSessions_UserId] ON [dbo].[tblShipmentLoadSessions]([UserId]);
CREATE NONCLUSTERED INDEX [IX_tblShipmentLoadSessions_Status] ON [dbo].[tblShipmentLoadSessions]([Status]);

-- Planned skids indexes
CREATE NONCLUSTERED INDEX [IX_tblPlannedSkids_RouteNumber] ON [dbo].[tblPlannedSkids]([RouteNumber]);
CREATE NONCLUSTERED INDEX [IX_tblPlannedSkids_SkidId] ON [dbo].[tblPlannedSkids]([SkidId]);

-- Scanned skids indexes
CREATE NONCLUSTERED INDEX [IX_tblScannedSkids_SessionId] ON [dbo].[tblScannedSkids]([SessionId]);
CREATE NONCLUSTERED INDEX [IX_tblScannedSkids_SkidId] ON [dbo].[tblScannedSkids]([SkidId]);

-- Pre-shipment indexes
CREATE NONCLUSTERED INDEX [IX_tblPreShipmentShipments_CreatedBy] ON [dbo].[tblPreShipmentShipments]([CreatedBy]);
CREATE NONCLUSTERED INDEX [IX_tblPreShipmentShipments_Status] ON [dbo].[tblPreShipmentShipments]([Status]);

-- Dock monitor indexes
CREATE NONCLUSTERED INDEX [IX_tblDockOrders_OrderNumber] ON [dbo].[tblDockOrders]([OrderNumber]);
CREATE NONCLUSTERED INDEX [IX_tblDockOrders_Status] ON [dbo].[tblDockOrders]([Status]);
CREATE NONCLUSTERED INDEX [IX_tblDockOrders_Location] ON [dbo].[tblDockOrders]([Location]);

-- Part master indexes
CREATE NONCLUSTERED INDEX [IX_tblPartMaster_PartNumber] ON [dbo].[tblPartMaster]([PartNumber]);
CREATE NONCLUSTERED INDEX [IX_tblPartMaster_SupplierCode] ON [dbo].[tblPartMaster]([SupplierCode]);

-- Audit log indexes
CREATE NONCLUSTERED INDEX [IX_tblAuditLog_TableName] ON [dbo].[tblAuditLog]([TableName]);
CREATE NONCLUSTERED INDEX [IX_tblAuditLog_ChangedBy] ON [dbo].[tblAuditLog]([ChangedBy]);
CREATE NONCLUSTERED INDEX [IX_tblAuditLog_ChangedAt] ON [dbo].[tblAuditLog]([ChangedAt]);

-- System log indexes
CREATE NONCLUSTERED INDEX [IX_tblSystemLog_Level] ON [dbo].[tblSystemLog]([Level]);
CREATE NONCLUSTERED INDEX [IX_tblSystemLog_CreatedAt] ON [dbo].[tblSystemLog]([CreatedAt]);

-- =============================================
-- SECTION 10: SEED DATA
-- =============================================

-- Insert default admin user (password: Admin@123)
INSERT INTO [dbo].[tblUserMaster] ([UserId], [Username], [PasswordHash], [Name], [Email], [Role], [MenuLevel], [Operation], [IsActive])
VALUES (
    'ADMIN001',
    'admin',
    '$2a$11$rGZvQKEWqCJqJ2nPF8YyTu0YxGPZ5K0F3JQxLMCkFQQQhFxLqXKLK', -- Admin@123
    'System Administrator',
    'admin@vuteq.com',
    'ADMIN',
    'Admin',
    'Administration',
    1
);
GO

-- Insert default supervisor user (password: Super@123)
INSERT INTO [dbo].[tblUserMaster] ([UserId], [Username], [PasswordHash], [Name], [Email], [Role], [MenuLevel], [Operation], [IsSupervisor], [IsActive])
VALUES (
    'SUPER001',
    'supervisor',
    '$2a$11$tN8qJ5R3mH6yPQXvWkZAG.jYx4Kq8LmN9pOiUyTrEwQaSdFgHjKlM', -- Super@123
    'Warehouse Supervisor',
    'supervisor@vuteq.com',
    'SUPERVISOR',
    'Scanner',
    'Warehouse',
    1,
    1
);
GO

-- Insert default operator user (password: Oper@123)
INSERT INTO [dbo].[tblUserMaster] ([UserId], [Username], [PasswordHash], [Name], [Email], [Role], [MenuLevel], [Operation], [IsActive])
VALUES (
    'OPER001',
    'operator',
    '$2a$11$wK7pL2QxMnHyRzVxWqYeA.kLx3Kp9JmO8nPjVzTsEuRbScDeFhIjL', -- Oper@123
    'Warehouse Operator',
    'operator@vuteq.com',
    'OPERATOR',
    'Scanner',
    'Warehouse',
    1
);
GO

-- Insert sample offices
INSERT INTO [dbo].[tblOfficeMaster] ([Code], [Name], [City], [State], [IsActive])
VALUES
    ('TMH', 'Toyota Motor Manufacturing Houston', 'Houston', 'TX', 1),
    ('TMI', 'Toyota Motor Manufacturing Indiana', 'Princeton', 'IN', 1),
    ('TMK', 'Toyota Motor Manufacturing Kentucky', 'Georgetown', 'KY', 1);
GO

-- Insert sample warehouses
INSERT INTO [dbo].[tblWarehouseMaster] ([Code], [Name], [City], [State], [OfficeCode], [IsActive])
VALUES
    ('WH-TMH-01', 'Houston Warehouse 1', 'Houston', 'TX', 'TMH', 1),
    ('WH-TMI-01', 'Indiana Warehouse 1', 'Princeton', 'IN', 'TMI', 1),
    ('WH-TMK-01', 'Kentucky Warehouse 1', 'Georgetown', 'KY', 'TMK', 1);
GO

-- Insert sample plants
INSERT INTO [dbo].[tblPlantMaster] ([Code], [Name], [City], [State], [IsActive])
VALUES
    ('TMMTX', 'Toyota Motor Manufacturing Texas', 'San Antonio', 'TX', 1),
    ('TMMIN', 'Toyota Motor Manufacturing Indiana', 'Princeton', 'IN', 1),
    ('TMMK', 'Toyota Motor Manufacturing Kentucky', 'Georgetown', 'KY', 1);
GO

-- Insert sample suppliers
INSERT INTO [dbo].[tblSupplierMaster] ([Code], [Name], [City], [State], [IsActive])
VALUES
    ('SUP001', 'VUTEQ USA LLC', 'Houston', 'TX', 1),
    ('SUP002', 'Sample Supplier 2', 'Detroit', 'MI', 1);
GO

-- Insert sample docks
INSERT INTO [dbo].[tblDockMaster] ([Code], [Name], [PlantCode], [DockType], [IsActive])
VALUES
    ('D001', 'Dock 1', 'TMMTX', 'BOTH', 1),
    ('D002', 'Dock 2', 'TMMIN', 'BOTH', 1),
    ('D003', 'Dock 3', 'TMMK', 'BOTH', 1);
GO

PRINT 'Database VUTEQ_Scanner created successfully with all tables, indexes, and seed data!';
GO
