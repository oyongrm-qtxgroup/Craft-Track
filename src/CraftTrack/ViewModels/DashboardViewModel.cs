using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CraftTrack.Helpers;
using CraftTrack.Models;
using CraftTrack.Models.Enums;
using CraftTrack.Services;

namespace CraftTrack.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IOrderService _orderService;
    private readonly IInventoryService _inventoryService;
    private readonly IProductService _productService;
    private readonly IFinanceService _financeService;
    private readonly IRejectService _rejectService;
    private readonly ICustomerService _customerService;
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty] private bool _isLoading;

    // ── Period ──
    [ObservableProperty] private string _periodLabel = "This Month";
    [ObservableProperty] private DateTime? _dateFrom;
    [ObservableProperty] private DateTime? _dateTo;

    // ── Operational Alert Cards ──
    [ObservableProperty] private int _activeOrderCount;
    [ObservableProperty] private int _rushHighPriorityCount;
    [ObservableProperty] private int _overdueOrderCount;
    [ObservableProperty] private int _lowStockCount;

    // ── Financial KPI Cards ──
    [ObservableProperty] private decimal _totalRevenue;
    [ObservableProperty] private int _totalOrders;
    [ObservableProperty] private decimal _averageOrderValue;
    [ObservableProperty] private decimal _totalExpenses;
    [ObservableProperty] private decimal _netProfit;
    [ObservableProperty] private bool _isProfitPositive;
    [ObservableProperty] private decimal _outstandingBalance;
    [ObservableProperty] private decimal _rejectionLosses;
    [ObservableProperty] private int _rejectCount;
    [ObservableProperty] private int _productCount;

    // ── Revenue vs Expenses Period Comparison ──
    [ObservableProperty] private ObservableCollection<MonthlySalesData> _monthlyData = [];

    // ── Order Status Breakdown ──
    [ObservableProperty] private ObservableCollection<StatusBreakdownItem> _statusBreakdown = [];

    // ── Top Products + Customers ──
    [ObservableProperty] private ObservableCollection<TopProductData> _topProducts = [];
    [ObservableProperty] private ObservableCollection<CustomerRevenueData> _topCustomers = [];

    // ── Expense Category Breakdown ──
    [ObservableProperty] private ObservableCollection<ExpenseCategoryBreakdown> _expenseCategoryBreakdown = [];

    // ── Income Records ──
    [ObservableProperty] private ObservableCollection<Order> _incomeRecords = [];

    // ── Recent Orders ──
    [ObservableProperty] private ObservableCollection<Order> _recentOrders = [];

    // ── Upcoming Deadlines ──
    [ObservableProperty] private List<Order> _upcomingDeadlines = [];

    public DashboardViewModel(
        IOrderService orderService,
        IInventoryService inventoryService,
        IProductService productService,
        IFinanceService financeService,
        IRejectService rejectService,
        ICustomerService customerService,
        MainViewModel mainViewModel)
    {
        _orderService = orderService;
        _inventoryService = inventoryService;
        _productService = productService;
        _financeService = financeService;
        _rejectService = rejectService;
        _customerService = customerService;
        _mainViewModel = mainViewModel;

        SetPeriod("ThisMonth");
    }

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
        _ = LoadDashboardAsync();
    }

    [RelayCommand]
    private async Task LoadDashboardAsync()
    {
        IsLoading = true;
        try
        {
            // ── Operational alerts (always current, not period-filtered) ──
            ActiveOrderCount = await _orderService.GetActiveOrderCountAsync();
            RushHighPriorityCount = await _orderService.GetRushHighPriorityCountAsync();
            OverdueOrderCount = await _orderService.GetOverdueOrderCountAsync();
            LowStockCount = await _inventoryService.GetLowStockCountAsync();
            ProductCount = await _productService.GetProductCountAsync();

            // ── Financial KPIs (period-filtered) ──
            TotalRevenue = await _financeService.GetTotalRevenueAsync(DateFrom, DateTo);
            TotalExpenses = await _financeService.GetTotalExpensesAsync(DateFrom, DateTo);
            NetProfit = TotalRevenue - TotalExpenses;
            IsProfitPositive = NetProfit >= 0;
            TotalOrders = await _financeService.GetTotalOrderCountAsync(DateFrom, DateTo);
            AverageOrderValue = TotalOrders > 0 ? Math.Round(TotalRevenue / TotalOrders, 2) : 0;
            OutstandingBalance = await _financeService.GetTotalOutstandingBalanceAsync();

            // Rejection data
            RejectionLosses = await _rejectService.GetTotalRejectionCostAsync(DateFrom, DateTo);
            RejectCount = await _rejectService.GetRejectCountAsync(DateFrom, DateTo);

            // ── Monthly revenue vs expenses data ──
            var monthlyData = await _financeService.GetSalesDataAsync(TimeGranularity.Month, DateFrom, DateTo);
            MonthlyData = new ObservableCollection<MonthlySalesData>(monthlyData);

            // ── Order status breakdown ──
            var statusData = await _financeService.GetOrderStatusBreakdownAsync(DateFrom, DateTo);
            var statusItems = statusData.Select(kvp => new StatusBreakdownItem
            {
                Status = kvp.Key,
                Count = kvp.Value
            }).ToList();
            StatusBreakdown = new ObservableCollection<StatusBreakdownItem>(statusItems);

            // ── Top products ──
            var topProducts = await _financeService.GetTopSellingProductsAsync(DateFrom, DateTo, 8);
            TopProducts = new ObservableCollection<TopProductData>(topProducts);

            // ── Top customers ──
            var topCustomers = await _financeService.GetRevenueByCustomerAsync(DateFrom, DateTo, 8);
            TopCustomers = new ObservableCollection<CustomerRevenueData>(topCustomers);

            // ── Expense category breakdown ──
            var categoryData = await _financeService.GetExpensesByCategoryAsync(DateFrom, DateTo);
            var totalForPct = categoryData.Values.Sum();
            var breakdown = categoryData.Select(kvp => new ExpenseCategoryBreakdown
            {
                Category = kvp.Key,
                Amount = kvp.Value,
                Percentage = totalForPct > 0 ? (double)(kvp.Value / totalForPct * 100) : 0
            }).OrderByDescending(b => b.Amount).ToList();
            ExpenseCategoryBreakdown = new ObservableCollection<ExpenseCategoryBreakdown>(breakdown);

            // ── Income records ──
            var records = await _financeService.GetIncomeRecordsAsync(DateFrom, DateTo);
            IncomeRecords = new ObservableCollection<Order>(records);

            // ── Recent orders (latest 8) ──
            var allOrders = await _orderService.GetAllOrdersAsync();
            RecentOrders = new ObservableCollection<Order>(
                allOrders.OrderByDescending(o => o.CreatedAt).Take(8));

            // ── Upcoming deadlines ──
            UpcomingDeadlines = await _orderService.GetUpcomingDeadlinesAsync(7);
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Quick actions ──

    [RelayCommand]
    private void AddNewOrder() => _mainViewModel.NavigateToOrderDetail();

    [RelayCommand]
    private void AddNewProduct() => _mainViewModel.NavigateToProductDetail();

    [RelayCommand]
    private void AddInventoryItem() => _mainViewModel.NavigateToInventoryDetail();

    [RelayCommand]
    private void ViewOrder(Order order) => _mainViewModel.NavigateToOrderDetail(order.Id);
}

/// <summary>Dashboard DTO for top products.</summary>
public class TopProductItem
{
    public string ProductName { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public double Percentage { get; set; }
}

/// <summary>DTO for order status breakdown cards.</summary>
public class StatusBreakdownItem
{
    public OrderStatus Status { get; set; }
    public int Count { get; set; }

    public string StatusName => Status switch
    {
        OrderStatus.Active => "Active",
        OrderStatus.InProgress => "In Progress",
        OrderStatus.Completed => "Completed",
        OrderStatus.Cancelled => "Cancelled",
        _ => Status.ToString()
    };
}
