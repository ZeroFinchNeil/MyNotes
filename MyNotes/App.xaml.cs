using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Interop;
using MyNotes.Services.Database;
using MyNotes.Services.Settings;
using MyNotes.ViewModels;

using Windows.ApplicationModel;

namespace MyNotes;

public partial class App : Application
{
  internal static App Instance => (App)Current;
  internal static string PackageFamilyName { get; } = Package.Current.Id.FamilyName;

  private Window? _mainWindow;

  internal App()
  {
    InitializeComponent();

    using(var appIitializeScope = Services.CreateScope())
    {
      _ = appIitializeScope.ServiceProvider.GetRequiredService<AppDbContextInitializer>();
    }
  }

  protected override void OnLaunched(LaunchActivatedEventArgs args)
  {
    NativeMethods.SetConsole();

    _mainWindow = new Views.Windows.MainWindow();
    _mainWindow.Activate();

    _mainWindow.Closed += (s, e) => NativeMethods.FreeConsole();
  }

  internal ServiceProvider Services { get; } = ConfigureServices();

  private static ServiceProvider ConfigureServices()
  {
    ServiceCollection services = new();

    // ViewModels
    services.AddSingleton<MainViewModel>();
    services.AddSingleton<SettingsViewModel>();

    // Services
    services.AddSingleton<SettingsService>();
    services.AddSingleton<AppDbContextTaskDispatcher>();
    services.AddDbContextFactory<AppDbContext>();
    services.AddScoped<AppDbContextInitializer>();

    return services.BuildServiceProvider();
  }
}
