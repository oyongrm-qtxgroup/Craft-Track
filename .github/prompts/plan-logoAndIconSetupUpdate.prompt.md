## Plan: Logo and Icon Setup Update

Use the .ico asset for all Windows icon surfaces (installer, executable, taskbar, shortcuts) and the .png asset for sidebar logo/fallback in the wizard. Also narrow wizard customization to only sidebar logo (PNG), store name, and description, with the new defaults you specified.

**Steps**
1. Phase 1 - Align icon references for installer/app
1. Update [installer.iss](installer.iss) so `SetupIconFile` points to `src/CraftTrack/Craft Track (Icon).ico`.
2. Update [src/CraftTrack/CraftTrack.csproj](src/CraftTrack/CraftTrack.csproj) so `ApplicationIcon` and resource includes use the new icon/logo files (replace stale `default.ico` / `Assets/default.png` references).
3. Update default runtime icon URI in [src/CraftTrack/Services/BrandingService.cs](src/CraftTrack/Services/BrandingService.cs) to `pack://application:,,,/Craft Track (Icon).ico`.

1. Phase 2 - Restrict wizard branding inputs
1. Remove ICO upload/edit controls from [src/CraftTrack/Views/SetupWizardWindow.xaml](src/CraftTrack/Views/SetupWizardWindow.xaml) so the wizard only supports sidebar logo PNG + store name + description.
2. Remove ICO-related properties/commands/preview logic from [src/CraftTrack/ViewModels/SetupWizardViewModel.cs](src/CraftTrack/ViewModels/SetupWizardViewModel.cs).
3. Update wizard text labels to reflect your wording (store name + description + sidebar logo PNG only).

1. Phase 3 - Apply new defaults
1. Set default title to `Craft Track` and default description/subtitle to `Your small business helper at hand` in [src/CraftTrack/Models/BrandingSettings.cs](src/CraftTrack/Models/BrandingSettings.cs) and [src/CraftTrack/Services/BrandingService.cs](src/CraftTrack/Services/BrandingService.cs).
2. Set default sidebar logo fallback to `Assets/Craft Track (Icon).png` in [src/CraftTrack/Services/BrandingService.cs](src/CraftTrack/Services/BrandingService.cs).
3. Update setup window product naming text from `CraftTrack` to `Craft Track` where appropriate in [src/CraftTrack/Views/SetupWizardWindow.xaml](src/CraftTrack/Views/SetupWizardWindow.xaml).

1. Phase 4 - Verify behavior
1. Build app: `dotnet build src/CraftTrack/CraftTrack.csproj -c Release`.
2. Test fresh wizard without selecting logo: verify default logo/title/description are applied.
3. Test custom PNG selection: verify preview, save, and reload persistence.
4. Build installer and verify setup exe icon + installed desktop/taskbar icons all use the new ICO.

**Scope decisions**
1. Included: unify installer/app/taskbar/shortcut icons to the ICO asset, wizard logo fallback to PNG asset, and the exact default text values you gave.
2. Excluded: broader branding/settings redesign outside wizard, and unrelated installer behavior changes.

Plan is saved in /memories/session/plan.md and ready for handoff. If you approve, the next agent can implement this directly.
