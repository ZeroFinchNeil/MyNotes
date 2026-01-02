using Microsoft.Extensions.DependencyInjection;

using MyNotes.Debugging;
using MyNotes.Services.Commands;
using MyNotes.Services.Database;
using MyNotes.Services.Dialog;
using MyNotes.Services.Navigation;
using MyNotes.Services.Settings;
using MyNotes.Services.Window;
using MyNotes.ViewModels;
using MyNotes.ViewModels.Dialogs;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Notes;
using MyNotes.Views.Windows;

using Windows.ApplicationModel;

namespace MyNotes;

public sealed partial class App : Application, IDisposable
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
#if DEBUG
    new DebugWindow().Activate();
#endif
    var windowService = Services.GetRequiredService<WindowService>();
    var mainWindow = new MainWindow();
    mainWindow.Activate();
    windowService.MainWindow = new WeakReference<MainWindow>(mainWindow);
  }

  internal ServiceProvider Services { get; } = ConfigureServices();

  private static ServiceProvider ConfigureServices()
  {
    ServiceCollection services = new();

    // ViewModel
    services.AddScoped<MainViewModel>();
    services.AddSingleton<SettingsViewModel>();

    services.AddSingleton<NavigationViewModelProvider>();
    services.AddSingleton<DialogViewModelFactory>();
    services.AddSingleton<NoteViewModelProvider>();

    // Service
    services.AddSingleton<DialogService>();
    services.AddSingleton<NavigationService>();
    services.AddSingleton<SettingsService>();
    services.AddSingleton<WindowService>();

    services.AddKeyedSingleton<ICommandService, NavigationCommandService>(CommandServiceType.Navigation);
    services.AddSingleton<CommandServiceFactory>(sp => new(sp)
    {
      ResolveMap = new Dictionary<CommandServiceType, ICommandService?>()
      {
        { CommandServiceType.Navigation, sp.GetKeyedService<ICommandService>(CommandServiceType.Navigation) }
      }
    });

    // DbContext
    services.AddSingleton<AppDbContextTaskDispatcher>();
    services.AddDbContextFactory<AppDbContext>();
    services.AddScoped<AppDbContextInitializer>();


    return services.BuildServiceProvider();
  }

  private bool _disposed;

  private void Dispose(bool disposing)
  {
    if (!_disposed)
    {
      if (disposing)
      {
        Services.Dispose();
        Console.WriteLine("{0}: {1}", "App Closing...", "");
      }
      _disposed = true;
    }
  }

  public void Dispose()
  {
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }
}
