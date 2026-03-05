using System.Windows;
using System.Windows.Controls;
using CraftTrack.ViewModels;

namespace CraftTrack.Views;

public partial class RejectListView : UserControl
{
    public RejectListView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is RejectListViewModel vm)
            await vm.InitializeAsync();
    }
}
