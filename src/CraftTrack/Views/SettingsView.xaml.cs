using System.Windows;
using System.Windows.Input;
using CraftTrack.Models;
using CraftTrack.ViewModels;
using ThemeMode = CraftTrack.Models.ThemeMode;

namespace CraftTrack.Views;

public partial class SettingsView
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void LightCard_Click(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            vm.SelectedTheme = ThemeMode.Light;
    }

    private void DarkCard_Click(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            vm.SelectedTheme = ThemeMode.Dark;
    }

    private void SystemCard_Click(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            vm.SelectedTheme = ThemeMode.System;
    }
}
