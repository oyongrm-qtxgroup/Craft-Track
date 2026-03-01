using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CraftTrack.Helpers;
using CraftTrack.Models;
using CraftTrack.Services;
using Microsoft.Win32;

namespace CraftTrack.ViewModels;

public partial class ProductDetailViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private readonly IInventoryService _inventoryService;
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty] private int _productId;
    [ObservableProperty] private bool _isNewProduct = true;
    [ObservableProperty] private string _productName = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private decimal _defaultPrice;
    [ObservableProperty] private decimal _laborCost;
    [ObservableProperty] private decimal _overheadCost;
    [ObservableProperty] private string? _imagePath;
    [ObservableProperty] private BitmapImage? _imagePreview;
    [ObservableProperty] private string _errorMessage = string.Empty;

    [ObservableProperty] private ObservableCollection<ProductMaterialViewModel> _materials = [];
    [ObservableProperty] private ObservableCollection<InventoryItem> _inventorySuggestions = [];
    [ObservableProperty] private string _inventorySearchTerm = string.Empty;
    [ObservableProperty] private bool _isInventoryDropdownOpen;

    // Temp path for new image before save
    private string? _pendingImageSourcePath;

    public ProductDetailViewModel(
        IProductService productService,
        IInventoryService inventoryService,
        MainViewModel mainViewModel)
    {
        _productService = productService;
        _inventoryService = inventoryService;
        _mainViewModel = mainViewModel;
    }

    public void InitializeNewProduct()
    {
        IsNewProduct = true;
        ProductId = 0;
        ProductName = string.Empty;
        Description = string.Empty;
        DefaultPrice = 0;
        LaborCost = 0;
        OverheadCost = 0;
        ImagePath = null;
        ImagePreview = null;
        Materials = [];
        ErrorMessage = string.Empty;
    }

    public async Task LoadProductAsync(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null) return;

        IsNewProduct = false;
        ProductId = product.Id;
        ProductName = product.Name;
        Description = product.Description ?? string.Empty;
        DefaultPrice = product.DefaultPrice;
        LaborCost = product.LaborCost;
        OverheadCost = product.OverheadCost;
        ImagePath = product.ImagePath;
        LoadImagePreview(product.ImagePath);

        Materials = new ObservableCollection<ProductMaterialViewModel>(
            product.ProductMaterials.Select(m => new ProductMaterialViewModel
            {
                InventoryItemId = m.InventoryItemId,
                InventoryItemName = m.InventoryItem.Name,
                UnitOfMeasure = m.InventoryItem.UnitOfMeasure ?? string.Empty,
                QuantityRequired = m.QuantityRequired,
                UnitCost = m.InventoryItem.UnitCost
            }));
    }

    private void LoadImagePreview(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || !ImageHelper.ImageExists(relativePath))
        {
            ImagePreview = null;
            return;
        }

        try
        {
            var fullPath = ImageHelper.GetFullImagePath(relativePath);
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(fullPath, UriKind.Absolute);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.DecodePixelWidth = 400;
            bmp.EndInit();
            bmp.Freeze();
            ImagePreview = bmp;
        }
        catch
        {
            ImagePreview = null;
        }
    }

    [RelayCommand]
    private void BrowseImage()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select Product Image",
            Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
            Multiselect = false
        };

        if (dialog.ShowDialog() == true)
        {
            _pendingImageSourcePath = dialog.FileName;

            // Show preview immediately
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(dialog.FileName, UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.DecodePixelWidth = 400;
                bmp.EndInit();
                bmp.Freeze();
                ImagePreview = bmp;
            }
            catch
            {
                ImagePreview = null;
            }
        }
    }

    [RelayCommand]
    private void RemoveImage()
    {
        _pendingImageSourcePath = null;
        ImagePath = null;
        ImagePreview = null;
    }

    [RelayCommand]
    private async Task SearchInventory()
    {
        if (string.IsNullOrWhiteSpace(InventorySearchTerm) || InventorySearchTerm.Length < 1)
        {
            // Show all items when search is empty/short
            var allItems = await _inventoryService.GetAllItemsAsync();
            InventorySuggestions = new ObservableCollection<InventoryItem>(
                allItems.Where(i => !Materials.Any(m => m.InventoryItemId == i.Id)));
            IsInventoryDropdownOpen = InventorySuggestions.Count > 0;
            return;
        }

        var items = await _inventoryService.GetAllItemsAsync();
        var filtered = items
            .Where(i => i.Name.Contains(InventorySearchTerm, StringComparison.OrdinalIgnoreCase)
                     && !Materials.Any(m => m.InventoryItemId == i.Id))
            .ToList();

        InventorySuggestions = new ObservableCollection<InventoryItem>(filtered);
        IsInventoryDropdownOpen = filtered.Count > 0;
    }

    [RelayCommand]
    private void AddMaterial(InventoryItem item)
    {
        if (Materials.Any(m => m.InventoryItemId == item.Id)) return;

        Materials.Add(new ProductMaterialViewModel
        {
            InventoryItemId = item.Id,
            InventoryItemName = item.Name,
            UnitOfMeasure = item.UnitOfMeasure ?? string.Empty,
            QuantityRequired = 1,
            UnitCost = item.UnitCost
        });

        InventorySearchTerm = string.Empty;
        IsInventoryDropdownOpen = false;
    }

    [RelayCommand]
    private void RemoveMaterial(ProductMaterialViewModel material)
    {
        Materials.Remove(material);
    }

    [RelayCommand]
    private async Task SaveProduct()
    {
        ErrorMessage = string.Empty;

        // Validation
        if (string.IsNullOrWhiteSpace(ProductName))
        {
            ErrorMessage = "Product name is required.";
            return;
        }

        if (!Materials.Any())
        {
            ErrorMessage = "At least one material is required in the Bill of Materials.";
            return;
        }

        if (Materials.Any(m => m.QuantityRequired <= 0))
        {
            ErrorMessage = "All material quantities must be greater than zero.";
            return;
        }

        if (DefaultPrice <= 0)
        {
            ErrorMessage = "Default price must be greater than zero.";
            return;
        }

        // Handle image
        string? savedImagePath = ImagePath;
        if (_pendingImageSourcePath != null)
        {
            // Delete old image if replacing
            if (!IsNewProduct && !string.IsNullOrWhiteSpace(ImagePath))
                ImageHelper.DeleteProductImage(ImagePath);

            savedImagePath = ImageHelper.SaveProductImage(_pendingImageSourcePath);
        }

        var product = new Product
        {
            Id = ProductId,
            Name = ProductName.Trim(),
            Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
            ImagePath = savedImagePath,
            DefaultPrice = DefaultPrice,
            LaborCost = LaborCost,
            OverheadCost = OverheadCost,
            ProductMaterials = Materials.Select(m => new ProductMaterial
            {
                InventoryItemId = m.InventoryItemId,
                QuantityRequired = m.QuantityRequired
            }).ToList()
        };

        if (IsNewProduct)
            await _productService.CreateProductAsync(product);
        else
            await _productService.UpdateProductAsync(product);

        _mainViewModel.NavigateTo("Products");
    }

    [RelayCommand]
    private void Cancel()
    {
        _mainViewModel.NavigateTo("Products");
    }
}
