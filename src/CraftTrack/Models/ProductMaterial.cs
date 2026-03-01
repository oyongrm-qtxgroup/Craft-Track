namespace CraftTrack.Models;

public class ProductMaterial
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int InventoryItemId { get; set; }

    public decimal QuantityRequired { get; set; }

    // Navigation
    public Product Product { get; set; } = null!;
    public InventoryItem InventoryItem { get; set; } = null!;
}
