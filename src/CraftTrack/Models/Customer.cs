using System.ComponentModel.DataAnnotations;

namespace CraftTrack.Models;

public class Customer
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public bool IsArchived { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
