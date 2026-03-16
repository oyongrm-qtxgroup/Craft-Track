# Plan: CraftTrack Branding & Setup Wizard

## TL;DR
Add an editable business branding system (name, subtitle, PNG logo, ICO icon) that drives the sidebar header, window title, taskbar icon, and desktop/Start Menu shortcuts. A 4-step setup wizard appears on first run; the same controls are added to Settings.

---

## Decisions
- **App Name**: Two separate fields — Business Name (large) + Subtitle (small)
- **Logo formats**: PNG for sidebar; separate ICO for taskbar/shortcut
- **Taskbar/Shortcut**: Runtime `Window.Icon` from ICO + auto-update `.lnk` shortcuts via WshShell COM
- **Wizard style**: Plain white card dialog (`WindowStyle="None"`, `WindowStartupLocation=CenterScreen`)
- **Wizard steps**: Welcome → Branding → Theme → Finish
- **Persistence**: New `branding.json` in `%MyDocuments%\CraftTrack\` (separate from `settings.json`)
- **Shortcut paths**: `%USERPROFILE%\Desktop\CraftTrack.lnk`, `%PUBLIC%\Desktop\CraftTrack.lnk`, `%APPDATA%\...\Start Menu\Programs\CraftTrack\CraftTrack.lnk`, `%ALLUSERSPROFILE%\...\Start Menu\Programs\CraftTrack\CraftTrack.lnk`

---

## Phase 1: New Models & Services

### Step 1 — `BrandingSettings` model
Create `src/CraftTrack/Models/BrandingSettings.cs`:
- `string AppName` (default "CraftTrack")
- `string AppSubtitle` (default "")
- `string? LogoPath` — absolute path to stored PNG (copied into app data folder)
- `string? IcoPath` — absolute path to stored ICO (copied into app data folder)
- `bool IsSetupComplete` (default false)

### Step 2 — `IBrandingService` interface
Create `src/CraftTrack/Services/IBrandingService.cs`:
- Properties: `AppName`, `AppSubtitle`, `LogoImageSource` (ImageSource?), `IcoImageSource` (BitmapFrame?), `IsSetupComplete`
- Methods: `Initialize()`, `SetBranding(name, subtitle, logoPngSourcePath?, icoSourcePath?)`, `CopyLogoToAppFolder(string)→string?`, `CopyIcoToAppFolder(string)→string?`
- Events: `BrandingChanged`

### Step 3 — `BrandingService` implementation
Create `src/CraftTrack/Services/BrandingService.cs`:
- Data folder: `Path.Combine(Environment.GetFolderPath(SpecialFolder.MyDocuments), "CraftTrack")`
- Settings file: `branding.json` in that folder
- `CopyLogoToAppFolder`: copies selected PNG to `<AppData>/logo_user.png`, returns stored path
- `CopyIcoToAppFolder`: copies selected ICO to `<AppData>/app_user.ico`, returns stored path
- `SetBranding`: saves to `branding.json`, raises `BrandingChanged`, triggers shortcut update
- Loads `BitmapImage` from stored LogoPath and `BitmapFrame` from stored IcoPath on Initialize
- Shortcut update: calls `ShortcutHelper.UpdateShortcutIcon(icoPath)` after saving ICO

### Step 4 — `ShortcutHelper`
Create `src/CraftTrack/Helpers/ShortcutHelper.cs`:
- `UpdateAllShortcuts(string icoPath)` — scans 4 standard shortcut locations for `CraftTrack.lnk`
- Uses `WshShell` late-binding COM: `Type.GetTypeFromProgID("WScript.Shell")` + `dynamic`
- Sets `shortcut.IconLocation = $"{icoPath},0"` + `shortcut.Save()`
- Wrapped in try/catch (shortcut may not exist if user skipped install option)

### Step 5 — Register in DI + Initialize
In `App.xaml.cs`:
- Register `IBrandingService → BrandingService` as `Singleton`
- Call `brandingService.Initialize()` after `themeService.Initialize()`
- Check `!brandingService.IsSetupComplete` → show `SetupWizardWindow` as dialog before `MainWindow`

---

## Phase 2: MainWindow Live Bindings

### Step 6 — `MainViewModel` branding properties
Add to `MainViewModel.cs` (depends on step 1–3):
- `[ObservableProperty] string AppName`
- `[ObservableProperty] string AppSubtitle`
- `[ObservableProperty] ImageSource? LogoImageSource`
- `[ObservableProperty] ImageSource? TaskbarIconSource`
- `[ObservableProperty] string WindowTitle` — computed as `$"{AppName} — CraftTrack"`
- Constructor: inject `IBrandingService`, load initial values, subscribe to `BrandingChanged`
- `BrandingChanged` handler: refreshes all 4 properties on UI thread

### Step 7 — `MainWindow.xaml` sidebar bindings
Replace in sidebar brand block:
- `<Border>` + `<ImageBrush ImageSource="Assets/logo.png">` → `<Image Source="{Binding LogoImageSource}" Stretch="UniformToFill"/>` inside the `<Border ClipToBounds="True">`
- `Text="Oyong's Ink"` → `Text="{Binding AppName}"`
- `Text="&amp; Print Studio"` → `Text="{Binding AppSubtitle}"`
- `Title="The Oyong's Ink..."` attribute → `Title="{Binding WindowTitle}"`
- Add `Icon="{Binding TaskbarIconSource}"` to Window element

---

## Phase 3: Setup Wizard

### Step 8 — `SetupWizardViewModel`
Create `src/CraftTrack/ViewModels/SetupWizardViewModel.cs`:
- `[ObservableProperty] int CurrentStep` (0–3)
- `[ObservableProperty] string AppName`, `AppSubtitle`
- `[ObservableProperty] string? LogoPngPath`, `string? IcoPath` (selected file paths)
- `[ObservableProperty] ImageSource? LogoPreview`
- `[ObservableProperty] ThemeMode SelectedTheme`
- `[ObservableProperty] string SelectedPresetName`
- `ObservableCollection<ColorScheme> Presets` (from ColorPresets.All)
- Commands: `NextCommand`, `BackCommand`, `BrowseLogoCommand` (OpenFileDialog PNG), `BrowseIcoCommand` (OpenFileDialog ICO), `FinishCommand`
- `FinishCommand`: calls `_brandingService.SetBranding(...)`, `_themeService.SetTheme(...)`, `_themeService.SetPreset(...)`, sets `IsSetupComplete=true`, closes window

### Step 9 — `SetupWizardWindow.xaml`
Create `src/CraftTrack/Views/SetupWizardWindow.xaml`:
- `WindowStyle="None"`, `ResizeMode="NoResize"`, `WindowStartupLocation="CenterScreen"`, ~520×620
- Outer white `Border` with `CornerRadius="12"` and subtle drop shadow
- Step indicator: 4 dots at top with active dot highlighted in accent color
- Page 0 (Welcome): App logo (default), "Welcome to CraftTrack" heading, brief desc, "Get Started" button
- Page 1 (Branding): "Set Up Your Business" — Name TextBox, Subtitle TextBox, PNG logo picker with `Image` preview, ICO picker with filename display
- Page 2 (Theme): Light/Dark/System cards (reuse SettingsView style) + scrollable preset list
- Page 3 (Finish): "You're all set!" with live mini-preview of sidebar, "Launch App" button
- Back/Next/Finish navigation buttons at bottom
- DataContext = `SetupWizardViewModel` (set from App.xaml.cs)

### Step 10 — `SetupWizardWindow.xaml.cs`
Create code-behind:
- Theme card clicks → `vm.SelectedTheme = ...`
- Preset card clicks → `vm.SelectedPresetName = ...`
- Wire `FinishCommand` result → `this.DialogResult = true; Close()`

### Step 11 — App.xaml.cs wizard launch
```csharp
if (!brandingService.IsSetupComplete) {
    var wizard = new SetupWizardWindow(sp.GetRequiredService<SetupWizardViewModel>());
    wizard.ShowDialog();
}
var mainWindow = sp.GetRequiredService<MainWindow>();
mainWindow.Show();
```

---

## Phase 4: Settings — Branding Section

### Step 12 — `SettingsViewModel` branding additions
Add to `SettingsViewModel.cs` (depends on step 1–3):
- `[ObservableProperty] string BrandingAppName`
- `[ObservableProperty] string BrandingAppSubtitle`
- `[ObservableProperty] ImageSource? BrandingLogoPreview`
- `[ObservableProperty] string? PendingLogoPngPath` (selected but not yet applied)
- `[ObservableProperty] string? PendingIcoPath`
- `[RelayCommand] BrowseLogo()` — OpenFileDialog filter PNG
- `[RelayCommand] BrowseIco()` — OpenFileDialog filter ICO
- `[RelayCommand] ApplyBranding()` — calls `_brandingService.SetBranding(...)`, shows saved confirmation
- Initialize from `_brandingService` on construction

### Step 13 — `SettingsView.xaml` Branding section
Add a new `ModernCard` section "Business Branding" as the first card (before Appearance):
- Business Name field (label + TextBox bound to `BrandingAppName`)
- Subtitle field (label + TextBox bound to `BrandingAppSubtitle`)
- Logo (PNG): `Image` preview (48×48 circle) + "Browse..." button + filename label
- Taskbar/Shortcut Icon (ICO): filename label + "Browse..." button
- "Apply Changes" button (`ModernPrimaryButton` style) → `ApplyBrandingCommand`
- Small note: "Taskbar icon and shortcuts will also be updated"

---

## Phase 5: Shortcut Update (parallel with Phase 4)

Already covered in Steps 3 & 4 — `ShortcutHelper` is called from `BrandingService.SetBranding()`.

---

## Relevant Files

**New files to create:**
- `src/CraftTrack/Models/BrandingSettings.cs`
- `src/CraftTrack/Services/IBrandingService.cs`
- `src/CraftTrack/Services/BrandingService.cs`
- `src/CraftTrack/Helpers/ShortcutHelper.cs`
- `src/CraftTrack/ViewModels/SetupWizardViewModel.cs`
- `src/CraftTrack/Views/SetupWizardWindow.xaml` + `.xaml.cs`

**Files to modify:**
- `src/CraftTrack/Models/AppSettings.cs` — no change (using separate BrandingSettings)
- `src/CraftTrack/ViewModels/MainViewModel.cs` — add branding properties
- `src/CraftTrack/MainWindow.xaml` — bind sidebar name/logo/title
- `src/CraftTrack/App.xaml.cs` — register BrandingService, wizard launch logic
- `src/CraftTrack/Views/SettingsView.xaml` — add Branding section
- `src/CraftTrack/ViewModels/SettingsViewModel.cs` — add branding properties/commands

---

## Verification Steps

1. Build succeeds: `dotnet build src/CraftTrack/CraftTrack.csproj`
2. First run (delete `branding.json`): wizard appears → complete all steps → app launches with entered brand
3. Sidebar shows entered name, subtitle, and selected logo
4. `Window.Title` and taskbar icon update to reflect new ICO
5. `CraftTrack.lnk` shortcut icon updated (if shortcut exists)
6. Settings > Business Branding: change name/logo/ico, click Apply → sidebar updates live
7. Repeat app launch: wizard does NOT appear (IsSetupComplete=true)

---

## Further Considerations

1. **Fallback logo**: If no logo is set, should the sidebar show the bundled `Assets/default.png` or a monogram/initials placeholder? Recommend: fall back to Assets/default.png so the app always looks finished.
2. **Wizard cancellation**: If user closes the wizard without finishing, should the app still launch (skipping wizard) or exit? Recommend: still launch, but mark as incomplete so the wizard shows again next time.
3. **Logo in wizard finish preview**: The "finish" step shows a mini sidebar preview — this could match the `SidebarGradient` brush or be simplified to just a colored box. Recommend: use actual SidebarGradient dynamic resource so it reflects the chosen theme preset.
