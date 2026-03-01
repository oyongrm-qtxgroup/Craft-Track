using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CraftTrack.Helpers;
using CraftTrack.Models;
using CraftTrack.Services;

namespace CraftTrack.ViewModels;

public partial class ProductListViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty] private ObservableCollection<Product> _products = [];
    [ObservableProperty] private string _searchTerm = string.Empty;
    [ObservableProperty] private bool _isLoading;

    public ProductListViewModel(IProductService productService, MainViewModel mainViewModel)
    {
        _productService = productService;
        _mainViewModel = mainViewModel;
        _ = LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        IsLoading = true;
        try
        {
            var products = string.IsNullOrWhiteSpace(SearchTerm)
                ? await _productService.GetAllProductsAsync()
                : await _productService.SearchProductsAsync(SearchTerm);

            Products = new ObservableCollection<Product>(products);
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSearchTermChanged(string value) => _ = LoadProductsAsync();

    [RelayCommand]
    private void AddProduct()
    {
        _mainViewModel.NavigateToProductDetail();
    }

    [RelayCommand]
    private void EditProduct(Product product)
    {
        _mainViewModel.NavigateToProductDetail(product.Id);
    }

    [RelayCommand]
    private async Task ArchiveProduct(Product product)
    {
        await _productService.ArchiveProductAsync(product.Id);
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task DeleteProduct(Product product)
    {
        var canDelete = await _productService.CanDeleteProductAsync(product.Id);
        if (canDelete)
        {
            // Delete image if exists
            ImageHelper.DeleteProductImage(product.ImagePath);
            await _productService.DeleteProductAsync(product.Id);
        }
        else
        {
            // Archive instead (product has orders)
            await _productService.ArchiveProductAsync(product.Id);
        }
        await LoadProductsAsync();
    }

    public string GetProductImageFullPath(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || !ImageHelper.ImageExists(relativePath))
            return string.Empty;
        return ImageHelper.GetFullImagePath(relativePath);
    }
}
