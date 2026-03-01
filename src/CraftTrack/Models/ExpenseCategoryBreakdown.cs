using CraftTrack.Models.Enums;

namespace CraftTrack.Models;

public class ExpenseCategoryBreakdown
{
    public ExpenseCategory Category { get; set; }
    public decimal Amount { get; set; }
    public double Percentage { get; set; }

    public string CategoryName => Category switch
    {
        ExpenseCategory.Materials => "Materials",
        ExpenseCategory.Tools => "Tools",
        ExpenseCategory.Packaging => "Packaging",
        ExpenseCategory.Utilities => "Utilities",
        ExpenseCategory.Other => "Other",
        _ => Category.ToString()
    };

    public string CategoryIcon => Category switch
    {
        ExpenseCategory.Materials => "Palette",
        ExpenseCategory.Tools => "Hammer",
        ExpenseCategory.Packaging => "PackageVariant",
        ExpenseCategory.Utilities => "LightningBolt",
        ExpenseCategory.Other => "DotsHorizontal",
        _ => "Circle"
    };
}
