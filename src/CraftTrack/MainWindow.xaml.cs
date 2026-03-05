using System.Windows;
using System.Windows.Input;
using CraftTrack.ViewModels;

namespace CraftTrack;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    private void NavDashboard_Click(object sender, MouseButtonEventArgs e) => _viewModel.NavigateToCommand.Execute("Dashboard");
    private void NavOrders_Click(object sender, MouseButtonEventArgs e) => _viewModel.NavigateToCommand.Execute("Orders");
    private void NavCustomers_Click(object sender, MouseButtonEventArgs e) => _viewModel.NavigateToCommand.Execute("Customers");
    private void NavInventory_Click(object sender, MouseButtonEventArgs e) => _viewModel.NavigateToCommand.Execute("Inventory");
    private void NavProducts_Click(object sender, MouseButtonEventArgs e) => _viewModel.NavigateToCommand.Execute("Products");
    private void NavExpenses_Click(object sender, MouseButtonEventArgs e) => _viewModel.NavigateToCommand.Execute("Expenses");
    private void NavRejects_Click(object sender, MouseButtonEventArgs e) => _viewModel.NavigateToCommand.Execute("Rejects");
    private void NavNotifications_Click(object sender, MouseButtonEventArgs e) => _viewModel.NavigateToCommand.Execute("Notifications");
    private void NavSettings_Click(object sender, MouseButtonEventArgs e) => _viewModel.NavigateToCommand.Execute("Settings");
}