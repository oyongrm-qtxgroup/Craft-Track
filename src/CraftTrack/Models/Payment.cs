using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CraftTrack.Models;

/// <summary>
/// Represents a single payment transaction against an order.
/// Orders accumulate payments over time; AmountPaid is the running total.
/// </summary>
public class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    [Required]
    public DateTime PaidAt { get; set; } = DateTime.Now;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation
    public Order Order { get; set; } = null!;
}
