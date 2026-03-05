using CraftTrack.Models;
using CraftTrack.Models.Enums;

namespace CraftTrack.Services;

public interface IOrderService
{
    Task<List<Order>> GetAllOrdersAsync();
    Task<List<Order>> GetFilteredOrdersAsync(
        SeverityLevel? severity = null,
        PaymentType? paymentType = null,
        OrderStatus? status = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        string? searchTerm = null,
        DateTime? deadlineFrom = null,
        DateTime? deadlineTo = null);
    Task<Order?> GetOrderByIdAsync(int id);
    Task<Order?> GetOrderWithDetailsAsync(int id);
    Task<Order> CreateOrderAsync(Order order);
    Task<Order> UpdateOrderAsync(Order order);
    Task<bool> DeleteOrderAsync(int id);
    Task<bool> CancelOrderAsync(int id);
    Task<Order> UpdatePaymentAsync(int orderId, decimal amountPaid, PaymentType paymentType);
    Task RecalculateOrderTotalAsync(int orderId);

    // Payment history
    Task<Payment> AddPaymentAsync(int orderId, decimal amount, string? notes = null);
    Task<List<Payment>> GetPaymentsForOrderAsync(int orderId);
    Task<bool> DeletePaymentAsync(int paymentId);

    // Dashboard queries
    Task<int> GetActiveOrderCountAsync();
    Task<int> GetRushHighPriorityCountAsync();
    Task<int> GetOverdueOrderCountAsync();
    Task<List<Order>> GetUpcomingDeadlinesAsync(int days = 7);
    Task<decimal> GetMonthlyRevenueAsync(DateTime month);
    Task<decimal> GetTotalUnpaidBalanceAsync();
}
