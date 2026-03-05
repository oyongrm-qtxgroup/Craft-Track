using CraftTrack.Models;
using CraftTrack.Models.Enums;

namespace CraftTrack.Services;

public interface IRejectService
{
    /// <summary>Reject items from an order item, auto-deducting BOM materials from inventory.</summary>
    Task<RejectRecord> RejectOrderItemAsync(int orderItemId, decimal quantity, RejectReason reason, string? notes = null);

    /// <summary>Reject raw materials directly from inventory stock.</summary>
    Task<RejectRecord> RejectInventoryItemAsync(int inventoryItemId, decimal quantity, RejectReason reason, string? notes = null);

    Task<List<RejectRecord>> GetRejectRecordsAsync(DateTime? from = null, DateTime? to = null, RejectReason? reason = null);
    Task<List<RejectRecord>> GetRejectRecordsByOrderAsync(int orderId);
    Task<List<RejectRecord>> GetRejectRecordsByInventoryItemAsync(int inventoryItemId);
    Task<decimal> GetTotalRejectionCostAsync(DateTime? from = null, DateTime? to = null);
    Task<int> GetRejectCountAsync(DateTime? from = null, DateTime? to = null);
}
