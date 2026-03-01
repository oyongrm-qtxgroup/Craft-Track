using CraftTrack.Data;
using CraftTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace CraftTrack.Services;

public class CustomerService : ICustomerService
{
    private readonly IDbContextFactory<CraftTrackDbContext> _contextFactory;

    public CustomerService(IDbContextFactory<CraftTrackDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Customer>> GetAllCustomersAsync(bool includeArchived = false)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Customers.AsQueryable();
        if (!includeArchived)
            query = query.Where(c => !c.IsArchived);
        return await query.OrderBy(c => c.FullName).ToListAsync();
    }

    public async Task<Customer?> GetCustomerByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Customers.FindAsync(id);
    }

    public async Task<Customer?> GetCustomerWithOrdersAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Customers
            .Include(c => c.Orders)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Customer>> SearchCustomersAsync(string searchTerm)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Customers
            .Where(c => !c.IsArchived && c.FullName.Contains(searchTerm))
            .OrderBy(c => c.FullName)
            .Take(20)
            .ToListAsync();
    }

    public async Task<Customer> CreateCustomerAsync(Customer customer)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        customer.CreatedAt = DateTime.Now;
        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        return customer;
    }

    public async Task<Customer> UpdateCustomerAsync(Customer customer)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Customers.Update(customer);
        await context.SaveChangesAsync();
        return customer;
    }

    public async Task<bool> CanDeleteCustomerAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return !await context.Orders.AnyAsync(o => o.CustomerId == id);
    }

    public async Task<bool> ArchiveCustomerAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var customer = await context.Customers.FindAsync(id);
        if (customer == null) return false;
        customer.IsArchived = true;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        if (await context.Orders.AnyAsync(o => o.CustomerId == id))
            return false; // Cannot delete — has orders

        var customer = await context.Customers.FindAsync(id);
        if (customer == null) return false;

        context.Customers.Remove(customer);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<decimal> GetTotalRevenueForCustomerAsync(int customerId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Orders
            .Where(o => o.CustomerId == customerId)
            .SumAsync(o => o.AmountPaid);
    }

    public async Task<decimal> GetOutstandingBalanceForCustomerAsync(int customerId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Orders
            .Where(o => o.CustomerId == customerId && o.Status != Models.Enums.OrderStatus.Cancelled)
            .SumAsync(o => o.TotalPrice - o.AmountPaid);
    }
}
