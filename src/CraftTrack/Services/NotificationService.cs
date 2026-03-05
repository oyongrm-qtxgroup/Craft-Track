using CraftTrack.Data;
using CraftTrack.Models;
using CraftTrack.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Toolkit.Uwp.Notifications;

namespace CraftTrack.Services;

public class NotificationService : INotificationService
{
    private readonly IDbContextFactory<CraftTrackDbContext> _contextFactory;

    public NotificationService(IDbContextFactory<CraftTrackDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Notification>> GetAllNotificationsAsync(bool includeAcknowledged = false)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Notifications
            .Include(n => n.RelatedOrder)
            .Include(n => n.RelatedInventoryItem)
            .AsQueryable();

        if (!includeAcknowledged)
            query = query.Where(n => !n.IsAcknowledged);

        return await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
    }

    public async Task<int> GetUnacknowledgedCountAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Notifications.CountAsync(n => !n.IsAcknowledged);
    }

    public async Task AcknowledgeNotificationAsync(int notificationId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var notification = await context.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            notification.IsAcknowledged = true;
            await context.SaveChangesAsync();
        }
    }

    public async Task AcknowledgeAllNotificationsAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var unacknowledged = await context.Notifications
            .Where(n => !n.IsAcknowledged)
            .ToListAsync();

        foreach (var n in unacknowledged)
            n.IsAcknowledged = true;

        await context.SaveChangesAsync();
    }

    public async Task DeleteNotificationAsync(int notificationId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var notification = await context.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            context.Notifications.Remove(notification);
            await context.SaveChangesAsync();
        }
    }

    public async Task GenerateDeadlineNotificationsAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var today = DateTime.Today;
        var in1Day = today.AddDays(1);
        var in3Days = today.AddDays(3);

        var activeOrders = await context.Orders
            .Include(o => o.Customer)
            .Where(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled)
            .ToListAsync();

        foreach (var order in activeOrders)
        {
            // Check overdue
            if (order.DueDate.Date < today)
            {
                await CreateNotificationIfNotExistsAsync(context, NotificationType.OrderOverdue, order.Id,
                    $"Order #{order.Id} for {order.Customer.FullName} is OVERDUE (was due {order.DueDate:d})");
            }
            // Due in 1 day
            else if (order.DueDate.Date == in1Day)
            {
                await CreateNotificationIfNotExistsAsync(context, NotificationType.OrderDueIn1Day, order.Id,
                    $"Order #{order.Id} for {order.Customer.FullName} is due TOMORROW ({order.DueDate:d})");
            }
            // Due in 3 days
            else if (order.DueDate.Date == in3Days)
            {
                await CreateNotificationIfNotExistsAsync(context, NotificationType.OrderDueIn3Days, order.Id,
                    $"Order #{order.Id} for {order.Customer.FullName} is due in 3 days ({order.DueDate:d})");
            }
        }

        await context.SaveChangesAsync();
    }

    public async Task GenerateRushOrderNotificationAsync(Order order)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var notification = new Notification
        {
            Type = NotificationType.RushOrderCreated,
            Message = $"RUSH Order #{order.Id} created for {order.Customer?.FullName ?? "Unknown"} — due {order.DueDate:d}",
            RelatedOrderId = order.Id,
            CreatedAt = DateTime.Now
        };
        context.Notifications.Add(notification);
        await context.SaveChangesAsync();

        ShowToastNotification("🔴 Rush Order Created", notification.Message, $"order:{order.Id}");
    }

    public async Task GenerateLowStockNotificationAsync(InventoryItem item)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var notification = new Notification
        {
            Type = NotificationType.LowStock,
            Message = $"Low stock alert: {item.Name} — {item.Quantity} {item.UnitOfMeasure} remaining (threshold: {item.LowStockThreshold})",
            RelatedInventoryItemId = item.Id,
            CreatedAt = DateTime.Now
        };
        context.Notifications.Add(notification);
        await context.SaveChangesAsync();

        ShowToastNotification("⚠️ Low Stock Alert", notification.Message, $"inventory:{item.Id}");
    }

    public async Task GenerateRejectNotificationAsync(RejectRecord rejectRecord, string itemName)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var notification = new Notification
        {
            Type = NotificationType.RejectCreated,
            Message = $"Rejected {rejectRecord.Quantity} × {itemName}: {rejectRecord.RejectReason}" +
                      (string.IsNullOrWhiteSpace(rejectRecord.Notes) ? "" : $" — {rejectRecord.Notes}"),
            RelatedInventoryItemId = rejectRecord.InventoryItemId,
            CreatedAt = DateTime.Now
        };
        context.Notifications.Add(notification);
        await context.SaveChangesAsync();

        ShowToastNotification("🚫 Item Rejected", notification.Message);
    }

    public void ShowToastNotification(string title, string message, string? arguments = null)
    {
        try
        {
            var builder = new ToastContentBuilder()
                .AddText(title)
                .AddText(message);

            if (!string.IsNullOrEmpty(arguments))
                builder.AddArgument("action", arguments);

            builder.Show();
        }
        catch
        {
            // Toast notifications may not be available in all environments
        }
    }

    private async Task CreateNotificationIfNotExistsAsync(
        CraftTrackDbContext context,
        NotificationType type,
        int orderId,
        string message)
    {
        var today = DateTime.Today;
        var exists = await context.Notifications.AnyAsync(n =>
            n.Type == type
            && n.RelatedOrderId == orderId
            && n.CreatedAt >= today);

        if (!exists)
        {
            var notification = new Notification
            {
                Type = type,
                Message = message,
                RelatedOrderId = orderId,
                CreatedAt = DateTime.Now
            };
            context.Notifications.Add(notification);

            // Also show a toast
            ShowToastNotification(
                type switch
                {
                    NotificationType.OrderOverdue => "⚠️ Order Overdue",
                    NotificationType.OrderDueIn1Day => "📅 Due Tomorrow",
                    NotificationType.OrderDueIn3Days => "📅 Due in 3 Days",
                    _ => "CraftTrack Alert"
                },
                message,
                $"order:{orderId}");
        }
    }
}
