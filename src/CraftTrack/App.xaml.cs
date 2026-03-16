using System.Windows;
using System.Windows.Threading;
using CraftTrack.Data;
using CraftTrack.Services;
using CraftTrack.ViewModels;
using CraftTrack.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CraftTrack;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;
    private DispatcherTimer? _notificationTimer;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Ensure database is created and migrated
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CraftTrackDbContext>();
            await context.Database.MigrateAsync();
        }

        // Start notification background timer (check every 30 minutes)
        _notificationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(30)
        };
        _notificationTimer.Tick += async (s, args) =>
        {
            var notificationService = _serviceProvider.GetRequiredService<INotificationService>();
            await notificationService.GenerateDeadlineNotificationsAsync();

            var mainVm = _serviceProvider.GetRequiredService<MainViewModel>();
            await mainVm.RefreshNotificationBadgeAsync();
        };
        _notificationTimer.Start();

        // Initialize theme
        var themeService = _serviceProvider.GetRequiredService<IThemeService>();
        themeService.Initialize();

        // Initialize branding
        var brandingService = _serviceProvider.GetRequiredService<IBrandingService>();
        brandingService.Initialize();

        // Show first-run setup wizard before opening the main window
        if (!brandingService.IsSetupComplete)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var wizard = new SetupWizardWindow(_serviceProvider.GetRequiredService<SetupWizardViewModel>());
            wizard.ShowDialog();
            ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        // Show main window
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        MainWindow = mainWindow;
        mainWindow.Show();

        // Initialize main view model
        var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
        await mainViewModel.InitializeAsync();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        var dbPath = CraftTrackDbContext.GetDatabasePath();

        // Database
        services.AddDbContextFactory<CraftTrackDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));
        services.AddDbContext<CraftTrackDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // Services
        services.AddSingleton<ICustomerService, CustomerService>();
        services.AddSingleton<IInventoryService, InventoryService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<IProductService, ProductService>();
        services.AddSingleton<IOrderService, OrderService>();
        services.AddSingleton<IFinanceService, FinanceService>();
        services.AddSingleton<IRejectService, RejectService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IBrandingService, BrandingService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<OrderListViewModel>();
        services.AddTransient<OrderDetailViewModel>();
        services.AddTransient<CustomerListViewModel>();
        services.AddTransient<CustomerProfileViewModel>();
        services.AddTransient<InventoryListViewModel>();
        services.AddTransient<InventoryDetailViewModel>();
        services.AddTransient<ProductListViewModel>();
        services.AddTransient<ProductDetailViewModel>();
        services.AddTransient<NotificationCenterViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<ExpensesViewModel>();
        services.AddTransient<RejectListViewModel>();
        services.AddTransient<SetupWizardViewModel>();

        // Windows
        services.AddSingleton<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _notificationTimer?.Stop();
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}

