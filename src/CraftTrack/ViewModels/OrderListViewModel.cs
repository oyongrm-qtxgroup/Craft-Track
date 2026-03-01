using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CraftTrack.Models;
using CraftTrack.Models.Enums;
using CraftTrack.Services;

namespace CraftTrack.ViewModels;

public partial class OrderListViewModel : ObservableObject
{
    private readonly IOrderService _orderService;
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty] private ObservableCollection<Order> _orders = [];
    [ObservableProperty] private SeverityLevel? _filterSeverity;
    [ObservableProperty] private PaymentType? _filterPaymentType;
    [ObservableProperty] private OrderStatus? _filterStatus;
    [ObservableProperty] private DateTime? _filterDateFrom;
    [ObservableProperty] private DateTime? _filterDateTo;
    [ObservableProperty] private DateTime? _filterDeadlineFrom;
    [ObservableProperty] private DateTime? _filterDeadlineTo;
    [ObservableProperty] private string _searchTerm = string.Empty;
    [ObservableProperty] private Order? _selectedOrder;

    public OrderListViewModel(IOrderService orderService, MainViewModel mainViewModel)
    {
        _orderService = orderService;
        _mainViewModel = mainViewModel;
        _ = LoadOrdersAsync();
    }

    [RelayCommand]
    private async Task LoadOrdersAsync()
    {
        var orders = await _orderService.GetFilteredOrdersAsync(
            FilterSeverity, FilterPaymentType, FilterStatus,
            FilterDateFrom, FilterDateTo, SearchTerm,
            FilterDeadlineFrom, FilterDeadlineTo);
        Orders = new ObservableCollection<Order>(orders);
    }

    [RelayCommand]
    private async Task ApplyFilters()
    {
        await LoadOrdersAsync();
    }

    [RelayCommand]
    private async Task ClearFilters()
    {
        FilterSeverity = null;
        FilterPaymentType = null;
        FilterStatus = null;
        FilterDateFrom = null;
        FilterDateTo = null;
        FilterDeadlineFrom = null;
        FilterDeadlineTo = null;
        SearchTerm = string.Empty;
        await LoadOrdersAsync();
    }

    [RelayCommand]
    private void AddOrder()
    {
        _mainViewModel.NavigateToOrderDetail();
    }

    [RelayCommand]
    private void EditOrder(Order order)
    {
        _mainViewModel.NavigateToOrderDetail(order.Id);
    }

    [RelayCommand]
    private async Task DeleteOrder(Order order)
    {
        var result = System.Windows.MessageBox.Show(
            $"Are you sure you want to delete Order #{order.Id}?",
            "Confirm Delete",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (result != System.Windows.MessageBoxResult.Yes)
            return;

        await _orderService.DeleteOrderAsync(order.Id);
        Orders.Remove(order);
    }

    partial void OnSearchTermChanged(string value)
    {
        _ = LoadOrdersAsync();
    }
}
