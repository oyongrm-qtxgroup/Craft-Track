using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CraftTrack.Models;
using CraftTrack.Models.Enums;
using CraftTrack.Services;

namespace CraftTrack.ViewModels;

public partial class InventoryDetailViewModel : ObservableObject
{
    private readonly IInventoryService _inventoryService;
    private readonly INotificationService _notificationService;
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty] private int _itemId;
    [ObservableProperty] private bool _isNewItem = true;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private InventoryCategory _category = InventoryCategory.RawMaterial;
    [ObservableProperty] private string _unitOfMeasure = string.Empty;
    [ObservableProperty] private decimal _quantity;
    [ObservableProperty] private decimal _lowStockThreshold;
    [ObservableProperty] private decimal _unitCost;
    [ObservableProperty] private string _notes = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;

    // Stock adjustment
    [ObservableProperty] private AdjustmentType _adjustmentType = AdjustmentType.Add;
    [ObservableProperty] private decimal _adjustmentQuantity;
    [ObservableProperty] private string _adjustmentNote = string.Empty;
    [ObservableProperty] private ObservableCollection<StockAdjustment> _adjustmentHistory = [];

    public InventoryDetailViewModel(
        IInventoryService inventoryService,
        INotificationService notificationService,
        MainViewModel mainViewModel)
    {
        _inventoryService = inventoryService;
        _notificationService = notificationService;
        _mainViewModel = mainViewModel;
    }

    public void InitializeNewItem()
    {
        IsNewItem = true;
        ItemId = 0;
        Name = string.Empty;
        Category = InventoryCategory.RawMaterial;
        UnitOfMeasure = string.Empty;
        Quantity = 0;
        LowStockThreshold = 0;
        UnitCost = 0;
        Notes = string.Empty;
        AdjustmentHistory = [];
    }

    public async Task LoadItemAsync(int itemId)
    {
        var item = await _inventoryService.GetItemWithAdjustmentsAsync(itemId);
        if (item == null) return;

        IsNewItem = false;
        ItemId = item.Id;
        Name = item.Name;
        Category = item.Category;
        UnitOfMeasure = item.UnitOfMeasure ?? string.Empty;
        Quantity = item.Quantity;
        LowStockThreshold = item.LowStockThreshold;
        UnitCost = item.UnitCost;
        Notes = item.Notes ?? string.Empty;
        AdjustmentHistory = new ObservableCollection<StockAdjustment>(item.StockAdjustments);
    }

    [RelayCommand]
    private async Task SaveItem()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Item name is required.";
            return;
        }

        if (IsNewItem)
        {
            var item = new InventoryItem
            {
                Name = Name.Trim(),
                Category = Category,
                UnitOfMeasure = string.IsNullOrWhiteSpace(UnitOfMeasure) ? null : UnitOfMeasure.Trim(),
                Quantity = Quantity,
                LowStockThreshold = LowStockThreshold,
                UnitCost = UnitCost,
                Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim()
            };
            await _inventoryService.CreateItemAsync(item);

            if (item.IsLowStock)
                await _notificationService.GenerateLowStockNotificationAsync(item);
        }
        else
        {
            var existing = await _inventoryService.GetItemByIdAsync(ItemId);
            if (existing == null) return;

            existing.Name = Name.Trim();
            existing.Category = Category;
            existing.UnitOfMeasure = string.IsNullOrWhiteSpace(UnitOfMeasure) ? null : UnitOfMeasure.Trim();
            existing.LowStockThreshold = LowStockThreshold;
            existing.UnitCost = UnitCost;
            existing.Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim();

            await _inventoryService.UpdateItemAsync(existing);
        }

        _mainViewModel.NavigateTo("Inventory");
    }

    [RelayCommand]
    private async Task AdjustStock()
    {
        if (AdjustmentQuantity <= 0)
        {
            ErrorMessage = "Adjustment quantity must be greater than zero.";
            return;
        }

        ErrorMessage = string.Empty;
        var item = await _inventoryService.AdjustStockAsync(
            ItemId, AdjustmentType, AdjustmentQuantity,
            string.IsNullOrWhiteSpace(AdjustmentNote) ? null : AdjustmentNote.Trim());

        Quantity = item.Quantity;
        AdjustmentQuantity = 0;
        AdjustmentNote = string.Empty;

        // Refresh adjustment history
        var history = await _inventoryService.GetAdjustmentHistoryAsync(ItemId);
        AdjustmentHistory = new ObservableCollection<StockAdjustment>(history);

        // Check low stock
        if (item.IsLowStock)
        {
            await _notificationService.GenerateLowStockNotificationAsync(item);
            await _mainViewModel.RefreshNotificationBadgeAsync();
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _mainViewModel.NavigateTo("Inventory");
    }
}
