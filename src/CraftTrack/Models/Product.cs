using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraftTrack.Models;

public class Product
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? ImagePath { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DefaultPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal LaborCost { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OverheadCost { get; set; }

    public bool IsArchived { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public ICollection<ProductMaterial> ProductMaterials { get; set; } = new List<ProductMaterial>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    // Computed
    [NotMapped]
    public decimal TotalCost => LaborCost + OverheadCost + ProductMaterials.Sum(m => m.QuantityRequired * (m.InventoryItem?.UnitCost ?? 0));
}
