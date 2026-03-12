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
    private readonly IRejectService _rejectService;
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty] private int _itemId;
    [ObservableProperty] private bool _isNewItem = true;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _variant = string.Empty;
    [ObservableProperty] private string _size = string.Empty;
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

    // Reject dialog
    [ObservableProperty] private bool _isRejectDialogOpen;
    [ObservableProperty] private decimal _rejectQuantity;
    [ObservableProperty] private RejectReason _rejectReason = RejectReason.MaterialDefect;
    [ObservableProperty] private string _rejectNotes = string.Empty;
    [ObservableProperty] private string _rejectErrorMessage = string.Empty;
    [ObservableProperty] private ObservableCollection<RejectRecord> _rejectHistory = [];
    [ObservableProperty] private ObservableCollection<CostHistory> _costHistory = [];

    public InventoryDetailViewModel(
        IInventoryService inventoryService,
        INotificationService notificationService,
        IRejectService rejectService,
        MainViewModel mainViewModel)
    {
        _inventoryService = inventoryService;
        _notificationService = notificationService;
        _rejectService = rejectService;
        _mainViewModel = mainViewModel;
    }

    public void InitializeNewItem()
    {
        IsNewItem = true;
        ItemId = 0;
        Name = string.Empty;
        Variant = string.Empty;
        Size = string.Empty;
        Category = InventoryCategory.RawMaterial;
        UnitOfMeasure = string.Empty;
        Quantity = 0;
        LowStockThreshold = 0;
        UnitCost = 0;
        Notes = string.Empty;
        AdjustmentHistory = [];
        CostHistory = [];
    }

    public async Task LoadItemAsync(int itemId)
    {
        var item = await _inventoryService.GetItemWithAdjustmentsAsync(itemId);
        if (item == null) return;

        IsNewItem = false;
        ItemId = item.Id;
        Name = item.Name;
        Variant = item.Variant ?? string.Empty;
        Size = item.Size ?? string.Empty;
        Category = item.Category;
        UnitOfMeasure = item.UnitOfMeasure ?? string.Empty;
        Quantity = item.Quantity;
        LowStockThreshold = item.LowStockThreshold;
        UnitCost = item.UnitCost;
        Notes = item.Notes ?? string.Empty;
        AdjustmentHistory = new ObservableCollection<StockAdjustment>(item.StockAdjustments);

        // Load reject history for this item
        var rejects = await _rejectService.GetRejectRecordsByInventoryItemAsync(itemId);
        RejectHistory = new ObservableCollection<RejectRecord>(rejects);

        // Load cost history
        var costs = await _inventoryService.GetCostHistoryAsync(itemId);
        CostHistory = new ObservableCollection<CostHistory>(costs);
    }

    public async Task LoadItemForCopyAsync(int itemId)
    {
        var item = await _inventoryService.GetItemWithAdjustmentsAsync(itemId);
        if (item == null) return;

        IsNewItem = true;
        ItemId = 0;
        Name = item.Name + " (Copy)";
        Variant = item.Variant ?? string.Empty;
        Size = item.Size ?? string.Empty;
        Category = item.Category;
        UnitOfMeasure = item.UnitOfMeasure ?? string.Empty;
        Quantity = 0;
        LowStockThreshold = item.LowStockThreshold;
        UnitCost = item.UnitCost;
        Notes = item.Notes ?? string.Empty;
        AdjustmentHistory = [];
        RejectHistory = [];
        CostHistory = [];
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
                Variant = string.IsNullOrWhiteSpace(Variant) ? null : Variant.Trim(),
                Size = string.IsNullOrWhiteSpace(Size) ? null : Size.Trim(),
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
            existing.Variant = string.IsNullOrWhiteSpace(Variant) ? null : Variant.Trim();
            existing.Size = string.IsNullOrWhiteSpace(Size) ? null : Size.Trim();
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

    // ── Reject from inventory ──

    [RelayCommand]
    private void OpenRejectDialog()
    {
        RejectQuantity = 0;
        RejectReason = RejectReason.MaterialDefect;
        RejectNotes = string.Empty;
        RejectErrorMessage = string.Empty;
        IsRejectDialogOpen = true;
    }

    [RelayCommand]
    private void CloseRejectDialog()
    {
        IsRejectDialogOpen = false;
    }

    [RelayCommand]
    private async Task SubmitReject()
    {
        RejectErrorMessage = string.Empty;

        if (RejectQuantity <= 0)
        {
            RejectErrorMessage = "Quantity must be greater than zero.";
            return;
        }

        if (RejectQuantity > Quantity)
        {
            RejectErrorMessage = $"Cannot reject more than available stock ({Quantity:N2}).";
            return;
        }

        try
        {
            var record = await _rejectService.RejectInventoryItemAsync(
                ItemId, RejectQuantity, RejectReason,
                string.IsNullOrWhiteSpace(RejectNotes) ? null : RejectNotes.Trim());

            await _notificationService.GenerateRejectNotificationAsync(record, Name);
            await _mainViewModel.RefreshNotificationBadgeAsync();

            // Refresh local state
            Quantity -= RejectQuantity;
            IsRejectDialogOpen = false;

            var rejects = await _rejectService.GetRejectRecordsByInventoryItemAsync(ItemId);
            RejectHistory = new ObservableCollection<RejectRecord>(rejects);
        }
        catch (Exception ex)
        {
            RejectErrorMessage = ex.Message;
        }
    }
}
