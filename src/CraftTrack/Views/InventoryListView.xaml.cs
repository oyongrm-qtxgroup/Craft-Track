using System.Windows.Controls;
using System.Windows.Input;
using CraftTrack.Models;
using CraftTrack.ViewModels;

namespace CraftTrack.Views;

public partial class InventoryListView : UserControl
{
    public InventoryListView()
    {
        InitializeComponent();
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is DataGrid grid && grid.SelectedItem is InventoryItem item && DataContext is InventoryListViewModel vm)
        {
            vm.EditItemCommand.Execute(item);
        }
    }
}
