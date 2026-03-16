using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CraftTrack.Models;
using CraftTrack.ViewModels;
using ThemeMode = CraftTrack.Models.ThemeMode;

namespace CraftTrack.Views;

public partial class SetupWizardWindow : Window
{
    private readonly SetupWizardViewModel _viewModel;

    public SetupWizardWindow(SetupWizardViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _viewModel.RequestClose += ViewModel_RequestClose;
        DataContext = _viewModel;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        _viewModel.RequestClose -= ViewModel_RequestClose;
        base.OnClosing(e);
    }

    private void ViewModel_RequestClose(bool success)
    {
        DialogResult = success;
        Close();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void LightCard_Click(object sender, MouseButtonEventArgs e)
    {
        _viewModel.SelectedTheme = CraftTrack.Models.ThemeMode.Light;
    }

    private void DarkCard_Click(object sender, MouseButtonEventArgs e)
    {
        _viewModel.SelectedTheme = CraftTrack.Models.ThemeMode.Dark;
    }

    private void SystemCard_Click(object sender, MouseButtonEventArgs e)
    {
        _viewModel.SelectedTheme = CraftTrack.Models.ThemeMode.System;
    }

    private void PresetCard_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.Tag is string presetName)
            _viewModel.SelectedPresetName = presetName;
    }
}
