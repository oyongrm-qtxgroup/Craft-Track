using CraftTrack.Models;

namespace CraftTrack.Services;

public interface INotificationService
{
    Task<List<Notification>> GetAllNotificationsAsync(bool includeAcknowledged = false);
    Task<int> GetUnacknowledgedCountAsync();
    Task AcknowledgeNotificationAsync(int notificationId);
    Task AcknowledgeAllNotificationsAsync();
    Task DeleteNotificationAsync(int notificationId);
    Task GenerateDeadlineNotificationsAsync();
    Task GenerateRushOrderNotificationAsync(Order order);
    Task GenerateLowStockNotificationAsync(InventoryItem item);
    Task GenerateRejectNotificationAsync(RejectRecord rejectRecord, string itemName);
    void ShowToastNotification(string title, string message, string? arguments = null);
}
