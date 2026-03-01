using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CraftTrack.Models;
using CraftTrack.Services;

namespace CraftTrack.ViewModels;

public partial class CustomerListViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty] private ObservableCollection<Customer> _customers = [];
    [ObservableProperty] private string _searchTerm = string.Empty;
    [ObservableProperty] private Customer? _selectedCustomer;

    public CustomerListViewModel(ICustomerService customerService, MainViewModel mainViewModel)
    {
        _customerService = customerService;
        _mainViewModel = mainViewModel;
        _ = LoadCustomersAsync();
    }

    [RelayCommand]
    private async Task LoadCustomersAsync()
    {
        List<Customer> customers;
        if (!string.IsNullOrWhiteSpace(SearchTerm))
            customers = await _customerService.SearchCustomersAsync(SearchTerm);
        else
            customers = await _customerService.GetAllCustomersAsync();

        Customers = new ObservableCollection<Customer>(customers);
    }

    [RelayCommand]
    private void ViewCustomer(Customer customer)
    {
        _mainViewModel.NavigateToCustomerProfile(customer.Id);
    }

    [RelayCommand]
    private async Task DeleteCustomer(Customer customer)
    {
        var result = System.Windows.MessageBox.Show(
            $"Are you sure you want to delete/archive \"{customer.FullName}\"?",
            "Confirm Delete",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (result != System.Windows.MessageBoxResult.Yes)
            return;

        var canDelete = await _customerService.CanDeleteCustomerAsync(customer.Id);
        if (canDelete)
        {
            await _customerService.DeleteCustomerAsync(customer.Id);
        }
        else
        {
            await _customerService.ArchiveCustomerAsync(customer.Id);
        }
        await LoadCustomersAsync();
    }

    partial void OnSearchTermChanged(string value)
    {
        _ = LoadCustomersAsync();
    }
}
