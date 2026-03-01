using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CraftTrack.Models;
using CraftTrack.Services;

namespace CraftTrack.ViewModels;

public partial class CustomerProfileViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty] private int _customerId;
    [ObservableProperty] private string _fullName = string.Empty;
    [ObservableProperty] private string _phone = string.Empty;
    [ObservableProperty] private string _notes = string.Empty;
    [ObservableProperty] private decimal _totalRevenue;
    [ObservableProperty] private decimal _outstandingBalance;
    [ObservableProperty] private ObservableCollection<Order> _orders = [];
    [ObservableProperty] private bool _isEditing;

    public CustomerProfileViewModel(ICustomerService customerService, MainViewModel mainViewModel)
    {
        _customerService = customerService;
        _mainViewModel = mainViewModel;
    }

    public async Task LoadCustomerAsync(int customerId)
    {
        CustomerId = customerId;
        var customer = await _customerService.GetCustomerWithOrdersAsync(customerId);
        if (customer == null) return;

        FullName = customer.FullName;
        Phone = customer.Phone ?? string.Empty;
        Notes = customer.Notes ?? string.Empty;
        Orders = new ObservableCollection<Order>(customer.Orders);

        TotalRevenue = await _customerService.GetTotalRevenueForCustomerAsync(customerId);
        OutstandingBalance = await _customerService.GetOutstandingBalanceForCustomerAsync(customerId);
    }

    [RelayCommand]
    private void ToggleEdit()
    {
        IsEditing = !IsEditing;
    }

    [RelayCommand]
    private async Task SaveCustomer()
    {
        var customer = await _customerService.GetCustomerByIdAsync(CustomerId);
        if (customer == null) return;

        customer.FullName = FullName.Trim();
        customer.Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim();
        customer.Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim();

        await _customerService.UpdateCustomerAsync(customer);
        IsEditing = false;
    }

    [RelayCommand]
    private void ViewOrder(Order order)
    {
        _mainViewModel.NavigateToOrderDetail(order.Id);
    }

    [RelayCommand]
    private void GoBack()
    {
        _mainViewModel.NavigateTo("Customers");
    }
}
