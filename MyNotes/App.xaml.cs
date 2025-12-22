using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Interop;
using MyNotes.Services.Database;
using MyNotes.Services.Dialog;
using MyNotes.Services.Navigation;
using MyNotes.Services.Settings;
using MyNotes.Services.Window;
using MyNotes.ViewModels;
using MyNotes.ViewModels.Dialogs;
using MyNotes.ViewModels.Navigations;

using Windows.ApplicationModel;

namespace MyNotes;

public partial class App : Application
{
  internal static App Instance => (App)Current;
  internal static string PackageFamilyName { get; } = Package.Current.Id.FamilyName;

  internal App()
  {
    InitializeComponent();

    using (var appIitializeScope = Services.CreateScope())
    {
      _ = appIitializeScope.ServiceProvider.GetRequiredService<AppDbContextInitializer>();
    }
  }

  protected override void OnLaunched(LaunchActivatedEventArgs args)
  {
    NativeMethods.SetConsole();

    var windowService = Services.GetRequiredService<WindowService>();
    var mainWindow = windowService.MainWindow;
    mainWindow.Activate();
    mainWindow.Closed += (s, e) => NativeMethods.FreeConsole();
  }

  internal ServiceProvider Services { get; } = ConfigureServices();

  private static ServiceProvider ConfigureServices()
  {
    ServiceCollection services = new();

    // ViewModel
    services.AddSingleton<MainViewModel>();
    services.AddSingleton<SettingsViewModel>();

    services.AddSingleton<NavigationViewModelFactory>();
    services.AddSingleton<DialogViewModelFactory>();

    // Service
    services.AddSingleton<DialogService>();
    services.AddSingleton<NavigationService>();
    services.AddSingleton<SettingsService>();
    services.AddSingleton<WindowService>();

    // DbContext
    services.AddSingleton<AppDbContextTaskDispatcher>();
    services.AddDbContextFactory<AppDbContext>();
    services.AddScoped<AppDbContextInitializer>();


    return services.BuildServiceProvider();
  }
}
