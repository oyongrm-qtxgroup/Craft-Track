using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CraftTrack.Models;
using CraftTrack.Models.Enums;
using CraftTrack.Services;

namespace CraftTrack.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IOrderService _orderService;
    private readonly IInventoryService _inventoryService;
    private readonly IProductService _productService;
    private readonly MainViewModel _mainViewModel;

    // ── Stat cards ──
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private int _activeOrderCount;
    [ObservableProperty] private int _rushHighPriorityCount;
    [ObservableProperty] private int _overdueOrderCount;
    [ObservableProperty] private int _lowStockCount;

    // ── Financial summary ──
    [ObservableProperty] private decimal _monthlyRevenue;
    [ObservableProperty] private int _ordersThisMonth;
    [ObservableProperty] private int _productCount;
    [ObservableProperty] private decimal _unpaidBalance;

    // ── Recent orders ──
    [ObservableProperty] private ObservableCollection<Order> _recentOrders = [];

    // ── Top products ──
    [ObservableProperty] private ObservableCollection<TopProductItem> _topProducts = [];

    // ── Upcoming deadlines ──
    [ObservableProperty] private List<Order> _upcomingDeadlines = [];

    public DashboardViewModel(
        IOrderService orderService,
        IInventoryService inventoryService,
        IProductService productService,
        MainViewModel mainViewModel)
    {
        _orderService = orderService;
        _inventoryService = inventoryService;
        _productService = productService;
        _mainViewModel = mainViewModel;

        _ = LoadDashboardAsync();
    }

    [RelayCommand]
    private async Task LoadDashboardAsync()
    {
        IsLoading = true;
        try
        {
            // Current month range
            var now = DateTime.Today;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            // Stat cards
            ActiveOrderCount = await _orderService.GetActiveOrderCountAsync();
            RushHighPriorityCount = await _orderService.GetRushHighPriorityCountAsync();
            OverdueOrderCount = await _orderService.GetOverdueOrderCountAsync();
            LowStockCount = await _inventoryService.GetLowStockCountAsync();

            // Financial summary
            MonthlyRevenue = await _orderService.GetMonthlyRevenueAsync(now);
            UnpaidBalance = await _orderService.GetTotalUnpaidBalanceAsync();
            ProductCount = await _productService.GetProductCountAsync();

            // Orders this month
            var monthOrders = await _orderService.GetFilteredOrdersAsync(
                dateFrom: monthStart, dateTo: monthEnd);
            OrdersThisMonth = monthOrders.Count;

            // Recent orders (last 8)
            var allOrders = await _orderService.GetAllOrdersAsync();
            RecentOrders = new ObservableCollection<Order>(
                allOrders.OrderByDescending(o => o.CreatedAt).Take(8));

            // Top products (most ordered)
            var topProducts = allOrders
                .SelectMany(o => o.OrderItems)
                .Where(oi => oi.Product != null)
                .GroupBy(oi => new { oi.ProductId, oi.Product!.Name, oi.Product.ImagePath })
                .Select(g => new TopProductItem
                {
                    ProductName = g.Key.Name,
                    ImagePath = g.Key.ImagePath,
                    OrderCount = g.Count(),
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(p => p.OrderCount)
                .Take(5)
                .ToList();

            var maxRevenue = topProducts.Select(p => (double)p.TotalRevenue).DefaultIfEmpty(0).Max();
            foreach (var p in topProducts)
                p.Percentage = maxRevenue > 0 ? (double)p.TotalRevenue / maxRevenue * 100.0 : 0;

            TopProducts = new ObservableCollection<TopProductItem>(topProducts);

            // Upcoming deadlines
            UpcomingDeadlines = await _orderService.GetUpcomingDeadlinesAsync(7);
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Quick actions ──

    [RelayCommand]
    private void AddNewOrder()
    {
        _mainViewModel.NavigateToOrderDetail();
    }

    [RelayCommand]
    private void AddNewProduct()
    {
        _mainViewModel.NavigateToProductDetail();
    }

    [RelayCommand]
    private void AddInventoryItem()
    {
        _mainViewModel.NavigateToInventoryDetail();
    }

    [RelayCommand]
    private void ViewOrder(Order order)
    {
        _mainViewModel.NavigateToOrderDetail(order.Id);
    }
}

/// <summary>Dashboard DTO for top products chart.</summary>
public class TopProductItem
{
    public string ProductName { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public double Percentage { get; set; }
}
