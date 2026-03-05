using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CraftTrack.Models;
using CraftTrack.Models.Enums;
using CraftTrack.Services;

namespace CraftTrack.ViewModels;

public partial class RejectListViewModel : ObservableObject
{
    private readonly IRejectService _rejectService;
    private readonly IInventoryService _inventoryService;
    private readonly INotificationService _notificationService;
    private readonly MainViewModel _mainViewModel;

    // ── List state ──
    [ObservableProperty] private ObservableCollection<RejectRecord> _rejectRecords = [];
    [ObservableProperty] private RejectReason? _filterReason;
    [ObservableProperty] private DateTime? _filterFrom;
    [ObservableProperty] private DateTime? _filterTo;
    [ObservableProperty] private string _searchTerm = string.Empty;

    // ── Summary KPIs ──
    [ObservableProperty] private int _totalRejectCount;
    [ObservableProperty] private decimal _totalRejectionCost;

    // ── Reject Inventory Dialog ──
    [ObservableProperty] private bool _isRejectDialogOpen;
    [ObservableProperty] private ObservableCollection<InventoryItem> _inventoryItems = [];
    [ObservableProperty] private InventoryItem? _selectedInventoryItem;
    [ObservableProperty] private decimal _rejectQuantity;
    [ObservableProperty] private RejectReason _rejectReason = RejectReason.Other;
    [ObservableProperty] private string _rejectNotes = string.Empty;
    [ObservableProperty] private string _dialogErrorMessage = string.Empty;

    public RejectListViewModel(
        IRejectService rejectService,
        IInventoryService inventoryService,
        INotificationService notificationService,
        MainViewModel mainViewModel)
    {
        _rejectService = rejectService;
        _inventoryService = inventoryService;
        _notificationService = notificationService;
        _mainViewModel = mainViewModel;
    }

    public async Task InitializeAsync()
    {
        await LoadRejectRecordsAsync();
        await LoadKpisAsync();
    }

    [RelayCommand]
    private async Task LoadRejectRecordsAsync()
    {
        var records = await _rejectService.GetRejectRecordsAsync(FilterFrom, FilterTo, FilterReason);

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            records = records.Where(r =>
                (r.OrderItem?.Product?.Name?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (r.InventoryItem?.Name?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (r.Notes?.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
            ).ToList();
        }

        RejectRecords = new ObservableCollection<RejectRecord>(records);
    }

    private async Task LoadKpisAsync()
    {
        TotalRejectCount = await _rejectService.GetRejectCountAsync(FilterFrom, FilterTo);
        TotalRejectionCost = await _rejectService.GetTotalRejectionCostAsync(FilterFrom, FilterTo);
    }

    [RelayCommand]
    private async Task ApplyFilters()
    {
        await LoadRejectRecordsAsync();
        await LoadKpisAsync();
    }

    [RelayCommand]
    private async Task ClearFilters()
    {
        FilterReason = null;
        FilterFrom = null;
        FilterTo = null;
        SearchTerm = string.Empty;
        await LoadRejectRecordsAsync();
        await LoadKpisAsync();
    }

    // ── Quick reject from inventory ──

    [RelayCommand]
    private async Task OpenRejectDialog()
    {
        DialogErrorMessage = string.Empty;
        RejectQuantity = 0;
        RejectReason = RejectReason.Other;
        RejectNotes = string.Empty;
        SelectedInventoryItem = null;

        var items = await _inventoryService.GetAllItemsAsync();
        InventoryItems = new ObservableCollection<InventoryItem>(items);
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
        DialogErrorMessage = string.Empty;

        if (SelectedInventoryItem == null)
        {
            DialogErrorMessage = "Please select an inventory item.";
            return;
        }

        if (RejectQuantity <= 0)
        {
            DialogErrorMessage = "Quantity must be greater than zero.";
            return;
        }

        if (RejectQuantity > SelectedInventoryItem.Quantity)
        {
            DialogErrorMessage = $"Cannot reject more than available stock ({SelectedInventoryItem.Quantity:N2}).";
            return;
        }

        try
        {
            var record = await _rejectService.RejectInventoryItemAsync(
                SelectedInventoryItem.Id, RejectQuantity, RejectReason,
                string.IsNullOrWhiteSpace(RejectNotes) ? null : RejectNotes.Trim());

            await _notificationService.GenerateRejectNotificationAsync(record, SelectedInventoryItem.Name);
            await _mainViewModel.RefreshNotificationBadgeAsync();

            IsRejectDialogOpen = false;
            await LoadRejectRecordsAsync();
            await LoadKpisAsync();
        }
        catch (Exception ex)
        {
            DialogErrorMessage = ex.Message;
        }
    }

    partial void OnFilterReasonChanged(RejectReason? value) => _ = ApplyFilters();
    partial void OnSearchTermChanged(string value) => _ = LoadRejectRecordsAsync();
}
