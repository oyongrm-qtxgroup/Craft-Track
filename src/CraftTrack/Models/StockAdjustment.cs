using System.ComponentModel.DataAnnotations;
using CraftTrack.Models.Enums;

namespace CraftTrack.Models;

public class StockAdjustment
{
    public int Id { get; set; }

    public int InventoryItemId { get; set; }

    [Required]
    public AdjustmentType AdjustmentType { get; set; }

    public decimal QuantityChange { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    public DateTime AdjustedAt { get; set; } = DateTime.Now;

    // Navigation
    public InventoryItem InventoryItem { get; set; } = null!;
}
