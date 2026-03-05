using System.Windows;
using System.Windows.Media;
using CraftTrack.Models;

namespace CraftTrack.Helpers;

/// <summary>
/// Derives all ~100 theme resources from a <see cref="ColorScheme"/> and isDark flag.
/// Uses HSL manipulation to create lighter/darker variants, background tints, gradients, etc.
/// </summary>
public static class ColorDerivationHelper
{
    /// <summary>
    /// Compute resource overrides for the given color scheme and apply them
    /// directly to <see cref="Application.Current.Resources"/>.
    /// Because views use DynamicResource, changes take effect immediately.
    /// </summary>
    public static void ApplyColorScheme(ColorScheme scheme, bool isDark)
    {
        var app = Application.Current;
        if (app is null) return;

        var accent = ParseHex(scheme.AccentColor);
        var secondary = ParseHex(scheme.SecondaryColor);
        var success = ParseHex(scheme.SuccessColor);
        var warning = ParseHex(scheme.WarningColor);
        var danger = ParseHex(scheme.DangerColor);
        var sidebar = ParseHex(scheme.SidebarColor);

        // === Core Brushes ===
        if (isDark)
        {
            Set(app, "AccentBrush", SolidBrush(Lighten(accent, 0.20)));
            Set(app, "AccentLightBrush", SolidBrush(Lighten(accent, 0.35)));
            Set(app, "AccentDarkBrush", SolidBrush(accent));
            Set(app, "AccentSecondaryBrush", SolidBrush(Lighten(secondary, 0.15)));
        }
        else
        {
            Set(app, "AccentBrush", SolidBrush(accent));
            Set(app, "AccentLightBrush", SolidBrush(Lighten(accent, 0.20)));
            Set(app, "AccentDarkBrush", SolidBrush(Darken(accent, 0.12)));
            Set(app, "AccentSecondaryBrush", SolidBrush(secondary));
        }

        // === Status ===
        if (isDark)
        {
            Set(app, "DangerBrush", SolidBrush(Lighten(danger, 0.15)));
            Set(app, "WarningBrush", SolidBrush(Lighten(warning, 0.15)));
            Set(app, "SuccessBrush", SolidBrush(Lighten(success, 0.15)));
        }
        else
        {
            Set(app, "DangerBrush", SolidBrush(danger));
            Set(app, "WarningBrush", SolidBrush(warning));
            Set(app, "SuccessBrush", SolidBrush(success));
        }

        // === Sidebar ===
        var sidebarBottom = isDark ? Darken(secondary, 0.55) : Darken(secondary, 0.35);
        var sidebarTop = isDark ? Darken(sidebar, 0.25) : sidebar;
        Set(app, "SidebarGradient", GradientBrush(sidebarTop, sidebarBottom));
        Set(app, "SidebarSectionBrush", SolidBrush(isDark ? Lighten(secondary, 0.30) : Lighten(secondary, 0.55)));
        Set(app, "SidebarIconBrush", SolidBrush(isDark ? Lighten(secondary, 0.20) : Lighten(secondary, 0.35)));
        Set(app, "SidebarSubtitleBrush", SolidBrush(isDark ? Lighten(secondary, 0.20) : secondary));

        // === DataGrid ===
        if (isDark)
        {
            Set(app, "RowHoverBrush", SolidBrush(Darken(secondary, 0.55)));
            Set(app, "RowSelectedBrush", SolidBrush(Darken(secondary, 0.45)));
        }
        else
        {
            Set(app, "RowHoverBrush", SolidBrush(Lighten(secondary, 0.85)));
            Set(app, "RowSelectedBrush", SolidBrush(Lighten(secondary, 0.75)));
        }

        // === Interactive States ===
        Set(app, "OutlinedHoverBgBrush", SolidBrush(WithAlpha(secondary, isDark ? 0.10 : 0.05)));

        // === Status Backgrounds ===
        if (isDark)
        {
            Set(app, "DangerBgBrush", SolidBrush(Darken(danger, 0.70)));
            Set(app, "DangerTextBrush", SolidBrush(Lighten(danger, 0.30)));
            Set(app, "SuccessBgBrush", SolidBrush(Darken(success, 0.70)));
            Set(app, "SuccessTextBrush", SolidBrush(Lighten(success, 0.30)));
            Set(app, "WarningBgBrush", SolidBrush(Darken(warning, 0.70)));
            Set(app, "WarningTextBrush", SolidBrush(Lighten(warning, 0.30)));
        }
        else
        {
            Set(app, "DangerBgBrush", SolidBrush(Lighten(danger, 0.85)));
            Set(app, "DangerTextBrush", SolidBrush(Darken(danger, 0.12)));
            Set(app, "SuccessBgBrush", SolidBrush(Lighten(success, 0.80)));
            Set(app, "SuccessTextBrush", SolidBrush(Darken(success, 0.15)));
            Set(app, "WarningBgBrush", SolidBrush(Lighten(warning, 0.80)));
            Set(app, "WarningTextBrush", SolidBrush(Darken(warning, 0.20)));
        }

        // === Dashboard Stat Icon Circles ===
        if (isDark)
        {
            Set(app, "StatIconBg1", SolidBrush(Darken(secondary, 0.55)));
            Set(app, "StatIconFg1", SolidBrush(Lighten(secondary, 0.20)));
            Set(app, "StatIconBg2", SolidBrush(Darken(secondary, 0.55)));
            Set(app, "StatIconFg2", SolidBrush(Lighten(secondary, 0.20)));
            Set(app, "StatIconBg3", SolidBrush(Darken(danger, 0.65)));
            Set(app, "StatIconFg3", SolidBrush(Lighten(danger, 0.20)));
            Set(app, "StatIconBg4", SolidBrush(Darken(accent, 0.60)));
            Set(app, "StatIconFg4", SolidBrush(Lighten(accent, 0.30)));
        }
        else
        {
            Set(app, "StatIconBg1", SolidBrush(Lighten(secondary, 0.85)));
            Set(app, "StatIconFg1", SolidBrush(secondary));
            Set(app, "StatIconBg2", SolidBrush(Lighten(secondary, 0.85)));
            Set(app, "StatIconFg2", SolidBrush(secondary));
            Set(app, "StatIconBg3", SolidBrush(Lighten(danger, 0.85)));
            Set(app, "StatIconFg3", SolidBrush(danger));
            Set(app, "StatIconBg4", SolidBrush(Lighten(accent, 0.80)));
            Set(app, "StatIconFg4", SolidBrush(Lighten(accent, 0.15)));
        }

        // === Financial Gradients ===
        // Revenue (based on success)
        var revStart = isDark ? Darken(success, 0.10) : success;
        var revEnd = isDark ? Darken(success, 0.25) : Darken(success, 0.12);
        Set(app, "RevenueGradient", GradientBrush(revStart, revEnd, 1.0, 1.0));
        Set(app, "RevenueSubtitleBrush", SolidBrush(isDark ? Lighten(success, 0.45) : Lighten(success, 0.70)));
        Set(app, "RevenueWatermarkBrush", SolidBrush(Lighten(success, 0.25)));

        // Expense (based on danger)
        var expStart = isDark ? danger : danger;
        var expEnd = isDark ? Darken(danger, 0.15) : Darken(danger, 0.10);
        Set(app, "ExpenseGradient", GradientBrush(expStart, expEnd, 1.0, 1.0));
        Set(app, "ExpenseSubtitleBrush", SolidBrush(isDark ? Lighten(danger, 0.40) : Lighten(danger, 0.70)));
        Set(app, "ExpenseWatermarkBrush", SolidBrush(Lighten(danger, 0.30)));

        // === P&L Card Colors ===
        if (isDark)
        {
            Set(app, "ProfitTextBrush", SolidBrush(Lighten(accent, 0.30)));
            Set(app, "ProfitBgBrush", SolidBrush(Darken(accent, 0.60)));
            Set(app, "OutstandingTextBrush", SolidBrush(Lighten(warning, 0.35)));
            Set(app, "OutstandingBgBrush", SolidBrush(Darken(warning, 0.65)));
        }
        else
        {
            Set(app, "ProfitTextBrush", SolidBrush(accent));
            Set(app, "ProfitBgBrush", SolidBrush(Lighten(accent, 0.85)));
            Set(app, "OutstandingTextBrush", SolidBrush(Darken(warning, 0.15)));
            Set(app, "OutstandingBgBrush", SolidBrush(Lighten(warning, 0.80)));
        }

        // === P&L Gradient Cards ===
        var profGStart = isDark ? Darken(accent, 0.15) : accent;
        var profGEnd = isDark ? Darken(accent, 0.30) : Darken(accent, 0.12);
        Set(app, "ProfitGradient", GradientBrush(profGStart, profGEnd, 1.0, 1.0));
        Set(app, "ProfitSubtitleBrush", SolidBrush(isDark ? Lighten(accent, 0.30) : Lighten(accent, 0.55)));
        Set(app, "ProfitWatermarkBrush", SolidBrush(Lighten(accent, 0.20)));

        var outGStart = isDark ? Darken(warning, 0.15) : Darken(warning, 0.05);
        var outGEnd = isDark ? Darken(warning, 0.30) : Darken(warning, 0.20);
        Set(app, "OutstandingGradient", GradientBrush(outGStart, outGEnd, 1.0, 1.0));
        Set(app, "OutstandingSubtitleBrush", SolidBrush(Lighten(warning, 0.45)));
        Set(app, "OutstandingWatermarkBrush", SolidBrush(Lighten(warning, 0.25)));

        // === Expense Chart Bars ===
        Set(app, "ChartBarMaterials", SolidBrush(isDark ? Lighten(accent, 0.15) : Lighten(accent, 0.10)));
        Set(app, "ChartBarTools", SolidBrush(isDark ? Lighten(secondary, 0.15) : secondary));
        Set(app, "ChartBarPackaging", SolidBrush(isDark ? Lighten(warning, 0.15) : warning));

        // === Notification ===
        if (isDark)
        {
            Set(app, "NotifHeaderBgBrush", SolidBrush(Darken(secondary, 0.55)));
            Set(app, "NotifHeaderFgBrush", SolidBrush(Lighten(secondary, 0.20)));
            Set(app, "PillNewBgBrush", SolidBrush(Darken(secondary, 0.45)));
            Set(app, "PillNewFgBrush", SolidBrush(Lighten(secondary, 0.30)));
        }
        else
        {
            Set(app, "NotifHeaderBgBrush", SolidBrush(Lighten(secondary, 0.85)));
            Set(app, "NotifHeaderFgBrush", SolidBrush(secondary));
            Set(app, "PillNewBgBrush", SolidBrush(Lighten(secondary, 0.75)));
            Set(app, "PillNewFgBrush", SolidBrush(Darken(secondary, 0.20)));
        }
        Set(app, "NotifBadgeBrush", SolidBrush(isDark ? Lighten(danger, 0.05) : danger));

        // === Severity pills ===
        Set(app, "RushColor", SolidBrush(isDark ? Lighten(danger, 0.10) : danger));
        Set(app, "HighColor", SolidBrush(isDark ? Lighten(warning, 0.10) : warning));
        Set(app, "LowColor", SolidBrush(isDark ? Lighten(success, 0.10) : success));
        Set(app, "PaidColor", SolidBrush(isDark ? Lighten(success, 0.10) : success));
        Set(app, "PartialColor", SolidBrush(isDark ? Lighten(warning, 0.10) : warning));
        Set(app, "UnpaidColor", SolidBrush(isDark ? Lighten(danger, 0.10) : danger));

        // === Customer Profile ===
        if (isDark)
        {
            Set(app, "RevenueCardBgBrush", SolidBrush(Darken(success, 0.70)));
            Set(app, "RevenueCardFgBrush", SolidBrush(Lighten(success, 0.30)));
            Set(app, "BalanceCardBgBrush", SolidBrush(Darken(danger, 0.70)));
            Set(app, "BalanceCardFgBrush", SolidBrush(Lighten(danger, 0.30)));
        }
        else
        {
            Set(app, "RevenueCardBgBrush", SolidBrush(Lighten(success, 0.80)));
            Set(app, "RevenueCardFgBrush", SolidBrush(Darken(success, 0.15)));
            Set(app, "BalanceCardBgBrush", SolidBrush(Lighten(danger, 0.80)));
            Set(app, "BalanceCardFgBrush", SolidBrush(Darken(danger, 0.10)));
        }

        // === Income / Expense header gradients ===
        Set(app, "IncomeHeaderGradient", HorizontalGradient(
            isDark ? Darken(success, 0.15) : Darken(success, 0.10),
            isDark ? success : Lighten(success, 0.10)));
        Set(app, "ExpenseHeaderGradient", HorizontalGradient(
            isDark ? Darken(danger, 0.15) : Darken(danger, 0.10),
            isDark ? danger : Lighten(danger, 0.05)));

        // === P&L gradient summary cards ===
        Set(app, "PLRevenueGradient", GradientBrush(
            isDark ? Darken(success, 0.15) : Darken(success, 0.10),
            isDark ? success : Lighten(success, 0.10), 1.0, 1.0));
        Set(app, "PLExpenseGradient", GradientBrush(
            isDark ? Darken(danger, 0.15) : Darken(danger, 0.10),
            isDark ? danger : Lighten(danger, 0.05), 1.0, 1.0));

        // === Chart Palette (Color resources, not brushes) ===
        SetColor(app, "Chart1Color", isDark ? Lighten(accent, 0.15) : Lighten(accent, 0.10));
        SetColor(app, "Chart2Color", isDark ? Lighten(secondary, 0.15) : secondary);
        SetColor(app, "Chart3Color", isDark ? Lighten(success, 0.15) : success);
        SetColor(app, "Chart4Color", isDark ? Lighten(warning, 0.15) : warning);
        SetColor(app, "Chart5Color", isDark ? Color.FromRgb(0x60, 0xA5, 0xFA) : Color.FromRgb(0x3B, 0x82, 0xF6));
        SetColor(app, "Chart6Color", isDark ? Lighten(danger, 0.15) : danger);

        // === Reject ===
        Set(app, "RejectBadgeBrush", SolidBrush(isDark ? Lighten(danger, 0.15) : Darken(danger, 0.08)));
        Set(app, "RejectBgBrush", SolidBrush(isDark ? Darken(danger, 0.70) : Lighten(danger, 0.85)));
        Set(app, "RejectTextBrush", SolidBrush(isDark ? Lighten(danger, 0.30) : Darken(danger, 0.35)));
        Set(app, "RejectGradient", GradientBrush(
            isDark ? Darken(danger, 0.05) : danger,
            isDark ? Darken(danger, 0.20) : Darken(danger, 0.10), 1.0, 1.0));

        // === Profit Positive / Negative ===
        Set(app, "ProfitPositiveBrush", SolidBrush(isDark ? Lighten(success, 0.20) : Darken(success, 0.10)));
        Set(app, "ProfitNegativeBrush", SolidBrush(isDark ? Lighten(danger, 0.15) : danger));

        // === Tab pill ===
        if (isDark)
        {
            Set(app, "TabActiveBgBrush", SolidBrush(Lighten(accent, 0.20)));
            Set(app, "TabActiveFgBrush", SolidBrush(Color.FromRgb(0x0F, 0x17, 0x2A)));
        }
        else
        {
            Set(app, "TabActiveBgBrush", SolidBrush(accent));
            Set(app, "TabActiveFgBrush", SolidBrush(Colors.White));
        }

        // === Notification due date brushes (derived from warning/danger) ===
        Set(app, "NotifOverdueBrush", SolidBrush(isDark ? Lighten(danger, 0.25) : Darken(danger, 0.08)));
        Set(app, "NotifDue1DayBrush", SolidBrush(isDark ? Lighten(warning, 0.20) : Darken(warning, 0.10)));
        Set(app, "NotifDue3DayBrush", SolidBrush(isDark ? Lighten(warning, 0.30) : Darken(warning, 0.05)));

        // === New keys for formerly-hardcoded colors ===
        // Reject subtitle on gradient cards
        Set(app, "RejectSubtitleBrush", SolidBrush(isDark ? Lighten(danger, 0.40) : Lighten(danger, 0.65)));
        // Modal overlay backdrop
        Set(app, "OverlayBackdropBrush", SolidBrush(isDark
            ? Color.FromArgb(0x99, 0x00, 0x00, 0x00)   // 60% black in dark
            : Color.FromArgb(0x80, 0x00, 0x00, 0x00))); // 50% black in light
        // Warning banner (stock warning)
        Set(app, "WarningBannerBgBrush", SolidBrush(isDark
            ? Darken(warning, 0.65)
            : Lighten(warning, 0.82)));
        Set(app, "WarningBannerFgBrush", SolidBrush(isDark
            ? Lighten(warning, 0.25)
            : Darken(warning, 0.35)));
        // KPI card value text on gradient backgrounds
        Set(app, "GradientCardTextBrush", SolidBrush(Colors.White));
    }

    // ─── HSL Helpers ────────────────────────────────────────

    private static Color Lighten(Color c, double amount)
    {
        ColorToHsl(c, out var h, out var s, out var l);
        l = Math.Min(1.0, l + amount);
        return HslToColor(h, s, l);
    }

    private static Color Darken(Color c, double amount)
    {
        ColorToHsl(c, out var h, out var s, out var l);
        l = Math.Max(0.0, l - amount);
        return HslToColor(h, s, l);
    }

    private static Color WithAlpha(Color c, double alpha)
    {
        return Color.FromArgb((byte)(alpha * 255), c.R, c.G, c.B);
    }

    private static Color ParseHex(string hex)
    {
        try
        {
            return (Color)ColorConverter.ConvertFromString(hex);
        }
        catch
        {
            return Colors.Gray;
        }
    }

    // ─── Brush Factories ────────────────────────────────────

    private static SolidColorBrush SolidBrush(Color c)
    {
        var brush = new SolidColorBrush(c);
        brush.Freeze();
        return brush;
    }

    private static LinearGradientBrush GradientBrush(Color start, Color end,
        double endX = 0.0, double endY = 1.0)
    {
        var brush = new LinearGradientBrush(start, end, new Point(0, 0), new Point(endX, endY));
        brush.Freeze();
        return brush;
    }

    private static LinearGradientBrush HorizontalGradient(Color start, Color end)
    {
        var brush = new LinearGradientBrush(start, end, new Point(0, 0), new Point(1, 0));
        brush.Freeze();
        return brush;
    }

    // ─── Resource Setters ───────────────────────────────────

    private static void Set(Application app, string key, Brush brush)
    {
        app.Resources[key] = brush;
    }

    private static void SetColor(Application app, string key, Color color)
    {
        app.Resources[key] = color;
    }

    // ─── HSL Conversion ─────────────────────────────────────

    private static void ColorToHsl(Color c, out double h, out double s, out double l)
    {
        double r = c.R / 255.0;
        double g = c.G / 255.0;
        double b = c.B / 255.0;

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double delta = max - min;

        l = (max + min) / 2.0;

        if (delta < 0.00001)
        {
            h = 0;
            s = 0;
        }
        else
        {
            s = l < 0.5 ? delta / (max + min) : delta / (2.0 - max - min);

            if (Math.Abs(max - r) < 0.00001)
                h = ((g - b) / delta) + (g < b ? 6 : 0);
            else if (Math.Abs(max - g) < 0.00001)
                h = ((b - r) / delta) + 2;
            else
                h = ((r - g) / delta) + 4;

            h /= 6.0;
        }
    }

    private static Color HslToColor(double h, double s, double l)
    {
        double r, g, b;

        if (s < 0.00001)
        {
            r = g = b = l;
        }
        else
        {
            double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            double p = 2 * l - q;
            r = HueToRgb(p, q, h + 1.0 / 3.0);
            g = HueToRgb(p, q, h);
            b = HueToRgb(p, q, h - 1.0 / 3.0);
        }

        return Color.FromRgb(
            (byte)Math.Round(r * 255),
            (byte)Math.Round(g * 255),
            (byte)Math.Round(b * 255));
    }

    private static double HueToRgb(double p, double q, double t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1.0 / 6.0) return p + (q - p) * 6 * t;
        if (t < 1.0 / 2.0) return q;
        if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;
        return p;
    }
}
