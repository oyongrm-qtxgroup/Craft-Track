namespace CraftTrack.Models;

/// <summary>DTO for revenue/expense/profit chart data at any time granularity.</summary>
public class MonthlySalesData
{
    /// <summary>Display label for the data point (e.g. "Mar 2026", "Week 10", "Mon Mar 02").</summary>
    public string Month { get; set; } = string.Empty;

    /// <summary>The date this data point represents (start of period).</summary>
    public DateTime PeriodStart { get; set; }

    public decimal Revenue { get; set; }
    public decimal Expenses { get; set; }
    public decimal Profit { get; set; }
}
