using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CraftTrack.Models;
using CraftTrack.Models.Enums;
using CraftTrack.Services;

namespace CraftTrack.ViewModels;

public partial class InventoryListViewModel : ObservableObject
{
    private readonly IInventoryService _inventoryService;
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty] private ObservableCollection<InventoryItem> _items = [];
    [ObservableProperty] private InventoryCategory? _filterCategory;
    [ObservableProperty] private string _searchTerm = string.Empty;

    public InventoryListViewModel(IInventoryService inventoryService, MainViewModel mainViewModel)
    {
        _inventoryService = inventoryService;
        _mainViewModel = mainViewModel;
        _ = LoadItemsAsync();
    }

    [RelayCommand]
    private async Task LoadItemsAsync()
    {
        var items = await _inventoryService.GetAllItemsAsync(FilterCategory);
        if (!string.IsNullOrWhiteSpace(SearchTerm))
            items = items.Where(i => i.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
        Items = new ObservableCollection<InventoryItem>(items);
    }

    [RelayCommand]
    private void AddItem()
    {
        _mainViewModel.NavigateToInventoryDetail();
    }

    [RelayCommand]
    private void EditItem(InventoryItem item)
    {
        _mainViewModel.NavigateToInventoryDetail(item.Id);
    }

    [RelayCommand]
    private void CopyItem(InventoryItem item)
    {
        _mainViewModel.NavigateToInventoryDetailCopy(item.Id);
    }

    [RelayCommand]
    private async Task DeleteItem(InventoryItem item)
    {
        var result = System.Windows.MessageBox.Show(
            $"Are you sure you want to delete \"{item.Name}\"?",
            "Confirm Delete",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (result != System.Windows.MessageBoxResult.Yes)
            return;

        await _inventoryService.DeleteItemAsync(item.Id);
        Items.Remove(item);
    }

    partial void OnFilterCategoryChanged(InventoryCategory? value) => _ = LoadItemsAsync();
    partial void OnSearchTermChanged(string value) => _ = LoadItemsAsync();
}
