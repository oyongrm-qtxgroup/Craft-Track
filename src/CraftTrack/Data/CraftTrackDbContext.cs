using System.IO;
using CraftTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace CraftTrack.Data;

public class CraftTrackDbContext : DbContext
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductMaterial> ProductMaterials => Set<ProductMaterial>();
    public DbSet<RejectRecord> RejectRecords => Set<RejectRecord>();
    public DbSet<Payment> Payments => Set<Payment>();

    public CraftTrackDbContext(DbContextOptions<CraftTrackDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Customer
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.FullName);
            entity.HasIndex(e => e.IsArchived);
        });

        // Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.Orders)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.DueDate);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.CustomerId);

            entity.Ignore(e => e.BalanceDue);
        });

        // OrderItem
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                  .WithMany(p => p.OrderItems)
                  .HasForeignKey(e => e.ProductId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Ignore(e => e.LineTotal);
        });

        // Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsArchived);
            entity.Ignore(e => e.TotalCost);
        });

        // ProductMaterial (BOM)
        modelBuilder.Entity<ProductMaterial>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Product)
                  .WithMany(p => p.ProductMaterials)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.InventoryItem)
                  .WithMany(i => i.ProductMaterials)
                  .HasForeignKey(e => e.InventoryItemId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.ProductId, e.InventoryItemId }).IsUnique();
        });

        // InventoryItem
        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Category);
            entity.Ignore(e => e.IsLowStock);
        });

        // StockAdjustment
        modelBuilder.Entity<StockAdjustment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.InventoryItem)
                  .WithMany(i => i.StockAdjustments)
                  .HasForeignKey(e => e.InventoryItemId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.InventoryItemId);
        });

        // Expense
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Order)
                  .WithMany(o => o.Expenses)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => e.Category);
        });

        // RejectRecord
        modelBuilder.Entity<RejectRecord>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.OrderItem)
                  .WithMany()
                  .HasForeignKey(e => e.OrderItemId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.InventoryItem)
                  .WithMany()
                  .HasForeignKey(e => e.InventoryItemId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.Date);
            entity.HasIndex(e => e.RejectReason);
            entity.HasIndex(e => e.InventoryItemId);
            entity.HasIndex(e => e.OrderItemId);
        });

        // Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Order)
                  .WithMany(o => o.Payments)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.PaidAt);
        });

        // Notification
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.RelatedOrder)
                  .WithMany()
                  .HasForeignKey(e => e.RelatedOrderId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.RelatedInventoryItem)
                  .WithMany()
                  .HasForeignKey(e => e.RelatedInventoryItemId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.IsAcknowledged);
            entity.HasIndex(e => e.CreatedAt);
        });
    }

    public static string GetDatabasePath()
    {
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var craftTrackFolder = Path.Combine(documentsPath, "CraftTrack");
        Directory.CreateDirectory(craftTrackFolder);
        return Path.Combine(craftTrackFolder, "crafttrack.db");
    }
}
