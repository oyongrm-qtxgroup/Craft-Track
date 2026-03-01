using System.Windows.Controls;
using System.Windows.Input;
using CraftTrack.Models;
using CraftTrack.ViewModels;

namespace CraftTrack.Views;

public partial class CustomerListView : UserControl
{
    public CustomerListView()
    {
        InitializeComponent();
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is DataGrid grid && grid.SelectedItem is Customer customer && DataContext is CustomerListViewModel vm)
        {
            vm.ViewCustomerCommand.Execute(customer);
        }
    }
}
