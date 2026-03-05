using System.Windows.Controls;
using CraftTrack.ViewModels;

namespace CraftTrack.Views;

public partial class ExpensesView : UserControl
{
    public ExpensesView()
    {
        InitializeComponent();
        Loaded += async (s, e) =>
        {
            if (DataContext is ExpensesViewModel vm)
                await vm.InitializeAsync();
        };
    }
}
