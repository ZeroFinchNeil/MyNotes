using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Interop;
using MyNotes.Services.Settings;
using MyNotes.ViewModels;

using Windows.ApplicationModel;

namespace MyNotes;

public partial class App : Application
{
  public static App Instance => (App)Current;
  public static string PackageFamilyName { get; } = Package.Current.Id.FamilyName;

  private Window? _mainWindow;

  public App()
  {
    InitializeComponent();
  }

  protected override void OnLaunched(LaunchActivatedEventArgs args)
  {
    NativeMethods.SetConsole();

    _mainWindow = new Views.Windows.MainWindow();
    _mainWindow.Activate();

    _mainWindow.Closed += (s, e) => NativeMethods.FreeConsole();
  }

  public ServiceProvider Services { get; } = ConfigureServices();

  private static ServiceProvider ConfigureServices()
  {
    ServiceCollection services = new();

    // ViewModels
    services.AddSingleton<MainViewModel>();

    // Services
    services.AddSingleton<SettingsService>();

    return services.BuildServiceProvider();
  }
}
