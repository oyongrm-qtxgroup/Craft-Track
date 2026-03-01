using CraftTrack.Models;
using CraftTrack.Models.Enums;

namespace CraftTrack.Services;

public interface IFinanceService
{
    // Income (derived from order payments)
    Task<List<Order>> GetIncomeRecordsAsync(DateTime? dateFrom = null, DateTime? dateTo = null, int? customerId = null);
    Task<decimal> GetTotalRevenueAsync(DateTime? dateFrom = null, DateTime? dateTo = null);

    // Expenses
    Task<List<Expense>> GetExpensesAsync(DateTime? dateFrom = null, DateTime? dateTo = null, ExpenseCategory? category = null);
    Task<Expense?> GetExpenseByIdAsync(int id);
    Task<Expense> CreateExpenseAsync(Expense expense);
    Task<Expense> UpdateExpenseAsync(Expense expense);
    Task<bool> DeleteExpenseAsync(int id);
    Task<decimal> GetTotalExpensesAsync(DateTime? dateFrom = null, DateTime? dateTo = null);

    // P&L
    Task<Dictionary<ExpenseCategory, decimal>> GetExpensesByCategoryAsync(DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<decimal> GetNetProfitAsync(DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<decimal> GetTotalOutstandingBalanceAsync();
}
