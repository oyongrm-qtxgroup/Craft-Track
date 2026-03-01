using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CraftTrack.Models;
using CraftTrack.Services;

namespace CraftTrack.ViewModels;

public partial class NotificationCenterViewModel : ObservableObject
{
    private readonly INotificationService _notificationService;
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty] private ObservableCollection<Notification> _notifications = [];
    [ObservableProperty] private bool _showAcknowledged;

    public NotificationCenterViewModel(INotificationService notificationService, MainViewModel mainViewModel)
    {
        _notificationService = notificationService;
        _mainViewModel = mainViewModel;
        _ = LoadNotificationsAsync();
    }

    [RelayCommand]
    private async Task LoadNotificationsAsync()
    {
        var notifications = await _notificationService.GetAllNotificationsAsync(ShowAcknowledged);
        Notifications = new ObservableCollection<Notification>(notifications);
    }

    [RelayCommand]
    private async Task AcknowledgeNotification(Notification notification)
    {
        await _notificationService.AcknowledgeNotificationAsync(notification.Id);
        await LoadNotificationsAsync();
        await _mainViewModel.RefreshNotificationBadgeAsync();
    }

    [RelayCommand]
    private async Task AcknowledgeAll()
    {
        await _notificationService.AcknowledgeAllNotificationsAsync();
        await LoadNotificationsAsync();
        await _mainViewModel.RefreshNotificationBadgeAsync();
    }

    [RelayCommand]
    private async Task DeleteNotification(Notification notification)
    {
        await _notificationService.DeleteNotificationAsync(notification.Id);
        await LoadNotificationsAsync();
        await _mainViewModel.RefreshNotificationBadgeAsync();
    }

    [RelayCommand]
    private void ViewRelatedItem(Notification notification)
    {
        if (notification.RelatedOrderId.HasValue)
            _mainViewModel.NavigateToOrderDetail(notification.RelatedOrderId.Value);
        else if (notification.RelatedInventoryItemId.HasValue)
            _mainViewModel.NavigateToInventoryDetail(notification.RelatedInventoryItemId.Value);
    }

    partial void OnShowAcknowledgedChanged(bool value) => _ = LoadNotificationsAsync();
}
