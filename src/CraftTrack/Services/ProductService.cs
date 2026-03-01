using CraftTrack.Data;
using CraftTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace CraftTrack.Services;

public class ProductService : IProductService
{
    private readonly IDbContextFactory<CraftTrackDbContext> _contextFactory;

    public ProductService(IDbContextFactory<CraftTrackDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Product>> GetAllProductsAsync(bool includeArchived = false)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.Products
            .Include(p => p.ProductMaterials)
                .ThenInclude(m => m.InventoryItem)
            .AsQueryable();

        if (!includeArchived)
            query = query.Where(p => !p.IsArchived);

        return await query.OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Products
            .Include(p => p.ProductMaterials)
                .ThenInclude(m => m.InventoryItem)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product?> GetProductWithMaterialsAsync(int id)
    {
        return await GetProductByIdAsync(id);
    }

    public async Task<List<Product>> SearchProductsAsync(string searchTerm)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Products
            .Include(p => p.ProductMaterials)
                .ThenInclude(m => m.InventoryItem)
            .Where(p => !p.IsArchived && p.Name.Contains(searchTerm))
            .OrderBy(p => p.Name)
            .Take(20)
            .ToListAsync();
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        product.CreatedAt = DateTime.Now;
        context.Products.Add(product);
        await context.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateProductAsync(Product product)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var existing = await context.Products
            .Include(p => p.ProductMaterials)
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        if (existing == null)
            throw new InvalidOperationException($"Product {product.Id} not found.");

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.ImagePath = product.ImagePath;
        existing.DefaultPrice = product.DefaultPrice;
        existing.LaborCost = product.LaborCost;
        existing.OverheadCost = product.OverheadCost;

        // Replace materials (delete + re-add)
        context.ProductMaterials.RemoveRange(existing.ProductMaterials);
        foreach (var material in product.ProductMaterials)
        {
            material.Id = 0;
            material.ProductId = existing.Id;
            context.ProductMaterials.Add(material);
        }

        await context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> ArchiveProductAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var product = await context.Products.FindAsync(id);
        if (product == null) return false;

        product.IsArchived = true;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CanDeleteProductAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return !await context.OrderItems.AnyAsync(oi => oi.ProductId == id);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        // Check if any orders reference this product
        if (await context.OrderItems.AnyAsync(oi => oi.ProductId == id))
            return false;

        var product = await context.Products
            .Include(p => p.ProductMaterials)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return false;

        context.Products.Remove(product);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<List<MaterialAvailability>> CheckMaterialAvailabilityAsync(int productId, int orderQuantity)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var product = await context.Products
            .Include(p => p.ProductMaterials)
                .ThenInclude(m => m.InventoryItem)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null) return [];

        return product.ProductMaterials.Select(m => new MaterialAvailability
        {
            InventoryItemId = m.InventoryItemId,
            InventoryItemName = m.InventoryItem.Name,
            UnitOfMeasure = m.InventoryItem.UnitOfMeasure,
            Required = m.QuantityRequired * orderQuantity,
            Available = m.InventoryItem.Quantity
        }).ToList();
    }

    public async Task<int> GetProductCountAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Products.CountAsync(p => !p.IsArchived);
    }
}
