using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CraftTrack.Helpers;
using CraftTrack.Models;
using CraftTrack.Models.Enums;
using CraftTrack.Services;

namespace CraftTrack.ViewModels;

public partial class OrderDetailViewModel : ObservableObject
{
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;
    private readonly IProductService _productService;
    private readonly INotificationService _notificationService;
    private readonly IRejectService _rejectService;
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty] private int _orderId;
    [ObservableProperty] private bool _isNewOrder = true;
    [ObservableProperty] private string _customerName = string.Empty;
    [ObservableProperty] private string _customerPhone = string.Empty;
    [ObservableProperty] private DateTime _dateOrdered = DateTime.Today;
    [ObservableProperty] private DateTime _dueDate = DateTime.Today.AddDays(7);
    [ObservableProperty] private SeverityLevel _severity = SeverityLevel.Low;
    [ObservableProperty] private OrderStatus _status = OrderStatus.Active;
    [ObservableProperty] private PaymentType _paymentType = PaymentType.Unpaid;
    [ObservableProperty] private decimal _amountPaid;
    [ObservableProperty] private decimal _totalPrice;
    [ObservableProperty] private decimal _balanceDue;
    [ObservableProperty] private decimal _subTotal;
    [ObservableProperty] private DiscountType _discountType = DiscountType.None;
    [ObservableProperty] private decimal _discountValue;
    [ObservableProperty] private decimal _discountAmount;
    [ObservableProperty] private string _notes = string.Empty;
    [ObservableProperty] private ObservableCollection<OrderItemViewModel> _orderItems = [];
    [ObservableProperty] private ObservableCollection<Customer> _customerSuggestions = [];
    [ObservableProperty] private Customer? _selectedCustomer;
    [ObservableProperty] private bool _isCustomerDropdownOpen;
    [ObservableProperty] private string _errorMessage = string.Empty;

    // Product picker
    [ObservableProperty] private bool _isProductPickerOpen;
    [ObservableProperty] private ObservableCollection<Product> _availableProducts = [];
    [ObservableProperty] private string _productSearchTerm = string.Empty;
    [ObservableProperty] private string _stockWarningMessage = string.Empty;

    // Reject dialog
    [ObservableProperty] private bool _isRejectDialogOpen;
    [ObservableProperty] private OrderItemViewModel? _rejectTargetItem;
    [ObservableProperty] private decimal _rejectQuantity;
    [ObservableProperty] private RejectReason _rejectReason = RejectReason.WrongPrinting;
    [ObservableProperty] private string _rejectNotes = string.Empty;
    [ObservableProperty] private string _rejectErrorMessage = string.Empty;

    // Payment history
    [ObservableProperty] private ObservableCollection<Payment> _paymentHistory = [];
    [ObservableProperty] private bool _isPaymentDialogOpen;
    [ObservableProperty] private decimal _paymentAmount;
    [ObservableProperty] private string _paymentNotes = string.Empty;
    [ObservableProperty] private string _paymentErrorMessage = string.Empty;

    public OrderDetailViewModel(
        IOrderService orderService,
        ICustomerService customerService,
        IProductService productService,
        INotificationService notificationService,
        IRejectService rejectService,
        MainViewModel mainViewModel)
    {
        _orderService = orderService;
        _customerService = customerService;
        _productService = productService;
        _notificationService = notificationService;
        _rejectService = rejectService;
        _mainViewModel = mainViewModel;
    }

    public void InitializeNewOrder()
    {
        IsNewOrder = true;
        OrderId = 0;
        CustomerName = string.Empty;
        CustomerPhone = string.Empty;
        DateOrdered = DateTime.Today;
        DueDate = DateTime.Today.AddDays(7);
        Severity = SeverityLevel.Low;
        Status = OrderStatus.Active;
        PaymentType = PaymentType.Unpaid;
        AmountPaid = 0;
        SubTotal = 0;
        DiscountType = DiscountType.None;
        DiscountValue = 0;
        DiscountAmount = 0;
        TotalPrice = 0;
        BalanceDue = 0;
        Notes = string.Empty;
        OrderItems = [];
        StockWarningMessage = string.Empty;
        PaymentHistory = [];
    }

    public async Task LoadOrderAsync(int orderId)
    {
        var order = await _orderService.GetOrderWithDetailsAsync(orderId);
        if (order == null) return;

        IsNewOrder = false;
        OrderId = order.Id;
        CustomerName = order.Customer.FullName;
        CustomerPhone = order.Customer.Phone ?? string.Empty;
        SelectedCustomer = order.Customer;
        DateOrdered = order.DateOrdered;
        DueDate = order.DueDate;
        Severity = order.Severity;
        Status = order.Status;
        PaymentType = order.PaymentType;
        AmountPaid = order.AmountPaid;
        DiscountType = order.DiscountType;
        DiscountValue = order.DiscountValue;
        TotalPrice = order.TotalPrice;
        BalanceDue = order.BalanceDue;
        Notes = order.Notes ?? string.Empty;

        // Load payment history
        PaymentHistory = new ObservableCollection<Payment>(
            order.Payments.OrderByDescending(p => p.PaidAt));

        OrderItems = new ObservableCollection<OrderItemViewModel>(
            order.OrderItems.Select(i =>
            {
                var vm = new OrderItemViewModel
                {
                    OrderItemId = i.Id,
                    ProductId = i.ProductId ?? 0,
                    ProductName = i.Product?.Name ?? i.ItemType ?? "Unknown",
                    ProductImagePath = i.Product?.ImagePath,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                };
                return vm;
            }));

        // Recalculate subtotal from loaded items
        SubTotal = OrderItems.Sum(i => i.LineTotal);
    }

    [RelayCommand]
    private async Task SearchCustomers()
    {
        if (string.IsNullOrWhiteSpace(CustomerName) || CustomerName.Length < 2)
        {
            CustomerSuggestions.Clear();
            IsCustomerDropdownOpen = false;
            return;
        }

        var customers = await _customerService.SearchCustomersAsync(CustomerName);
        CustomerSuggestions = new ObservableCollection<Customer>(customers);
        IsCustomerDropdownOpen = customers.Count > 0;
    }

    [RelayCommand]
    private void SelectCustomer(Customer customer)
    {
        SelectedCustomer = customer;
        CustomerName = customer.FullName;
        CustomerPhone = customer.Phone ?? string.Empty;
        IsCustomerDropdownOpen = false;
    }

    // ── Product Picker ──

    [RelayCommand]
    private async Task OpenProductPicker()
    {
        ProductSearchTerm = string.Empty;
        await LoadAvailableProducts();
        IsProductPickerOpen = true;
    }

    [RelayCommand]
    private void CloseProductPicker()
    {
        IsProductPickerOpen = false;
    }

    partial void OnProductSearchTermChanged(string value) => _ = LoadAvailableProducts();

    private async Task LoadAvailableProducts()
    {
        var products = string.IsNullOrWhiteSpace(ProductSearchTerm)
            ? await _productService.GetAllProductsAsync()
            : await _productService.SearchProductsAsync(ProductSearchTerm);

        AvailableProducts = new ObservableCollection<Product>(products);
    }

    [RelayCommand]
    private void SelectProduct(Product product)
    {
        // Add product as order item
        OrderItems.Add(new OrderItemViewModel
        {
            ProductId = product.Id,
            ProductName = product.Name,
            ProductImagePath = product.ImagePath,
            Quantity = 1,
            UnitPrice = product.DefaultPrice
        });
        RecalculateTotal();
        IsProductPickerOpen = false;
    }

    [RelayCommand]
    private void RemoveOrderItem(OrderItemViewModel item)
    {
        OrderItems.Remove(item);
        RecalculateTotal();
    }

    public void RecalculateTotal()
    {
        SubTotal = OrderItems.Sum(i => i.LineTotal);

        DiscountAmount = DiscountType switch
        {
            DiscountType.Percentage => Math.Round(SubTotal * DiscountValue / 100m, 2),
            DiscountType.FixedAmount => Math.Min(DiscountValue, SubTotal),
            _ => 0
        };

        TotalPrice = Math.Max(0, SubTotal - DiscountAmount);
        BalanceDue = TotalPrice - AmountPaid;
    }

    partial void OnDiscountTypeChanged(DiscountType value) => RecalculateTotal();
    partial void OnDiscountValueChanged(decimal value) => RecalculateTotal();

    partial void OnAmountPaidChanged(decimal value)
    {
        BalanceDue = TotalPrice - value;

        if (value >= TotalPrice && TotalPrice > 0)
            PaymentType = PaymentType.FullPayment;
        else if (value > 0)
            PaymentType = PaymentType.Downpayment;
        else
            PaymentType = PaymentType.Unpaid;
    }

    partial void OnPaymentTypeChanged(PaymentType value)
    {
        if (value == PaymentType.FullPayment)
        {
            AmountPaid = TotalPrice;
            BalanceDue = 0;
        }
    }

    private async Task CheckStockAvailability()
    {
        StockWarningMessage = string.Empty;
        var warnings = new List<string>();

        foreach (var item in OrderItems)
        {
            if (item.ProductId <= 0) continue;
            var availability = await _productService.CheckMaterialAvailabilityAsync(item.ProductId, item.Quantity);
            foreach (var mat in availability.Where(a => !a.IsSufficient))
            {
                warnings.Add($"⚠ {mat.InventoryItemName}: need {mat.Required:N2} {mat.UnitOfMeasure}, only {mat.Available:N2} in stock");
            }
        }

        if (warnings.Any())
            StockWarningMessage = string.Join("\n", warnings);
    }

    [RelayCommand]
    private async Task SaveOrder()
    {
        ErrorMessage = string.Empty;

        // Validation
        if (string.IsNullOrWhiteSpace(CustomerName))
        {
            ErrorMessage = "Customer name is required.";
            return;
        }

        if (DueDate < DateOrdered)
        {
            ErrorMessage = "Due date must be on or after the order date.";
            return;
        }

        if (!OrderItems.Any())
        {
            ErrorMessage = "At least one product is required.";
            return;
        }

        // Check stock availability (warning only)
        await CheckStockAvailability();

        // Ensure customer exists
        Customer customer;
        if (SelectedCustomer != null && SelectedCustomer.FullName == CustomerName)
        {
            customer = SelectedCustomer;
        }
        else
        {
            customer = await _customerService.CreateCustomerAsync(new Customer
            {
                FullName = CustomerName.Trim(),
                Phone = string.IsNullOrWhiteSpace(CustomerPhone) ? null : CustomerPhone.Trim()
            });
        }

        var order = new Order
        {
            Id = OrderId,
            CustomerId = customer.Id,
            DateOrdered = DateOrdered,
            DueDate = DueDate,
            Severity = Severity,
            Status = Status,
            PaymentType = PaymentType,
            AmountPaid = AmountPaid,
            DiscountType = DiscountType,
            DiscountValue = DiscountValue,
            Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
            OrderItems = OrderItems.Select(i => new OrderItem
            {
                ProductId = i.ProductId > 0 ? i.ProductId : null,
                ItemType = i.ProductName, // Store as legacy fallback
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        if (IsNewOrder)
        {
            var created = await _orderService.CreateOrderAsync(order);

            if (Severity == SeverityLevel.Rush)
            {
                created.Customer = customer;
                await _notificationService.GenerateRushOrderNotificationAsync(created);
            }
        }
        else
        {
            await _orderService.UpdateOrderAsync(order);
        }

        await _mainViewModel.RefreshNotificationBadgeAsync();
        _mainViewModel.NavigateTo("Orders");
    }

    // ── Reject Order Item ──

    [RelayCommand]
    private void OpenRejectDialog(OrderItemViewModel item)
    {
        if (item.OrderItemId <= 0)
        {
            ErrorMessage = "Save the order first before rejecting items.";
            return;
        }

        RejectTargetItem = item;
        RejectQuantity = 1;
        RejectReason = RejectReason.WrongPrinting;
        RejectNotes = string.Empty;
        RejectErrorMessage = string.Empty;
        IsRejectDialogOpen = true;
    }

    [RelayCommand]
    private void CloseRejectDialog()
    {
        IsRejectDialogOpen = false;
        RejectTargetItem = null;
    }

    [RelayCommand]
    private async Task SubmitReject()
    {
        RejectErrorMessage = string.Empty;

        if (RejectTargetItem == null) return;

        if (RejectQuantity <= 0)
        {
            RejectErrorMessage = "Quantity must be greater than zero.";
            return;
        }

        if (RejectQuantity > RejectTargetItem.Quantity)
        {
            RejectErrorMessage = $"Cannot reject more than the ordered quantity ({RejectTargetItem.Quantity}).";
            return;
        }

        try
        {
            var record = await _rejectService.RejectOrderItemAsync(
                RejectTargetItem.OrderItemId,
                RejectQuantity,
                RejectReason,
                string.IsNullOrWhiteSpace(RejectNotes) ? null : RejectNotes.Trim());

            await _notificationService.GenerateRejectNotificationAsync(record, RejectTargetItem.ProductName);
            await _mainViewModel.RefreshNotificationBadgeAsync();

            IsRejectDialogOpen = false;
            RejectTargetItem = null;
            ErrorMessage = string.Empty;
            StockWarningMessage = $"✓ {RejectQuantity:N0} item(s) rejected. Materials deducted from inventory.";
        }
        catch (Exception ex)
        {
            RejectErrorMessage = ex.Message;
        }
    }

    // ── Payment History ──

    [RelayCommand]
    private void OpenPaymentDialog()
    {
        if (IsNewOrder || OrderId <= 0)
        {
            ErrorMessage = "Save the order first before adding payments.";
            return;
        }

        PaymentAmount = 0;
        PaymentNotes = string.Empty;
        PaymentErrorMessage = string.Empty;
        IsPaymentDialogOpen = true;
    }

    [RelayCommand]
    private void ClosePaymentDialog()
    {
        IsPaymentDialogOpen = false;
    }

    [RelayCommand]
    private async Task SubmitPayment()
    {
        PaymentErrorMessage = string.Empty;

        if (PaymentAmount <= 0)
        {
            PaymentErrorMessage = "Amount must be greater than zero.";
            return;
        }

        try
        {
            var payment = await _orderService.AddPaymentAsync(
                OrderId,
                PaymentAmount,
                string.IsNullOrWhiteSpace(PaymentNotes) ? null : PaymentNotes.Trim());

            // Update local state
            AmountPaid += PaymentAmount;
            BalanceDue = TotalPrice - AmountPaid;

            if (AmountPaid >= TotalPrice && TotalPrice > 0)
                PaymentType = PaymentType.FullPayment;
            else if (AmountPaid > 0)
                PaymentType = PaymentType.Downpayment;
            else
                PaymentType = PaymentType.Unpaid;

            PaymentHistory.Insert(0, payment);
            IsPaymentDialogOpen = false;
        }
        catch (Exception ex)
        {
            PaymentErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private async Task DeletePayment(Payment payment)
    {
        var deleted = await _orderService.DeletePaymentAsync(payment.Id);
        if (!deleted) return;

        AmountPaid -= payment.Amount;
        if (AmountPaid < 0) AmountPaid = 0;
        BalanceDue = TotalPrice - AmountPaid;

        if (AmountPaid >= TotalPrice && TotalPrice > 0)
            PaymentType = PaymentType.FullPayment;
        else if (AmountPaid > 0)
            PaymentType = PaymentType.Downpayment;
        else
            PaymentType = PaymentType.Unpaid;

        PaymentHistory.Remove(payment);
    }

    [RelayCommand]
    private void Cancel()
    {
        _mainViewModel.NavigateTo("Orders");
    }
}

public partial class OrderItemViewModel : ObservableObject
{
    [ObservableProperty] private int _orderItemId; // database PK — 0 for unsaved items
    [ObservableProperty] private int _productId;
    [ObservableProperty] private string _productName = string.Empty;
    [ObservableProperty] private string? _productImagePath;
    [ObservableProperty] private int _quantity = 1;
    [ObservableProperty] private decimal _unitPrice;

    // Legacy fields for backward compat display
    [ObservableProperty] private string _itemType = string.Empty;
    [ObservableProperty] private string _description = string.Empty;

    public decimal LineTotal => Quantity * UnitPrice;

    /// <summary>Display name: uses ProductName if set, otherwise falls back to legacy ItemType.</summary>
    public string DisplayName => !string.IsNullOrWhiteSpace(ProductName) ? ProductName
        : !string.IsNullOrWhiteSpace(ItemType) ? ItemType : "Unknown";

    partial void OnQuantityChanged(int value) => OnPropertyChanged(nameof(LineTotal));
    partial void OnUnitPriceChanged(decimal value) => OnPropertyChanged(nameof(LineTotal));
}
