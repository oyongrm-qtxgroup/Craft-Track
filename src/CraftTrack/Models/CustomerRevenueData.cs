namespace CraftTrack.Models;

/// <summary>DTO for revenue-by-customer chart/table.</summary>
public class CustomerRevenueData
{
    public string CustomerName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}
