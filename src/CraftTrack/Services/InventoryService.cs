using CraftTrack.Data;
using CraftTrack.Models;
using CraftTrack.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CraftTrack.Services;

public class InventoryService : IInventoryService
{
    private readonly IDbContextFactory<CraftTrackDbContext> _contextFactory;

    public InventoryService(IDbContextFactory<CraftTrackDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<InventoryItem>> GetAllItemsAsync(InventoryCategory? category = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.InventoryItems.AsQueryable();

        if (category.HasValue)
            query = query.Where(i => i.Category == category.Value);

        return await query.OrderBy(i => i.Name).ToListAsync();
    }

    public async Task<InventoryItem?> GetItemByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.InventoryItems.FindAsync(id);
    }

    public async Task<InventoryItem?> GetItemWithAdjustmentsAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.InventoryItems
            .Include(i => i.StockAdjustments.OrderByDescending(s => s.AdjustedAt))
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<InventoryItem> CreateItemAsync(InventoryItem item)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        item.CreatedAt = DateTime.Now;
        context.InventoryItems.Add(item);
        await context.SaveChangesAsync();
        return item;
    }

    public async Task<InventoryItem> UpdateItemAsync(InventoryItem item)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // Auto-record cost history if unit cost changed
        var existing = await context.InventoryItems.AsNoTracking().FirstOrDefaultAsync(i => i.Id == item.Id);
        if (existing != null && existing.UnitCost != item.UnitCost)
        {
            context.CostHistories.Add(new CostHistory
            {
                InventoryItemId = item.Id,
                OldCost = existing.UnitCost,
                NewCost = item.UnitCost,
                ChangedAt = DateTime.Now
            });
        }

        context.InventoryItems.Update(item);
        await context.SaveChangesAsync();
        return item;
    }

    public async Task<bool> DeleteItemAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var item = await context.InventoryItems.FindAsync(id);
        if (item == null) return false;

        context.InventoryItems.Remove(item);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<InventoryItem> AdjustStockAsync(int itemId, AdjustmentType type, decimal quantity, string? note = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var item = await context.InventoryItems.FindAsync(itemId);
        if (item == null)
            throw new InvalidOperationException($"Inventory item {itemId} not found.");

        // Apply adjustment
        if (type == AdjustmentType.Add)
            item.Quantity += quantity;
        else
            item.Quantity -= quantity;

        // Prevent negative stock
        if (item.Quantity < 0)
            item.Quantity = 0;

        // Record adjustment
        var adjustment = new StockAdjustment
        {
            InventoryItemId = itemId,
            AdjustmentType = type,
            QuantityChange = quantity,
            Note = note,
            AdjustedAt = DateTime.Now
        };
        context.StockAdjustments.Add(adjustment);

        await context.SaveChangesAsync();
        return item;
    }

    public async Task<List<InventoryItem>> GetLowStockItemsAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.InventoryItems
            .Where(i => i.Quantity <= i.LowStockThreshold)
            .OrderBy(i => i.Name)
            .ToListAsync();
    }

    public async Task<int> GetLowStockCountAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.InventoryItems
            .CountAsync(i => i.Quantity <= i.LowStockThreshold);
    }

    public async Task<List<StockAdjustment>> GetAdjustmentHistoryAsync(int itemId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.StockAdjustments
            .Where(a => a.InventoryItemId == itemId)
            .OrderByDescending(a => a.AdjustedAt)
            .ToListAsync();
    }

    public async Task<List<CostHistory>> GetCostHistoryAsync(int itemId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.CostHistories
            .Where(c => c.InventoryItemId == itemId)
            .OrderByDescending(c => c.ChangedAt)
            .ToListAsync();
    }
}
