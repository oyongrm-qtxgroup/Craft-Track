using System.Windows.Controls;
using System.Windows.Input;
using CraftTrack.Models;
using CraftTrack.ViewModels;

namespace CraftTrack.Views;

public partial class CustomerProfileView : UserControl
{
    public CustomerProfileView()
    {
        InitializeComponent();
    }

    private void OrderGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is DataGrid grid && grid.SelectedItem is Order order && DataContext is CustomerProfileViewModel vm)
        {
            vm.ViewOrderCommand.Execute(order);
        }
    }
}
