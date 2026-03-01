using System.ComponentModel.DataAnnotations;
using CraftTrack.Models.Enums;

namespace CraftTrack.Models;

public class Notification
{
    public int Id { get; set; }

    [Required]
    public NotificationType Type { get; set; }

    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    public int? RelatedOrderId { get; set; }

    public int? RelatedInventoryItemId { get; set; }

    public bool IsAcknowledged { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public Order? RelatedOrder { get; set; }
    public InventoryItem? RelatedInventoryItem { get; set; }
}
