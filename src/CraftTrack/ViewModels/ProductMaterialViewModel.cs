using CommunityToolkit.Mvvm.ComponentModel;

namespace CraftTrack.ViewModels;

public partial class ProductMaterialViewModel : ObservableObject
{
    [ObservableProperty] private int _inventoryItemId;
    [ObservableProperty] private string _inventoryItemName = string.Empty;
    [ObservableProperty] private string _unitOfMeasure = string.Empty;
    [ObservableProperty] private decimal _quantityRequired = 1;
    [ObservableProperty] private decimal _unitCost;

    public decimal MaterialCost => QuantityRequired * UnitCost;

    partial void OnQuantityRequiredChanged(decimal value) => OnPropertyChanged(nameof(MaterialCost));
    partial void OnUnitCostChanged(decimal value) => OnPropertyChanged(nameof(MaterialCost));
}
