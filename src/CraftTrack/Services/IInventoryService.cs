using CraftTrack.Models;
using CraftTrack.Models.Enums;

namespace CraftTrack.Services;

public interface IInventoryService
{
    Task<List<InventoryItem>> GetAllItemsAsync(InventoryCategory? category = null);
    Task<InventoryItem?> GetItemByIdAsync(int id);
    Task<InventoryItem?> GetItemWithAdjustmentsAsync(int id);
    Task<InventoryItem> CreateItemAsync(InventoryItem item);
    Task<InventoryItem> UpdateItemAsync(InventoryItem item);
    Task<bool> DeleteItemAsync(int id);
    Task<InventoryItem> AdjustStockAsync(int itemId, AdjustmentType type, decimal quantity, string? note = null);
    Task<List<InventoryItem>> GetLowStockItemsAsync();
    Task<int> GetLowStockCountAsync();
    Task<List<StockAdjustment>> GetAdjustmentHistoryAsync(int itemId);
}
