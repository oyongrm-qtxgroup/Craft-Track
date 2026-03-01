namespace CraftTrack.Models.Enums;

public static class ExpenseCategoryHelper
{
    public static ExpenseCategory[] AllCategories { get; } =
        Enum.GetValues<ExpenseCategory>();
}
