using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CraftTrack.Models.Enums;

namespace CraftTrack.Models;

public class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    [Required]
    public DateTime DateOrdered { get; set; } = DateTime.Today;

    [Required]
    public DateTime DueDate { get; set; }

    [Required]
    public SeverityLevel Severity { get; set; } = SeverityLevel.Low;

    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Active;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountPaid { get; set; }

    public DiscountType DiscountType { get; set; } = DiscountType.None;

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountValue { get; set; }

    [NotMapped]
    public decimal BalanceDue => TotalPrice - AmountPaid;

    [Required]
    public PaymentType PaymentType { get; set; } = PaymentType.Unpaid;

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation
    public Customer Customer { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
