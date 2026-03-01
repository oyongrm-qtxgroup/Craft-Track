using CraftTrack.Models;

namespace CraftTrack.Services;

public interface IProductService
{
    Task<List<Product>> GetAllProductsAsync(bool includeArchived = false);
    Task<Product?> GetProductByIdAsync(int id);
    Task<Product?> GetProductWithMaterialsAsync(int id);
    Task<List<Product>> SearchProductsAsync(string searchTerm);
    Task<Product> CreateProductAsync(Product product);
    Task<Product> UpdateProductAsync(Product product);
    Task<bool> ArchiveProductAsync(int id);
    Task<bool> CanDeleteProductAsync(int id);
    Task<bool> DeleteProductAsync(int id);
    Task<List<MaterialAvailability>> CheckMaterialAvailabilityAsync(int productId, int orderQuantity);
    Task<int> GetProductCountAsync();
}

public class MaterialAvailability
{
    public int InventoryItemId { get; set; }
    public string InventoryItemName { get; set; } = string.Empty;
    public string? UnitOfMeasure { get; set; }
    public decimal Required { get; set; }
    public decimal Available { get; set; }
    public bool IsSufficient => Available >= Required;
}
