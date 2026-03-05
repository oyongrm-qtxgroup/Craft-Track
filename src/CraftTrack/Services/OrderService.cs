using CraftTrack.Data;
using CraftTrack.Models;
using CraftTrack.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CraftTrack.Services;

public class OrderService : IOrderService
{
    private readonly IDbContextFactory<CraftTrackDbContext> _contextFactory;
    private readonly IInventoryService _inventoryService;
    private readonly INotificationService _notificationService;

    public OrderService(
        IDbContextFactory<CraftTrackDbContext> contextFactory,
        IInventoryService inventoryService,
        INotificationService notificationService)
    {
        _contextFactory = contextFactory;
        _inventoryService = inventoryService;
        _notificationService = notificationService;
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetFilteredOrdersAsync(
        SeverityLevel? severity = null,
        PaymentType? paymentType = null,
        OrderStatus? status = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string? searchTerm = null,
        DateTime? deadlineFrom = null,
        DateTime? deadlineTo = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .AsQueryable();

        if (severity.HasValue)
            query = query.Where(o => o.Severity == severity.Value);

        if (paymentType.HasValue)
            query = query.Where(o => o.PaymentType == paymentType.Value);

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        if (dateFrom.HasValue)
            query = query.Where(o => o.DateOrdered >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(o => o.DateOrdered <= dateTo.Value);

        if (deadlineFrom.HasValue)
            query = query.Where(o => o.DueDate >= deadlineFrom.Value);

        if (deadlineTo.HasValue)
            query = query.Where(o => o.DueDate <= deadlineTo.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(o => o.Customer.FullName.Contains(searchTerm)
                                  || o.Notes != null && o.Notes.Contains(searchTerm));

        return await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Orders
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order?> GetOrderWithDetailsAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Expenses)
            .Include(o => o.Payments.OrderByDescending(p => p.PaidAt))
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        order.CreatedAt = DateTime.Now;
        order.TotalPrice = order.OrderItems.Sum(i => i.Quantity * i.UnitPrice);

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Deduct inventory for each order item's product BOM
        await DeductStockForOrderItemsAsync(context, order.OrderItems.ToList(), order.Id);

        return order;
    }

    public async Task<Order> UpdateOrderAsync(Order order)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var existing = await context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == order.Id);

        if (existing == null)
            throw new InvalidOperationException($"Order {order.Id} not found.");

        // Restore stock for old items before replacing
        if (existing.Status != OrderStatus.Cancelled)
            await RestoreStockForOrderItemsAsync(context, existing.OrderItems.ToList(), existing.Id, "edit - reverted");

        // Update scalar properties
        existing.CustomerId = order.CustomerId;
        existing.DateOrdered = order.DateOrdered;
        existing.DueDate = order.DueDate;
        existing.Severity = order.Severity;
        existing.Status = order.Status;
        existing.PaymentType = order.PaymentType;
        existing.AmountPaid = order.AmountPaid;
        existing.Notes = order.Notes;

        // Replace order items
        context.OrderItems.RemoveRange(existing.OrderItems);
        foreach (var item in order.OrderItems)
        {
            item.Id = 0;
            item.OrderId = existing.Id;
            context.OrderItems.Add(item);
        }

        existing.TotalPrice = order.OrderItems.Sum(i => i.Quantity * i.UnitPrice);

        await context.SaveChangesAsync();

        // Deduct stock for new items (unless cancelling)
        if (order.Status != OrderStatus.Cancelled)
            await DeductStockForOrderItemsAsync(context, order.OrderItems.ToList(), existing.Id);

        return existing;
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var order = await context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return false;

        // Restore stock if order was not already cancelled
        if (order.Status != OrderStatus.Cancelled)
            await RestoreStockForOrderItemsAsync(context, order.OrderItems.ToList(), order.Id, "deleted");

        context.Orders.Remove(order);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelOrderAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var order = await context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return false;

        // Restore stock if not already cancelled
        if (order.Status != OrderStatus.Cancelled)
            await RestoreStockForOrderItemsAsync(context, order.OrderItems.ToList(), order.Id, "cancelled");

        order.Status = OrderStatus.Cancelled;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<Order> UpdatePaymentAsync(int orderId, decimal amountPaid, PaymentType paymentType)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var order = await context.Orders.FindAsync(orderId);
        if (order == null)
            throw new InvalidOperationException($"Order {orderId} not found.");

        order.AmountPaid = amountPaid;
        order.PaymentType = paymentType;

        // Auto-update payment type based on amount
        if (order.AmountPaid >= order.TotalPrice)
            order.PaymentType = PaymentType.FullPayment;
        else if (order.AmountPaid > 0)
            order.PaymentType = PaymentType.Downpayment;
        else
            order.PaymentType = PaymentType.Unpaid;

        await context.SaveChangesAsync();
        return order;
    }

    public async Task RecalculateOrderTotalAsync(int orderId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var order = await context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null) return;

        order.TotalPrice = order.OrderItems.Sum(i => i.Quantity * i.UnitPrice);
        await context.SaveChangesAsync();
    }

    // ── Payment History ──

    public async Task<Payment> AddPaymentAsync(int orderId, decimal amount, string? notes = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var order = await context.Orders.FindAsync(orderId);
        if (order == null)
            throw new InvalidOperationException($"Order {orderId} not found.");

        var payment = new Payment
        {
            OrderId = orderId,
            Amount = amount,
            PaidAt = DateTime.Now,
            Notes = notes
        };
        context.Payments.Add(payment);

        // Update running total
        order.AmountPaid += amount;

        // Auto-update payment type
        if (order.AmountPaid >= order.TotalPrice && order.TotalPrice > 0)
            order.PaymentType = PaymentType.FullPayment;
        else if (order.AmountPaid > 0)
            order.PaymentType = PaymentType.Downpayment;
        else
            order.PaymentType = PaymentType.Unpaid;

        await context.SaveChangesAsync();
        return payment;
    }

    public async Task<List<Payment>> GetPaymentsForOrderAsync(int orderId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Payments
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync();
    }

    public async Task<bool> DeletePaymentAsync(int paymentId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var payment = await context.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null) return false;

        var order = payment.Order;
        order.AmountPaid -= payment.Amount;
        if (order.AmountPaid < 0) order.AmountPaid = 0;

        // Auto-update payment type
        if (order.AmountPaid >= order.TotalPrice && order.TotalPrice > 0)
            order.PaymentType = PaymentType.FullPayment;
        else if (order.AmountPaid > 0)
            order.PaymentType = PaymentType.Downpayment;
        else
            order.PaymentType = PaymentType.Unpaid;

        context.Payments.Remove(payment);
        await context.SaveChangesAsync();
        return true;
    }

    // Dashboard queries

    public async Task<int> GetActiveOrderCountAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Orders
            .CountAsync(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled);
    }

    public async Task<int> GetRushHighPriorityCountAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Orders
            .CountAsync(o => (o.Severity == SeverityLevel.Rush || o.Severity == SeverityLevel.High)
                          && o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled);
    }

    public async Task<int> GetOverdueOrderCountAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var today = DateTime.Today;
        return await context.Orders
            .CountAsync(o => o.DueDate < today
                          && o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled);
    }

    public async Task<List<Order>> GetUpcomingDeadlinesAsync(int days = 7)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var today = DateTime.Today;
        var endDate = today.AddDays(days);
        return await context.Orders
            .Include(o => o.Customer)
            .Where(o => o.DueDate >= today && o.DueDate <= endDate
                     && o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled)
            .OrderBy(o => o.DueDate)
            .ToListAsync();
    }

    public async Task<decimal> GetMonthlyRevenueAsync(DateTime month)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var startOfMonth = new DateTime(month.Year, month.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);
        return await context.Orders
            .Where(o => o.DateOrdered >= startOfMonth && o.DateOrdered < endOfMonth)
            .SumAsync(o => o.AmountPaid);
    }

    public async Task<decimal> GetTotalUnpaidBalanceAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Orders
            .Where(o => o.Status != OrderStatus.Cancelled)
            .SumAsync(o => o.TotalPrice - o.AmountPaid);
    }

    // ── Inventory stock helpers ──

    private async Task DeductStockForOrderItemsAsync(CraftTrackDbContext context, List<OrderItem> items, int orderId)
    {
        foreach (var item in items)
        {
            if (item.ProductId is not > 0) continue;

            var product = await context.Products
                .Include(p => p.ProductMaterials)
                .FirstOrDefaultAsync(p => p.Id == item.ProductId.Value);

            if (product == null) continue;

            foreach (var mat in product.ProductMaterials)
            {
                var totalNeeded = mat.QuantityRequired * item.Quantity;
                var adjusted = await _inventoryService.AdjustStockAsync(
                    mat.InventoryItemId,
                    AdjustmentType.Deduct,
                    totalNeeded,
                    $"Order #{orderId} — {product.Name} ×{item.Quantity}");

                // Check low stock and notify
                if (adjusted.IsLowStock)
                    await _notificationService.GenerateLowStockNotificationAsync(adjusted);
            }
        }
    }

    private async Task RestoreStockForOrderItemsAsync(CraftTrackDbContext context, List<OrderItem> items, int orderId, string reason)
    {
        foreach (var item in items)
        {
            if (item.ProductId is not > 0) continue;

            var product = await context.Products
                .Include(p => p.ProductMaterials)
                .FirstOrDefaultAsync(p => p.Id == item.ProductId.Value);

            if (product == null) continue;

            foreach (var mat in product.ProductMaterials)
            {
                var totalToRestore = mat.QuantityRequired * item.Quantity;
                await _inventoryService.AdjustStockAsync(
                    mat.InventoryItemId,
                    AdjustmentType.Add,
                    totalToRestore,
                    $"Order #{orderId} {reason} — {product.Name} ×{item.Quantity}");
            }
        }
    }
}
