using CraftTrack.Data;
using CraftTrack.Models;
using CraftTrack.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CraftTrack.Services;

public class FinanceService : IFinanceService
{
    private readonly IDbContextFactory<CraftTrackDbContext> _contextFactory;

    public FinanceService(IDbContextFactory<CraftTrackDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Order>> GetIncomeRecordsAsync(DateTime? dateFrom = null, DateTime? dateTo = null, int? customerId = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Orders
            .Include(o => o.Customer)
            .Where(o => o.AmountPaid > 0)
            .AsQueryable();

        if (dateFrom.HasValue)
            query = query.Where(o => o.DateOrdered >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(o => o.DateOrdered <= dateTo.Value);

        if (customerId.HasValue)
            query = query.Where(o => o.CustomerId == customerId.Value);

        return await query.OrderByDescending(o => o.DateOrdered).ToListAsync();
    }

    public async Task<decimal> GetTotalRevenueAsync(DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Orders.AsQueryable();

        if (dateFrom.HasValue)
            query = query.Where(o => o.DateOrdered >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(o => o.DateOrdered <= dateTo.Value);

        return await query.SumAsync(o => o.AmountPaid);
    }

    public async Task<List<Expense>> GetExpensesAsync(DateTime? dateFrom = null, DateTime? dateTo = null, ExpenseCategory? category = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Expenses
            .Include(e => e.Order)
            .AsQueryable();

        if (dateFrom.HasValue)
            query = query.Where(e => e.Date >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(e => e.Date <= dateTo.Value);

        if (category.HasValue)
            query = query.Where(e => e.Category == category.Value);

        return await query.OrderByDescending(e => e.Date).ToListAsync();
    }

    public async Task<Expense?> GetExpenseByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Expenses
            .Include(e => e.Order)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Expense> CreateExpenseAsync(Expense expense)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        expense.CreatedAt = DateTime.Now;
        context.Expenses.Add(expense);
        await context.SaveChangesAsync();
        return expense;
    }

    public async Task<Expense> UpdateExpenseAsync(Expense expense)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Expenses.Update(expense);
        await context.SaveChangesAsync();
        return expense;
    }

    public async Task<bool> DeleteExpenseAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var expense = await context.Expenses.FindAsync(id);
        if (expense == null) return false;

        context.Expenses.Remove(expense);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> GetTotalExpensesAsync(DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Expenses.AsQueryable();

        if (dateFrom.HasValue)
            query = query.Where(e => e.Date >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(e => e.Date <= dateTo.Value);

        return await query.SumAsync(e => e.Amount);
    }

    public async Task<Dictionary<ExpenseCategory, decimal>> GetExpensesByCategoryAsync(DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Expenses.AsQueryable();

        if (dateFrom.HasValue)
            query = query.Where(e => e.Date >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(e => e.Date <= dateTo.Value);

        return await query
            .GroupBy(e => e.Category)
            .ToDictionaryAsync(g => g.Key, g => g.Sum(e => e.Amount));
    }

    public async Task<decimal> GetNetProfitAsync(DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var revenue = await GetTotalRevenueAsync(dateFrom, dateTo);
        var expenses = await GetTotalExpensesAsync(dateFrom, dateTo);
        return revenue - expenses;
    }

    public async Task<decimal> GetTotalOutstandingBalanceAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Orders
            .Where(o => o.Status != OrderStatus.Cancelled)
            .SumAsync(o => o.TotalPrice - o.AmountPaid);
    }
}
