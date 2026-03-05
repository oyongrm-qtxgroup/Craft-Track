using CraftTrack.Data;
using CraftTrack.Models;
using CraftTrack.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CraftTrack.Services;

public class RejectService : IRejectService
{
    private readonly IDbContextFactory<CraftTrackDbContext> _contextFactory;
    private readonly IInventoryService _inventoryService;

    public RejectService(IDbContextFactory<CraftTrackDbContext> contextFactory, IInventoryService inventoryService)
    {
        _contextFactory = contextFactory;
        _inventoryService = inventoryService;
    }

    public async Task<RejectRecord> RejectOrderItemAsync(int orderItemId, decimal quantity, RejectReason reason, string? notes = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var orderItem = await context.OrderItems
            .Include(oi => oi.Product)
                .ThenInclude(p => p!.ProductMaterials)
                    .ThenInclude(pm => pm.InventoryItem)
            .FirstOrDefaultAsync(oi => oi.Id == orderItemId)
            ?? throw new InvalidOperationException($"OrderItem {orderItemId} not found.");

        decimal estimatedCost = 0;

        // Deduct BOM materials for rejected quantity
        if (orderItem.Product?.ProductMaterials != null)
        {
            foreach (var material in orderItem.Product.ProductMaterials)
            {
                var deductQty = material.QuantityRequired * quantity;
                var materialCost = deductQty * material.InventoryItem.UnitCost;
                estimatedCost += materialCost;

                await _inventoryService.AdjustStockAsync(
                    material.InventoryItemId,
                    AdjustmentType.Deduct,
                    deductQty,
                    $"Reject: {reason} — Order Item #{orderItemId} ({orderItem.Product.Name}), qty {quantity}");
            }
        }

        // Also add labor + overhead cost portion
        if (orderItem.Product != null)
        {
            estimatedCost += (orderItem.Product.LaborCost + orderItem.Product.OverheadCost) * quantity;
        }

        var record = new RejectRecord
        {
            Date = DateTime.Today,
            RejectReason = reason,
            Notes = notes,
            Quantity = quantity,
            EstimatedCost = Math.Round(estimatedCost, 2),
            OrderItemId = orderItemId,
            InventoryItemId = null,
            CreatedAt = DateTime.Now
        };

        context.RejectRecords.Add(record);
        await context.SaveChangesAsync();
        return record;
    }

    public async Task<RejectRecord> RejectInventoryItemAsync(int inventoryItemId, decimal quantity, RejectReason reason, string? notes = null)
    {
        var item = await _inventoryService.AdjustStockAsync(
            inventoryItemId,
            AdjustmentType.Deduct,
            quantity,
            $"Reject: {reason}" + (string.IsNullOrWhiteSpace(notes) ? "" : $" — {notes}"));

        var record = new RejectRecord
        {
            Date = DateTime.Today,
            RejectReason = reason,
            Notes = notes,
            Quantity = quantity,
            EstimatedCost = Math.Round(quantity * item.UnitCost, 2),
            OrderItemId = null,
            InventoryItemId = inventoryItemId,
            CreatedAt = DateTime.Now
        };

        using var context = await _contextFactory.CreateDbContextAsync();
        context.RejectRecords.Add(record);
        await context.SaveChangesAsync();
        return record;
    }

    public async Task<List<RejectRecord>> GetRejectRecordsAsync(DateTime? from = null, DateTime? to = null, RejectReason? reason = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.RejectRecords
            .Include(r => r.OrderItem).ThenInclude(oi => oi!.Product)
            .Include(r => r.InventoryItem)
            .AsQueryable();

        if (from.HasValue) query = query.Where(r => r.Date >= from.Value);
        if (to.HasValue) query = query.Where(r => r.Date <= to.Value);
        if (reason.HasValue) query = query.Where(r => r.RejectReason == reason.Value);

        return await query.OrderByDescending(r => r.Date).ThenByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<List<RejectRecord>> GetRejectRecordsByOrderAsync(int orderId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.RejectRecords
            .Include(r => r.OrderItem).ThenInclude(oi => oi!.Product)
            .Where(r => r.OrderItem != null && r.OrderItem.OrderId == orderId)
            .OrderByDescending(r => r.Date)
            .ToListAsync();
    }

    public async Task<List<RejectRecord>> GetRejectRecordsByInventoryItemAsync(int inventoryItemId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.RejectRecords
            .Include(r => r.InventoryItem)
            .Where(r => r.InventoryItemId == inventoryItemId)
            .OrderByDescending(r => r.Date)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalRejectionCostAsync(DateTime? from = null, DateTime? to = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.RejectRecords.AsQueryable();

        if (from.HasValue) query = query.Where(r => r.Date >= from.Value);
        if (to.HasValue) query = query.Where(r => r.Date <= to.Value);

        return await query.SumAsync(r => r.EstimatedCost);
    }

    public async Task<int> GetRejectCountAsync(DateTime? from = null, DateTime? to = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.RejectRecords.AsQueryable();

        if (from.HasValue) query = query.Where(r => r.Date >= from.Value);
        if (to.HasValue) query = query.Where(r => r.Date <= to.Value);

        return await query.CountAsync();
    }
}
