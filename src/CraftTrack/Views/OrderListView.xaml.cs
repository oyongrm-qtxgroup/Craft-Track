using System.Windows.Controls;
using System.Windows.Input;
using CraftTrack.Models;
using CraftTrack.ViewModels;

namespace CraftTrack.Views;

public partial class OrderListView : UserControl
{
    public OrderListView()
    {
        InitializeComponent();
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is DataGrid grid && grid.SelectedItem is Order order && DataContext is OrderListViewModel vm)
        {
            vm.EditOrderCommand.Execute(order);
        }
    }
}
