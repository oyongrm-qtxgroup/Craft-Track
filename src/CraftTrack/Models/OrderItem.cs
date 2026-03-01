using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraftTrack.Models;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int? ProductId { get; set; }

    // Legacy fields — kept nullable for backward-compat with pre-catalog orders
    [MaxLength(200)]
    public string? ItemType { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public int Quantity { get; set; } = 1;

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [NotMapped]
    public decimal LineTotal => Quantity * UnitPrice;

    // Navigation
    public Order Order { get; set; } = null!;
    public Product? Product { get; set; }
}
