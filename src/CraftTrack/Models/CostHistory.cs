using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraftTrack.Models;

public class CostHistory
{
    public int Id { get; set; }

    public int InventoryItemId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OldCost { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal NewCost { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.Now;

    [MaxLength(500)]
    public string? Note { get; set; }

    // Navigation
    public InventoryItem InventoryItem { get; set; } = null!;
}
