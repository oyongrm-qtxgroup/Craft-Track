using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CraftTrack.Services;

namespace CraftTrack.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;
    private readonly IInventoryService _inventoryService;
    private readonly IFinanceService _financeService;
    private readonly INotificationService _notificationService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private ObservableObject? _currentView;

    [ObservableProperty]
    private string _currentViewTitle = "Dashboard";

    [ObservableProperty]
    private int _notificationBadgeCount;

    [ObservableProperty]
    private bool _hasNotifications;

    public MainViewModel(
        IOrderService orderService,
        ICustomerService customerService,
        IInventoryService inventoryService,
        IFinanceService financeService,
        INotificationService notificationService,
        IServiceProvider serviceProvider)
    {
        _orderService = orderService;
        _customerService = customerService;
        _inventoryService = inventoryService;
        _financeService = financeService;
        _notificationService = notificationService;
        _serviceProvider = serviceProvider;
    }

    public async Task InitializeAsync()
    {
        await RefreshNotificationBadgeAsync();
        NavigateTo("Dashboard");
    }

    [RelayCommand]
    public void NavigateTo(string viewName)
    {
        CurrentView = viewName switch
        {
            "Dashboard" => GetViewModel<DashboardViewModel>(),
            "Orders" => GetViewModel<OrderListViewModel>(),
            "Customers" => GetViewModel<CustomerListViewModel>(),
            "Inventory" => GetViewModel<InventoryListViewModel>(),
            "Products" => GetViewModel<ProductListViewModel>(),
            "Expenses" => GetViewModel<ExpensesViewModel>(),
            "Rejects" => GetViewModel<RejectListViewModel>(),
            "Notifications" => GetViewModel<NotificationCenterViewModel>(),
            "Settings" => GetViewModel<SettingsViewModel>(),
            _ => GetViewModel<DashboardViewModel>()
        };

        CurrentViewTitle = viewName switch
        {
            "Dashboard" => "Dashboard",
            "Orders" => "Orders",
            "Customers" => "Customers",
            "Inventory" => "Inventory",
            "Products" => "Products",
            "Expenses" => "Expenses",
            "Rejects" => "Reject Records",
            "Notifications" => "Notifications",
            "Settings" => "Settings",
            _ => "Dashboard"
        };
    }

    public void NavigateToOrderDetail(int? orderId = null)
    {
        var vm = GetViewModel<OrderDetailViewModel>();
        if (orderId.HasValue)
            vm.LoadOrderAsync(orderId.Value).ConfigureAwait(false);
        else
            vm.InitializeNewOrder();
        CurrentView = vm;
        CurrentViewTitle = orderId.HasValue ? $"Edit Order #{orderId}" : "New Order";
    }

    public void NavigateToCustomerProfile(int customerId)
    {
        var vm = GetViewModel<CustomerProfileViewModel>();
        vm.LoadCustomerAsync(customerId).ConfigureAwait(false);
        CurrentView = vm;
        CurrentViewTitle = "Customer Profile";
    }

    public void NavigateToInventoryDetail(int? itemId = null)
    {
        var vm = GetViewModel<InventoryDetailViewModel>();
        if (itemId.HasValue)
            vm.LoadItemAsync(itemId.Value).ConfigureAwait(false);
        else
            vm.InitializeNewItem();
        CurrentView = vm;
        CurrentViewTitle = itemId.HasValue ? "Edit Inventory Item" : "New Inventory Item";
    }

    public void NavigateToInventoryDetailCopy(int itemId)
    {
        var vm = GetViewModel<InventoryDetailViewModel>();
        vm.LoadItemForCopyAsync(itemId).ConfigureAwait(false);
        CurrentView = vm;
        CurrentViewTitle = "Copy Inventory Item";
    }

    public void NavigateToProductDetail(int? productId = null)
    {
        var vm = GetViewModel<ProductDetailViewModel>();
        if (productId.HasValue)
            vm.LoadProductAsync(productId.Value).ConfigureAwait(false);
        else
            vm.InitializeNewProduct();
        CurrentView = vm;
        CurrentViewTitle = productId.HasValue ? "Edit Product" : "New Product";
    }

    public async Task RefreshNotificationBadgeAsync()
    {
        NotificationBadgeCount = await _notificationService.GetUnacknowledgedCountAsync();
        HasNotifications = NotificationBadgeCount > 0;
    }

    private T GetViewModel<T>() where T : notnull
    {
        return (T)_serviceProvider.GetService(typeof(T))!;
    }
}
