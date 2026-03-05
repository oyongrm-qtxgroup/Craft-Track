namespace CraftTrack.Models;

/// <summary>DTO for top-selling products chart/table.</summary>
public class TopProductData
{
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}
