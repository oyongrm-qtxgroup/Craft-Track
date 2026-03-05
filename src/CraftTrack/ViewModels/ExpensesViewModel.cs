using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CraftTrack.Helpers;
using CraftTrack.Models;
using CraftTrack.Models.Enums;
using CraftTrack.Services;

namespace CraftTrack.ViewModels;

public partial class ExpensesViewModel : ObservableObject
{
    private readonly IFinanceService _financeService;
    private readonly MainViewModel _mainViewModel;

    // ── Period ──
    [ObservableProperty] private string _periodLabel = "This Month";
    [ObservableProperty] private DateTime? _dateFrom;
    [ObservableProperty] private DateTime? _dateTo;
    [ObservableProperty] private bool _isLoading;

    // ── Summary Cards ──
    [ObservableProperty] private decimal _totalExpenses;
    [ObservableProperty] private int _expenseCount;
    [ObservableProperty] private decimal _averageExpense;
    [ObservableProperty] private decimal _largestExpense;

    // ── Expense Category Breakdown ──
    [ObservableProperty] private ObservableCollection<ExpenseCategoryBreakdown> _expenseCategoryBreakdown = [];

    // ── Expense List ──
    [ObservableProperty] private ObservableCollection<Expense> _expenses = [];
    [ObservableProperty] private ExpenseCategory? _selectedExpenseCategory;

    // ── Expense Form ──
    [ObservableProperty] private bool _isExpenseFormOpen;
    [ObservableProperty] private bool _isEditingExpense;
    [ObservableProperty] private int _editingExpenseId;
    [ObservableProperty] private DateTime _expenseFormDate = DateTime.Today;
    [ObservableProperty] private ExpenseCategory _expenseFormCategory = ExpenseCategory.Materials;
    [ObservableProperty] private string _expenseFormDescription = string.Empty;
    [ObservableProperty] private decimal _expenseFormAmount;
    [ObservableProperty] private string _expenseFormError = string.Empty;

    public ExpensesViewModel(
        IFinanceService financeService,
        MainViewModel mainViewModel)
    {
        _financeService = financeService;
        _mainViewModel = mainViewModel;

        SetPeriod("ThisMonth");
    }

    public ExpenseCategory[] AllExpenseCategories => ExpenseCategoryHelper.AllCategories;

    // ── Period Shortcuts ──

    [RelayCommand]
    private void SetPeriod(string period)
    {
        var today = DateTime.Today;
        switch (period)
        {
            case "ThisMonth":
                DateFrom = new DateTime(today.Year, today.Month, 1);
                DateTo = today;
                PeriodLabel = "This Month";
                break;
            case "LastMonth":
                var lastMonth = today.AddMonths(-1);
                DateFrom = new DateTime(lastMonth.Year, lastMonth.Month, 1);
                DateTo = new DateTime(lastMonth.Year, lastMonth.Month, DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month));
                PeriodLabel = "Last Month";
                break;
            case "Last3Months":
                DateFrom = today.AddMonths(-3);
                DateTo = today;
                PeriodLabel = "Last 3 Months";
                break;
            case "Last6Months":
                DateFrom = today.AddMonths(-6);
                DateTo = today;
                PeriodLabel = "Last 6 Months";
                break;
            case "ThisYear":
                DateFrom = new DateTime(today.Year, 1, 1);
                DateTo = today;
                PeriodLabel = "This Year";
                break;
            case "AllTime":
                DateFrom = null;
                DateTo = null;
                PeriodLabel = "All Time";
                break;
        }
        _ = LoadDataAsync();
    }

    public async Task InitializeAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            // Summary
            TotalExpenses = await _financeService.GetTotalExpensesAsync(DateFrom, DateTo);

            // Expenses list
            var expenses = await _financeService.GetExpensesAsync(DateFrom, DateTo, SelectedExpenseCategory);
            Expenses = new ObservableCollection<Expense>(expenses);

            ExpenseCount = Expenses.Count;
            AverageExpense = ExpenseCount > 0 ? Math.Round(TotalExpenses / ExpenseCount, 2) : 0;
            LargestExpense = Expenses.Count > 0 ? Expenses.Max(e => e.Amount) : 0;

            // Category breakdown
            var categoryData = await _financeService.GetExpensesByCategoryAsync(DateFrom, DateTo);
            var totalForPct = categoryData.Values.Sum();
            var breakdown = categoryData.Select(kvp => new ExpenseCategoryBreakdown
            {
                Category = kvp.Key,
                Amount = kvp.Value,
                Percentage = totalForPct > 0 ? (double)(kvp.Value / totalForPct * 100) : 0
            }).OrderByDescending(b => b.Amount).ToList();
            ExpenseCategoryBreakdown = new ObservableCollection<ExpenseCategoryBreakdown>(breakdown);
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSelectedExpenseCategoryChanged(ExpenseCategory? value) => _ = LoadDataAsync();

    // ── Expense CRUD ──

    [RelayCommand]
    private void OpenAddExpense()
    {
        IsEditingExpense = false;
        EditingExpenseId = 0;
        ExpenseFormDate = DateTime.Today;
        ExpenseFormCategory = ExpenseCategory.Materials;
        ExpenseFormDescription = string.Empty;
        ExpenseFormAmount = 0;
        ExpenseFormError = string.Empty;
        IsExpenseFormOpen = true;
    }

    [RelayCommand]
    private async Task OpenEditExpense(Expense expense)
    {
        var loaded = await _financeService.GetExpenseByIdAsync(expense.Id);
        if (loaded == null) return;

        IsEditingExpense = true;
        EditingExpenseId = loaded.Id;
        ExpenseFormDate = loaded.Date;
        ExpenseFormCategory = loaded.Category;
        ExpenseFormDescription = loaded.Description ?? string.Empty;
        ExpenseFormAmount = loaded.Amount;
        ExpenseFormError = string.Empty;
        IsExpenseFormOpen = true;
    }

    [RelayCommand]
    private void CancelExpenseForm()
    {
        IsExpenseFormOpen = false;
    }

    [RelayCommand]
    private async Task SaveExpense()
    {
        ExpenseFormError = string.Empty;

        if (ExpenseFormAmount <= 0)
        {
            ExpenseFormError = "Amount must be greater than zero.";
            return;
        }

        if (IsEditingExpense)
        {
            var expense = await _financeService.GetExpenseByIdAsync(EditingExpenseId);
            if (expense == null) return;

            expense.Date = ExpenseFormDate;
            expense.Category = ExpenseFormCategory;
            expense.Description = string.IsNullOrWhiteSpace(ExpenseFormDescription) ? null : ExpenseFormDescription.Trim();
            expense.Amount = ExpenseFormAmount;
            await _financeService.UpdateExpenseAsync(expense);
        }
        else
        {
            var expense = new Expense
            {
                Date = ExpenseFormDate,
                Category = ExpenseFormCategory,
                Description = string.IsNullOrWhiteSpace(ExpenseFormDescription) ? null : ExpenseFormDescription.Trim(),
                Amount = ExpenseFormAmount
            };
            await _financeService.CreateExpenseAsync(expense);
        }

        IsExpenseFormOpen = false;
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task DeleteExpense(Expense expense)
    {
        await _financeService.DeleteExpenseAsync(expense.Id);
        await LoadDataAsync();
    }
}
