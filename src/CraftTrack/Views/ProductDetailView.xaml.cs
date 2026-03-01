using System.Windows.Controls;
using CraftTrack.Models;
using CraftTrack.ViewModels;

namespace CraftTrack.Views;

public partial class ProductDetailView : UserControl
{
    public ProductDetailView()
    {
        InitializeComponent();
    }

    private void MaterialSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (DataContext is ProductDetailViewModel vm)
        {
            vm.SearchInventoryCommand.Execute(null);
        }
    }

    private void InventorySuggestion_Selected(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is InventoryItem item && DataContext is ProductDetailViewModel vm)
        {
            vm.AddMaterialCommand.Execute(item);
        }
    }
}
