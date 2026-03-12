using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CraftTrack.Models.Enums;

namespace CraftTrack.Models;

public class InventoryItem
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Variant { get; set; }

    [MaxLength(100)]
    public string? Size { get; set; }

    [Required]
    public InventoryCategory Category { get; set; }

    [MaxLength(50)]
    public string? UnitOfMeasure { get; set; }

    public decimal Quantity { get; set; }

    public decimal LowStockThreshold { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitCost { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public ICollection<StockAdjustment> StockAdjustments { get; set; } = new List<StockAdjustment>();
    public ICollection<ProductMaterial> ProductMaterials { get; set; } = new List<ProductMaterial>();
    public ICollection<CostHistory> CostHistories { get; set; } = new List<CostHistory>();

    // Computed
    [NotMapped]
    public bool IsLowStock => Quantity <= LowStockThreshold;
}
