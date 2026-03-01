using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CraftTrack.Models;
using CraftTrack.ViewModels;

namespace CraftTrack.Views;

public partial class OrderDetailView : UserControl
{
    public OrderDetailView()
    {
        InitializeComponent();
    }

    private void CustomerName_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (DataContext is OrderDetailViewModel vm)
        {
            vm.SearchCustomersCommand.Execute(null);
        }
    }

    private void CustomerSuggestion_Selected(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is Customer customer && DataContext is OrderDetailViewModel vm)
        {
            vm.SelectCustomerCommand.Execute(customer);
        }
    }

    private void OrderItem_Changed(object sender, TextChangedEventArgs e)
    {
        if (DataContext is OrderDetailViewModel vm)
        {
            vm.RecalculateTotal();
        }
    }

    private void ProductPickerOverlay_Click(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is OrderDetailViewModel vm)
        {
            vm.CloseProductPickerCommand.Execute(null);
        }
    }

    private void ProductPickerCard_Click(object sender, MouseButtonEventArgs e)
    {
        // Prevent click from bubbling to overlay
        e.Handled = true;
    }
}
