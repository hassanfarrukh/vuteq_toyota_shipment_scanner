using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tblDockOrders",
                columns: table => new
                {
                    DockOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Route = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Destination = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Supplier = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PlannedSkidBuild = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedSkidBuild = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlannedShipmentLoad = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedShipmentLoad = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsSupplementOrder = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblDockOrders", x => x.DockOrderId);
                });

            migrationBuilder.CreateTable(
                name: "tblInternalKanbanSettings",
                columns: table => new
                {
                    SettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AllowDuplicates = table.Column<bool>(type: "bit", nullable: false),
                    DuplicateWindow = table.Column<int>(type: "int", nullable: false),
                    AlertOnDuplicate = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblInternalKanbanSettings", x => x.SettingId);
                });

            migrationBuilder.CreateTable(
                name: "tblOfficeMaster",
                columns: table => new
                {
                    OfficeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    Zip = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Contact = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblOfficeMaster", x => x.OfficeId);
                    table.UniqueConstraint("AK_tblOfficeMaster_Code", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "tblOrders",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwkNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Destination = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TotalSkids = table.Column<int>(type: "int", nullable: true),
                    PlantCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SupplierCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DockCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LoadId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblOrders", x => x.OrderId);
                    table.UniqueConstraint("AK_tblOrders_OwkNumber", x => x.OwkNumber);
                });

            migrationBuilder.CreateTable(
                name: "tblPartMaster",
                columns: table => new
                {
                    PartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    WeightPerPiece = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    UomPerPiece = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PartType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PackingStyle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CommonPart = table.Column<bool>(type: "bit", nullable: false),
                    Discontinued = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblPartMaster", x => x.PartId);
                });

            migrationBuilder.CreateTable(
                name: "tblPickupRoutes",
                columns: table => new
                {
                    RouteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QrCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RouteNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Plant = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SupplierCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DockCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    EstimatedSkids = table.Column<int>(type: "int", nullable: true),
                    OrderDate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PickupDate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PickupTime = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblPickupRoutes", x => x.RouteId);
                });

            migrationBuilder.CreateTable(
                name: "tblPlannedSkids",
                columns: table => new
                {
                    PlannedSkidId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RouteNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SkidId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PartCount = table.Column<int>(type: "int", nullable: true),
                    Destination = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Plant = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SupplierCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblPlannedSkids", x => x.PlannedSkidId);
                });

            migrationBuilder.CreateTable(
                name: "tblUserMaster",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MenuLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Operation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LocationId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsSupervisor = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblUserMaster", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "tblDockOrderStatusHistory",
                columns: table => new
                {
                    HistoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DockOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OldStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NewStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblDockOrderStatusHistory", x => x.HistoryId);
                    table.ForeignKey(
                        name: "FK_tblDockOrderStatusHistory_tblDockOrders_DockOrderId",
                        column: x => x.DockOrderId,
                        principalTable: "tblDockOrders",
                        principalColumn: "DockOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblWarehouseMaster",
                columns: table => new
                {
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    Zip = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContactName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OfficeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblWarehouseMaster", x => x.WarehouseId);
                    table.ForeignKey(
                        name: "FK_tblWarehouseMaster_tblOfficeMaster_OfficeCode",
                        column: x => x.OfficeCode,
                        principalTable: "tblOfficeMaster",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tblPlannedItems",
                columns: table => new
                {
                    PlannedItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwkNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PartNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PlannedQty = table.Column<int>(type: "int", nullable: false),
                    RawKanbanValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SupplierCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblPlannedItems", x => x.PlannedItemId);
                    table.ForeignKey(
                        name: "FK_tblPlannedItems_tblOrders_OwkNumber",
                        column: x => x.OwkNumber,
                        principalTable: "tblOrders",
                        principalColumn: "OwkNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblDockMonitorSettings",
                columns: table => new
                {
                    SettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BehindThreshold = table.Column<int>(type: "int", nullable: false),
                    CriticalThreshold = table.Column<int>(type: "int", nullable: false),
                    DisplayMode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SelectedLocations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshInterval = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblDockMonitorSettings", x => x.SettingId);
                    table.ForeignKey(
                        name: "FK_tblDockMonitorSettings_tblUserMaster_UserId",
                        column: x => x.UserId,
                        principalTable: "tblUserMaster",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblOrderUploads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UploadedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblOrderUploads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tblOrderUploads_tblUserMaster_UploadedBy",
                        column: x => x.UploadedBy,
                        principalTable: "tblUserMaster",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "tblPreShipmentShipments",
                columns: table => new
                {
                    ShipmentId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CurrentScreen = table.Column<int>(type: "int", nullable: false),
                    TrailerNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SealNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CarrierName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DriverName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConfirmationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblPreShipmentShipments", x => x.ShipmentId);
                    table.ForeignKey(
                        name: "FK_tblPreShipmentShipments_tblUserMaster_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "tblUserMaster",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tblSettings",
                columns: table => new
                {
                    SettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SettingKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SettingValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SettingType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblSettings", x => x.SettingId);
                    table.ForeignKey(
                        name: "FK_tblSettings_tblUserMaster_UserId",
                        column: x => x.UserId,
                        principalTable: "tblUserMaster",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "tblShipmentLoadSessions",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RouteNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrailerNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SealNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CarrierName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DriverName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CurrentScreen = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConfirmationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblShipmentLoadSessions", x => x.SessionId);
                    table.ForeignKey(
                        name: "FK_tblShipmentLoadSessions_tblUserMaster_UserId",
                        column: x => x.UserId,
                        principalTable: "tblUserMaster",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tblSkidBuildSessions",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OwkNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SupplierCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CurrentScreen = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConfirmationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblSkidBuildSessions", x => x.SessionId);
                    table.ForeignKey(
                        name: "FK_tblSkidBuildSessions_tblOrders_OwkNumber",
                        column: x => x.OwkNumber,
                        principalTable: "tblOrders",
                        principalColumn: "OwkNumber",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tblSkidBuildSessions_tblUserMaster_UserId",
                        column: x => x.UserId,
                        principalTable: "tblUserMaster",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tblUserSessions",
                columns: table => new
                {
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastActivityAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblUserSessions", x => x.SessionId);
                    table.ForeignKey(
                        name: "FK_tblUserSessions_tblUserMaster_UserId",
                        column: x => x.UserId,
                        principalTable: "tblUserMaster",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblPreShipmentExceptions",
                columns: table => new
                {
                    ExceptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShipmentId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExceptionType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RelatedSkidId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblPreShipmentExceptions", x => x.ExceptionId);
                    table.ForeignKey(
                        name: "FK_tblPreShipmentExceptions_tblPreShipmentShipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "tblPreShipmentShipments",
                        principalColumn: "ShipmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblPreShipmentManifests",
                columns: table => new
                {
                    ManifestRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShipmentId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ManifestId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ScannedValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ScannedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblPreShipmentManifests", x => x.ManifestRecordId);
                    table.ForeignKey(
                        name: "FK_tblPreShipmentManifests_tblPreShipmentShipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "tblPreShipmentShipments",
                        principalColumn: "ShipmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblPreShipmentScannedSkids",
                columns: table => new
                {
                    ScannedSkidId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShipmentId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SkidId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PartCount = table.Column<int>(type: "int", nullable: true),
                    Destination = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ScannedValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ScannedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblPreShipmentScannedSkids", x => x.ScannedSkidId);
                    table.ForeignKey(
                        name: "FK_tblPreShipmentScannedSkids_tblPreShipmentShipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "tblPreShipmentShipments",
                        principalColumn: "ShipmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblScannedSkids",
                columns: table => new
                {
                    ScannedSkidId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SkidId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PartCount = table.Column<int>(type: "int", nullable: true),
                    Destination = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ScannedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblScannedSkids", x => x.ScannedSkidId);
                    table.ForeignKey(
                        name: "FK_tblScannedSkids_tblShipmentLoadSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "tblShipmentLoadSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblShipmentLoadDrafts",
                columns: table => new
                {
                    DraftId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RouteNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DraftData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentScreen = table.Column<int>(type: "int", nullable: true),
                    SavedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblShipmentLoadDrafts", x => x.DraftId);
                    table.ForeignKey(
                        name: "FK_tblShipmentLoadDrafts_tblShipmentLoadSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "tblShipmentLoadSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tblShipmentLoadDrafts_tblUserMaster_UserId",
                        column: x => x.UserId,
                        principalTable: "tblUserMaster",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblShipmentLoadExceptions",
                columns: table => new
                {
                    ExceptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExceptionType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RelatedSkidId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedByUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblShipmentLoadExceptions", x => x.ExceptionId);
                    table.ForeignKey(
                        name: "FK_tblShipmentLoadExceptions_tblShipmentLoadSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "tblShipmentLoadSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblScannedItems",
                columns: table => new
                {
                    ScannedItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PartNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    KanbanNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InternalKanban = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ScannedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblScannedItems", x => x.ScannedItemId);
                    table.ForeignKey(
                        name: "FK_tblScannedItems_tblSkidBuildSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "tblSkidBuildSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblSkidBuildDrafts",
                columns: table => new
                {
                    DraftId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwkNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DraftData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentScreen = table.Column<int>(type: "int", nullable: true),
                    SavedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblSkidBuildDrafts", x => x.DraftId);
                    table.ForeignKey(
                        name: "FK_tblSkidBuildDrafts_tblSkidBuildSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "tblSkidBuildSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tblSkidBuildDrafts_tblUserMaster_UserId",
                        column: x => x.UserId,
                        principalTable: "tblUserMaster",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblSkidBuildExceptions",
                columns: table => new
                {
                    ExceptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwkNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExceptionType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedByUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblSkidBuildExceptions", x => x.ExceptionId);
                    table.ForeignKey(
                        name: "FK_tblSkidBuildExceptions_tblSkidBuildSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "tblSkidBuildSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tblToyotaKanbans",
                columns: table => new
                {
                    KanbanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QrCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PartNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SupplierCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DockCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Quantity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    KanbanNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ShipToAddress1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ShipToAddress2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DeliveryDate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeliveryTime = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PlantCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Route = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContainerType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PalletCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    StorageLocation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SequenceNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BatchNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ManufacturingDate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExpiryDate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LotNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RevisionLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    QualityStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ScannedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScannedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblToyotaKanbans", x => x.KanbanId);
                    table.ForeignKey(
                        name: "FK_tblToyotaKanbans_tblSkidBuildSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "tblSkidBuildSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tblToyotaManifests",
                columns: table => new
                {
                    ManifestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QrCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PlantPrefix = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PlantCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    SupplierCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DockCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LoadId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PalletizationCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Mros = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    SkidId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FormattedSkidId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ScannedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScannedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblToyotaManifests", x => x.ManifestId);
                    table.ForeignKey(
                        name: "FK_tblToyotaManifests_tblSkidBuildSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "tblSkidBuildSessions",
                        principalColumn: "SessionId");
                });

            migrationBuilder.CreateTable(
                name: "tblInternalKanbans",
                columns: table => new
                {
                    InternalKanbanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScanValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ToyotaKanban = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InternalKanbanValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ToyotaKanbanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ScannedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScannedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblInternalKanbans", x => x.InternalKanbanId);
                    table.ForeignKey(
                        name: "FK_tblInternalKanbans_tblSkidBuildSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "tblSkidBuildSessions",
                        principalColumn: "SessionId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tblInternalKanbans_tblToyotaKanbans_ToyotaKanbanId",
                        column: x => x.ToyotaKanbanId,
                        principalTable: "tblToyotaKanbans",
                        principalColumn: "KanbanId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tblDockMonitorSettings_UserId",
                table: "tblDockMonitorSettings",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblDockOrders_Location",
                table: "tblDockOrders",
                column: "Location");

            migrationBuilder.CreateIndex(
                name: "IX_tblDockOrders_OrderNumber",
                table: "tblDockOrders",
                column: "OrderNumber");

            migrationBuilder.CreateIndex(
                name: "IX_tblDockOrders_Status",
                table: "tblDockOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_tblDockOrderStatusHistory_DockOrderId",
                table: "tblDockOrderStatusHistory",
                column: "DockOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_tblInternalKanbans_InternalKanbanValue",
                table: "tblInternalKanbans",
                column: "InternalKanbanValue");

            migrationBuilder.CreateIndex(
                name: "IX_tblInternalKanbans_ScannedAt",
                table: "tblInternalKanbans",
                column: "ScannedAt");

            migrationBuilder.CreateIndex(
                name: "IX_tblInternalKanbans_SessionId",
                table: "tblInternalKanbans",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_tblInternalKanbans_ToyotaKanbanId",
                table: "tblInternalKanbans",
                column: "ToyotaKanbanId");

            migrationBuilder.CreateIndex(
                name: "IX_tblOfficeMaster_Code",
                table: "tblOfficeMaster",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblOfficeMaster_IsActive",
                table: "tblOfficeMaster",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_tblOrders_OwkNumber",
                table: "tblOrders",
                column: "OwkNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblOrders_Status",
                table: "tblOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_tblOrders_SupplierCode",
                table: "tblOrders",
                column: "SupplierCode");

            migrationBuilder.CreateIndex(
                name: "IX_tblOrderUploads_UploadedBy",
                table: "tblOrderUploads",
                column: "UploadedBy");

            migrationBuilder.CreateIndex(
                name: "IX_tblPartMaster_IsActive",
                table: "tblPartMaster",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_tblPartMaster_PartNo",
                table: "tblPartMaster",
                column: "PartNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblPartMaster_PartType",
                table: "tblPartMaster",
                column: "PartType");

            migrationBuilder.CreateIndex(
                name: "IX_tblPlannedItems_OwkNumber",
                table: "tblPlannedItems",
                column: "OwkNumber");

            migrationBuilder.CreateIndex(
                name: "IX_tblPlannedItems_PartNumber",
                table: "tblPlannedItems",
                column: "PartNumber");

            migrationBuilder.CreateIndex(
                name: "IX_tblPlannedSkids_RouteNumber",
                table: "tblPlannedSkids",
                column: "RouteNumber");

            migrationBuilder.CreateIndex(
                name: "IX_tblPlannedSkids_SkidId",
                table: "tblPlannedSkids",
                column: "SkidId");

            migrationBuilder.CreateIndex(
                name: "IX_tblPreShipmentExceptions_ShipmentId",
                table: "tblPreShipmentExceptions",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_tblPreShipmentManifests_ShipmentId",
                table: "tblPreShipmentManifests",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_tblPreShipmentScannedSkids_ShipmentId",
                table: "tblPreShipmentScannedSkids",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_tblPreShipmentShipments_CreatedAt",
                table: "tblPreShipmentShipments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_tblPreShipmentShipments_CreatedByUserId",
                table: "tblPreShipmentShipments",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_tblPreShipmentShipments_Status",
                table: "tblPreShipmentShipments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_tblScannedItems_SessionId",
                table: "tblScannedItems",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_tblScannedSkids_SessionId",
                table: "tblScannedSkids",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_tblSettings_UserId",
                table: "tblSettings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tblShipmentLoadDrafts_SessionId",
                table: "tblShipmentLoadDrafts",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_tblShipmentLoadDrafts_UserId",
                table: "tblShipmentLoadDrafts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tblShipmentLoadExceptions_SessionId",
                table: "tblShipmentLoadExceptions",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_tblShipmentLoadSessions_RouteNumber",
                table: "tblShipmentLoadSessions",
                column: "RouteNumber");

            migrationBuilder.CreateIndex(
                name: "IX_tblShipmentLoadSessions_Status",
                table: "tblShipmentLoadSessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_tblShipmentLoadSessions_UserId",
                table: "tblShipmentLoadSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tblSkidBuildDrafts_SessionId",
                table: "tblSkidBuildDrafts",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_tblSkidBuildDrafts_UserId",
                table: "tblSkidBuildDrafts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tblSkidBuildExceptions_SessionId",
                table: "tblSkidBuildExceptions",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_tblSkidBuildSessions_CreatedAt",
                table: "tblSkidBuildSessions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_tblSkidBuildSessions_OwkNumber",
                table: "tblSkidBuildSessions",
                column: "OwkNumber");

            migrationBuilder.CreateIndex(
                name: "IX_tblSkidBuildSessions_Status",
                table: "tblSkidBuildSessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_tblSkidBuildSessions_UserId",
                table: "tblSkidBuildSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tblToyotaKanbans_KanbanNumber",
                table: "tblToyotaKanbans",
                column: "KanbanNumber");

            migrationBuilder.CreateIndex(
                name: "IX_tblToyotaKanbans_PartNumber",
                table: "tblToyotaKanbans",
                column: "PartNumber");

            migrationBuilder.CreateIndex(
                name: "IX_tblToyotaKanbans_SessionId",
                table: "tblToyotaKanbans",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_tblToyotaManifests_SessionId",
                table: "tblToyotaManifests",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_tblUserMaster_IsActive",
                table: "tblUserMaster",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_tblUserMaster_Role",
                table: "tblUserMaster",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_tblUserMaster_Username",
                table: "tblUserMaster",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblUserSessions_ExpiresAt",
                table: "tblUserSessions",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_tblUserSessions_IsActive",
                table: "tblUserSessions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_tblUserSessions_UserId",
                table: "tblUserSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tblWarehouseMaster_Code",
                table: "tblWarehouseMaster",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tblWarehouseMaster_IsActive",
                table: "tblWarehouseMaster",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_tblWarehouseMaster_OfficeCode",
                table: "tblWarehouseMaster",
                column: "OfficeCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblDockMonitorSettings");

            migrationBuilder.DropTable(
                name: "tblDockOrderStatusHistory");

            migrationBuilder.DropTable(
                name: "tblInternalKanbans");

            migrationBuilder.DropTable(
                name: "tblInternalKanbanSettings");

            migrationBuilder.DropTable(
                name: "tblOrderUploads");

            migrationBuilder.DropTable(
                name: "tblPartMaster");

            migrationBuilder.DropTable(
                name: "tblPickupRoutes");

            migrationBuilder.DropTable(
                name: "tblPlannedItems");

            migrationBuilder.DropTable(
                name: "tblPlannedSkids");

            migrationBuilder.DropTable(
                name: "tblPreShipmentExceptions");

            migrationBuilder.DropTable(
                name: "tblPreShipmentManifests");

            migrationBuilder.DropTable(
                name: "tblPreShipmentScannedSkids");

            migrationBuilder.DropTable(
                name: "tblScannedItems");

            migrationBuilder.DropTable(
                name: "tblScannedSkids");

            migrationBuilder.DropTable(
                name: "tblSettings");

            migrationBuilder.DropTable(
                name: "tblShipmentLoadDrafts");

            migrationBuilder.DropTable(
                name: "tblShipmentLoadExceptions");

            migrationBuilder.DropTable(
                name: "tblSkidBuildDrafts");

            migrationBuilder.DropTable(
                name: "tblSkidBuildExceptions");

            migrationBuilder.DropTable(
                name: "tblToyotaManifests");

            migrationBuilder.DropTable(
                name: "tblUserSessions");

            migrationBuilder.DropTable(
                name: "tblWarehouseMaster");

            migrationBuilder.DropTable(
                name: "tblDockOrders");

            migrationBuilder.DropTable(
                name: "tblToyotaKanbans");

            migrationBuilder.DropTable(
                name: "tblPreShipmentShipments");

            migrationBuilder.DropTable(
                name: "tblShipmentLoadSessions");

            migrationBuilder.DropTable(
                name: "tblOfficeMaster");

            migrationBuilder.DropTable(
                name: "tblSkidBuildSessions");

            migrationBuilder.DropTable(
                name: "tblOrders");

            migrationBuilder.DropTable(
                name: "tblUserMaster");
        }
    }
}
