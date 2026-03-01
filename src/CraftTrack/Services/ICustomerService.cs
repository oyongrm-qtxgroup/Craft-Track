using CraftTrack.Models;

namespace CraftTrack.Services;

public interface ICustomerService
{
    Task<List<Customer>> GetAllCustomersAsync(bool includeArchived = false);
    Task<Customer?> GetCustomerByIdAsync(int id);
    Task<Customer?> GetCustomerWithOrdersAsync(int id);
    Task<List<Customer>> SearchCustomersAsync(string searchTerm);
    Task<Customer> CreateCustomerAsync(Customer customer);
    Task<Customer> UpdateCustomerAsync(Customer customer);
    Task<bool> ArchiveCustomerAsync(int id);
    Task<bool> CanDeleteCustomerAsync(int id);
    Task<bool> DeleteCustomerAsync(int id);
    Task<decimal> GetTotalRevenueForCustomerAsync(int customerId);
    Task<decimal> GetOutstandingBalanceForCustomerAsync(int customerId);
}
