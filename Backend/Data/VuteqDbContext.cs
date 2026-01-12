// Author: Hassan
// Date: 2025-11-23
// Description: Entity Framework Core DbContext for VUTEQ Scanner Application with automatic audit tracking

using Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Backend.Data;

/// <summary>
/// Database context for VUTEQ Scanner Application
/// Handles all database operations and entity tracking
/// </summary>
public class VuteqDbContext : DbContext
{
    public VuteqDbContext(DbContextOptions<VuteqDbContext> options) : base(options)
    {
    }

    #region DbSets - Authentication & Users
    public DbSet<UserMaster> UserMasters { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    #endregion

    #region DbSets - Orders & Uploads
    public DbSet<OrderUpload> OrderUploads { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<PlannedItem> PlannedItems { get; set; }
    #endregion

    #region DbSets - Skid Build Workflow
    public DbSet<SkidBuildSession> SkidBuildSessions { get; set; }
    public DbSet<ToyotaManifest> ToyotaManifests { get; set; }
    public DbSet<ToyotaKanban> ToyotaKanbans { get; set; }
    public DbSet<InternalKanban> InternalKanbans { get; set; }
    public DbSet<ScannedItem> ScannedItems { get; set; }
    public DbSet<SkidScan> SkidScans { get; set; }
    public DbSet<SkidBuildException> SkidBuildExceptions { get; set; }
    public DbSet<SkidBuildDraft> SkidBuildDrafts { get; set; }
    #endregion

    #region DbSets - Shipment Load Workflow
    public DbSet<PickupRoute> PickupRoutes { get; set; }
    public DbSet<ShipmentLoadSession> ShipmentLoadSessions { get; set; }
    public DbSet<PlannedSkid> PlannedSkids { get; set; }
    public DbSet<ShipmentLoadException> ShipmentLoadExceptions { get; set; }
    // Removed: ScannedSkid (unused - Shipment Load verifies SkidScans from Skid Build)
    // Removed: ShipmentLoadDraft (unused - session provides persistence)
    #endregion

    #region DbSets - Pre-Shipment Scan Workflow
    public DbSet<PreShipmentShipment> PreShipmentShipments { get; set; }
    public DbSet<PreShipmentManifest> PreShipmentManifests { get; set; }
    public DbSet<PreShipmentScannedSkid> PreShipmentScannedSkids { get; set; }
    public DbSet<PreShipmentException> PreShipmentExceptions { get; set; }
    #endregion

    #region DbSets - Dock Monitor
    public DbSet<DockOrder> DockOrders { get; set; }
    public DbSet<DockOrderStatusHistory> DockOrderStatusHistories { get; set; }
    #endregion

    #region DbSets - Administration
    public DbSet<OfficeMaster> OfficeMasters { get; set; }
    public DbSet<WarehouseMaster> WarehouseMasters { get; set; }
    public DbSet<PartMaster> PartMasters { get; set; }
    #endregion

    #region DbSets - Settings
    public DbSet<Setting> Settings { get; set; }
    public DbSet<ToyotaApiConfig> ToyotaApiConfigs { get; set; }
    public DbSet<SiteSettings> SiteSettings { get; set; }
    #endregion

    /// <summary>
    /// Configure entity relationships and constraints
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        #region UserMaster Configurations
        modelBuilder.Entity<UserMaster>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Role);
            entity.HasIndex(e => e.IsActive);
        });
        #endregion

        #region UserSession Configurations
        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.ExpiresAt);

            entity.HasOne(s => s.User)
                .WithMany(u => u.UserSessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        #endregion

        #region Order Configurations
        modelBuilder.Entity<Order>(entity =>
        {
            // Composite unique index on RealOrderNumber + DockCode (replaces OrderSeries + OrderNumber)
            entity.HasIndex(e => new { e.RealOrderNumber, e.DockCode }).IsUnique();
            entity.HasIndex(e => e.SupplierCode);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ShipmentLoadSessionId);

            // Configure enum conversion for OrderStatus
            entity.Property(e => e.Status)
                .HasConversion<int>();

            // Relationship to ShipmentLoadSession
            entity.HasOne<ShipmentLoadSession>()
                .WithMany()
                .HasForeignKey(o => o.ShipmentLoadSessionId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        #endregion

        #region PlannedItem Configurations
        modelBuilder.Entity<PlannedItem>(entity =>
        {
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.PartNumber);

            // FK relationship: OrderId GUID -> Order.OrderId GUID
            entity.HasOne(p => p.Order)
                .WithMany(o => o.PlannedItems)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        #endregion

        #region SkidBuildSession Configurations
        modelBuilder.Entity<SkidBuildSession>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(s => s.User)
                .WithMany(u => u.SkidBuildSessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Order)
                .WithMany(o => o.SkidBuildSessions)
                .HasForeignKey(s => s.OrderId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        #endregion

        #region ToyotaKanban Configurations
        modelBuilder.Entity<ToyotaKanban>(entity =>
        {
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.KanbanNumber);
            entity.HasIndex(e => e.PartNumber);

            entity.HasOne(k => k.Session)
                .WithMany(s => s.ToyotaKanbans)
                .HasForeignKey(k => k.SessionId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        #endregion

        #region InternalKanban Configurations
        modelBuilder.Entity<InternalKanban>(entity =>
        {
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.InternalKanbanValue);
            entity.HasIndex(e => e.ScannedAt);

            entity.HasOne(i => i.Session)
                .WithMany(s => s.InternalKanbans)
                .HasForeignKey(i => i.SessionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(i => i.ToyotaKanbanReference)
                .WithMany(t => t.InternalKanbans)
                .HasForeignKey(i => i.ToyotaKanbanId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        #endregion

        #region SkidScan Configurations
        modelBuilder.Entity<SkidScan>(entity =>
        {
            entity.HasIndex(e => e.PlannedItemId);
            entity.HasIndex(e => e.SkidNumber);
            entity.HasIndex(e => e.ScannedAt);

            entity.HasOne(s => s.PlannedItem)
                .WithMany(p => p.SkidScans)
                .HasForeignKey(s => s.PlannedItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.ScannedByUser)
                .WithMany()
                .HasForeignKey(s => s.ScannedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });
        #endregion

        #region SkidBuildException Configurations
        modelBuilder.Entity<SkidBuildException>(entity =>
        {
            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.ExceptionCode);

            entity.HasOne(e => e.Order)
                .WithMany(o => o.SkidBuildExceptions)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        #endregion

        #region ShipmentLoadSession Configurations
        modelBuilder.Entity<ShipmentLoadSession>(entity =>
        {
            entity.HasIndex(e => e.RouteNumber);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);

            entity.HasOne(s => s.User)
                .WithMany(u => u.ShipmentLoadSessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        #endregion

        #region PlannedSkid Configurations
        modelBuilder.Entity<PlannedSkid>(entity =>
        {
            entity.HasIndex(e => e.RouteNumber);
            entity.HasIndex(e => e.SkidId);
        });
        #endregion

        #region PreShipmentShipment Configurations
        modelBuilder.Entity<PreShipmentShipment>(entity =>
        {
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedByUserId);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(s => s.CreatedByUser)
                .WithMany(u => u.PreShipmentShipments)
                .HasForeignKey(s => s.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        #endregion

        #region DockOrder Configurations
        modelBuilder.Entity<DockOrder>(entity =>
        {
            entity.HasIndex(e => e.OrderNumber);
            entity.HasIndex(e => e.Location);
            entity.HasIndex(e => e.Status);
        });
        #endregion

        #region OfficeMaster Configurations
        modelBuilder.Entity<OfficeMaster>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.IsActive);
        });
        #endregion

        #region WarehouseMaster Configurations
        modelBuilder.Entity<WarehouseMaster>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => e.OfficeCode);
            entity.HasIndex(e => e.IsActive);

            entity.HasOne(w => w.Office)
                .WithMany(o => o.Warehouses)
                .HasForeignKey(w => w.OfficeCode)
                .HasPrincipalKey(o => o.Code)
                .OnDelete(DeleteBehavior.SetNull);
        });
        #endregion

        #region PartMaster Configurations
        modelBuilder.Entity<PartMaster>(entity =>
        {
            entity.HasIndex(e => e.PartNo).IsUnique();
            entity.HasIndex(e => e.PartType);
            entity.HasIndex(e => e.IsActive);
        });
        #endregion

        #region DockMonitorSetting Configurations
        modelBuilder.Entity<DockMonitorSetting>(entity =>
        {
            // DockMonitorSetting is now global (system-wide) - no FK relationship to User
            // UserId is nullable for backwards compatibility but not used
        });
        #endregion

        #region ToyotaApiConfig Configurations
        modelBuilder.Entity<ToyotaApiConfig>(entity =>
        {
            entity.HasIndex(e => new { e.Environment, e.IsActive });
            entity.HasIndex(e => e.Environment);
        });
        #endregion

        #region Seed Data - Admin User
        // Seed default admin user
        var adminUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var adminPasswordHash = ComputeSha256Hash("cisg1234");
        var seedDateTime = new DateTime(2025, 11, 25, 0, 0, 0, DateTimeKind.Local);

        modelBuilder.Entity<UserMaster>().HasData(
            new UserMaster
            {
                UserId = adminUserId,
                Username = "cisg",
                PasswordHash = adminPasswordHash,
                Name = "CISG",
                Role = "Admin",
                MenuLevel = "Admin",
                Operation = "Administration",
                Code = null,
                IsActive = true,
                IsSupervisor = true,
                CreatedAt = seedDateTime,
                UpdatedAt = null,
                CreatedBy = "System",
                UpdatedBy = null,
                LastLoginAt = null,
                Email = null,
                LocationId = null
            }
        );
        #endregion
    }

    /// <summary>
    /// Compute SHA256 hash for password and return Base64 encoded string
    /// </summary>
    private static string ComputeSha256Hash(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Override SaveChanges to automatically track audit fields
    /// </summary>
    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically track audit fields
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Automatically update CreatedAt, UpdatedAt, CreatedBy, and UpdatedBy fields
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.Now;
                // CreatedBy should be set by the service layer before saving
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.Now;
                // UpdatedBy should be set by the service layer before saving
            }
        }
    }
}
