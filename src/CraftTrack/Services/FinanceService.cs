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

    // ── Sales Report Methods ──

    public async Task<List<MonthlySalesData>> GetMonthlySalesDataAsync(int monthsBack = 6)
    {
        var result = new List<MonthlySalesData>();
        for (int i = monthsBack - 1; i >= 0; i--)
        {
            var month = DateTime.Today.AddMonths(-i);
            var from = new DateTime(month.Year, month.Month, 1);
            var to = from.AddMonths(1).AddDays(-1);

            var revenue = await GetTotalRevenueAsync(from, to);
            var expenses = await GetTotalExpensesAsync(from, to);

            result.Add(new MonthlySalesData
            {
                Month = from.ToString("MMM yyyy"),
                PeriodStart = from,
                Revenue = revenue,
                Expenses = expenses,
                Profit = revenue - expenses
            });
        }
        return result;
    }

    public async Task<List<MonthlySalesData>> GetSalesDataAsync(TimeGranularity granularity, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        var from = dateFrom ?? DateTime.Today.AddYears(-1);
        var to = dateTo ?? DateTime.Today;
        var result = new List<MonthlySalesData>();

        switch (granularity)
        {
            case TimeGranularity.Day:
                for (var d = from; d <= to; d = d.AddDays(1))
                {
                    var dayEnd = d.AddDays(1).AddTicks(-1);
                    result.Add(new MonthlySalesData
                    {
                        Month = d.ToString("MMM dd"),
                        PeriodStart = d,
                        Revenue = await GetTotalRevenueAsync(d, dayEnd),
                        Expenses = await GetTotalExpensesAsync(d, dayEnd),
                        Profit = await GetTotalRevenueAsync(d, dayEnd) - await GetTotalExpensesAsync(d, dayEnd)
                    });
                }
                break;

            case TimeGranularity.Week:
                // Start on Monday of the 'from' week
                var weekStart = from.AddDays(-(int)from.DayOfWeek + (int)DayOfWeek.Monday);
                if (weekStart > from) weekStart = weekStart.AddDays(-7);
                for (var w = weekStart; w <= to; w = w.AddDays(7))
                {
                    var weekEnd = w.AddDays(6);
                    var rev = await GetTotalRevenueAsync(w, weekEnd);
                    var exp = await GetTotalExpensesAsync(w, weekEnd);
                    result.Add(new MonthlySalesData
                    {
                        Month = $"{w:MMM dd}",
                        PeriodStart = w,
                        Revenue = rev,
                        Expenses = exp,
                        Profit = rev - exp
                    });
                }
                break;

            case TimeGranularity.Month:
                var mStart = new DateTime(from.Year, from.Month, 1);
                for (var m = mStart; m <= to; m = m.AddMonths(1))
                {
                    var mEnd = m.AddMonths(1).AddDays(-1);
                    var rev = await GetTotalRevenueAsync(m, mEnd);
                    var exp = await GetTotalExpensesAsync(m, mEnd);
                    result.Add(new MonthlySalesData
                    {
                        Month = m.ToString("MMM yyyy"),
                        PeriodStart = m,
                        Revenue = rev,
                        Expenses = exp,
                        Profit = rev - exp
                    });
                }
                break;

            case TimeGranularity.Year:
                for (var y = from.Year; y <= to.Year; y++)
                {
                    var yFrom = new DateTime(y, 1, 1);
                    var yTo = new DateTime(y, 12, 31);
                    var rev = await GetTotalRevenueAsync(yFrom, yTo);
                    var exp = await GetTotalExpensesAsync(yFrom, yTo);
                    result.Add(new MonthlySalesData
                    {
                        Month = y.ToString(),
                        PeriodStart = yFrom,
                        Revenue = rev,
                        Expenses = exp,
                        Profit = rev - exp
                    });
                }
                break;
        }

        return result;
    }

    public async Task<List<TopProductData>> GetTopSellingProductsAsync(DateTime? from = null, DateTime? to = null, int count = 10)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.OrderItems
            .Include(oi => oi.Order)
            .Include(oi => oi.Product)
            .Where(oi => oi.ProductId != null && oi.Order.Status != OrderStatus.Cancelled)
            .AsQueryable();

        if (from.HasValue) query = query.Where(oi => oi.Order.DateOrdered >= from.Value);
        if (to.HasValue) query = query.Where(oi => oi.Order.DateOrdered <= to.Value);

        return await query
            .GroupBy(oi => new { oi.ProductId, oi.Product!.Name })
            .Select(g => new TopProductData
            {
                ProductName = g.Key.Name,
                QuantitySold = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
            })
            .OrderByDescending(x => x.Revenue)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<CustomerRevenueData>> GetRevenueByCustomerAsync(DateTime? from = null, DateTime? to = null, int count = 10)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Orders
            .Include(o => o.Customer)
            .Where(o => o.Status != OrderStatus.Cancelled)
            .AsQueryable();

        if (from.HasValue) query = query.Where(o => o.DateOrdered >= from.Value);
        if (to.HasValue) query = query.Where(o => o.DateOrdered <= to.Value);

        return await query
            .GroupBy(o => new { o.CustomerId, o.Customer.FullName })
            .Select(g => new CustomerRevenueData
            {
                CustomerName = g.Key.FullName,
                Revenue = g.Sum(o => o.AmountPaid),
                OrderCount = g.Count()
            })
            .OrderByDescending(x => x.Revenue)
            .Take(count)
            .ToListAsync();
    }

    public async Task<Dictionary<OrderStatus, int>> GetOrderStatusBreakdownAsync(DateTime? from = null, DateTime? to = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Orders.AsQueryable();

        if (from.HasValue) query = query.Where(o => o.DateOrdered >= from.Value);
        if (to.HasValue) query = query.Where(o => o.DateOrdered <= to.Value);

        return await query
            .GroupBy(o => o.Status)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task<int> GetTotalOrderCountAsync(DateTime? from = null, DateTime? to = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Orders.Where(o => o.Status != OrderStatus.Cancelled).AsQueryable();

        if (from.HasValue) query = query.Where(o => o.DateOrdered >= from.Value);
        if (to.HasValue) query = query.Where(o => o.DateOrdered <= to.Value);

        return await query.CountAsync();
    }
}
