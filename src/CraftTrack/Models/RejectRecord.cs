using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CraftTrack.Models.Enums;

namespace CraftTrack.Models;

public class RejectRecord
{
    public int Id { get; set; }

    [Required]
    public DateTime Date { get; set; } = DateTime.Today;

    [Required]
    public RejectReason RejectReason { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public decimal Quantity { get; set; }

    /// <summary>Estimated cost of the rejection (quantity × unit cost of materials wasted).</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal EstimatedCost { get; set; }

    // Optional FK — for order-level rejects
    public int? OrderItemId { get; set; }

    // Optional FK — for inventory-level rejects
    public int? InventoryItemId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public OrderItem? OrderItem { get; set; }
    public InventoryItem? InventoryItem { get; set; }
}
