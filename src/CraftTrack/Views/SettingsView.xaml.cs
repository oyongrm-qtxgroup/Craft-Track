using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using CraftTrack.Models;
using CraftTrack.ViewModels;
using ThemeMode = CraftTrack.Models.ThemeMode;

namespace CraftTrack.Views;

public partial class SettingsView
{
    private Action<Color>? _colorPickerApplyAction;

    public SettingsView()
    {
        InitializeComponent();
    }

    // ── Theme card clicks ──

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

    // ── Preset card click ──

    private void PresetCard_Click(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is SettingsViewModel vm && sender is FrameworkElement fe && fe.Tag is string name)
            vm.SelectedPresetName = name;
    }

    // ── Color picker helpers ──

    private void OpenColorPicker(Color currentColor, Action<Color> onApply)
    {
        _colorPickerApplyAction = onApply;
        ColorPicker.Color = currentColor;
        ColorPickerPopup.IsOpen = true;
    }

    private void ColorPickerApply_Click(object sender, RoutedEventArgs e)
    {
        _colorPickerApplyAction?.Invoke(ColorPicker.Color);
        ColorPickerPopup.IsOpen = false;
    }

    private void ColorPickerCancel_Click(object sender, RoutedEventArgs e)
    {
        ColorPickerPopup.IsOpen = false;
    }

    // ── Individual color clicks ──

    private void AccentColor_Click(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            OpenColorPicker(vm.AccentColor, c => vm.AccentColor = c);
    }

    private void SecondaryColor_Click(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            OpenColorPicker(vm.SecondaryColor, c => vm.SecondaryColor = c);
    }

    private void SuccessColor_Click(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            OpenColorPicker(vm.SuccessColor, c => vm.SuccessColor = c);
    }

    private void WarningColor_Click(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            OpenColorPicker(vm.WarningColor, c => vm.WarningColor = c);
    }

    private void DangerColor_Click(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            OpenColorPicker(vm.DangerColor, c => vm.DangerColor = c);
    }

    private void SidebarColor_Click(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            OpenColorPicker(vm.SidebarColor, c => vm.SidebarColor = c);
    }
}
