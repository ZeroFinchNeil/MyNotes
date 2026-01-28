using Microsoft.Extensions.DependencyInjection;

using MyNotes.Debugging;
using MyNotes.Services.Commands;
using MyNotes.Services.Database;
using MyNotes.Services.Dialogs;
using MyNotes.Services.Logging;
using MyNotes.Services.Navigations;
using MyNotes.Services.Notes;
using MyNotes.Services.Search;
using MyNotes.Services.Settings;
using MyNotes.Services.Windows;
using MyNotes.ViewModels;
using MyNotes.ViewModels.Dialogs;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Notes;

using Windows.ApplicationModel;

namespace MyNotes;

public sealed partial class App : Application, IDisposable
{
  internal static App Instance => (App)Current;
  internal static readonly string PackageFamilyName = Package.Current.Id.FamilyName;
  internal static ServiceProvider Services { get; } = ConfigureServices();

  internal App()
  {
    InitializeComponent();

    this.UnhandledException += App_UnhandledException;
    AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

    using (var appIitializeScope = Services.CreateScope())
    {
      _ = appIitializeScope.ServiceProvider.GetRequiredService<AppDbContextInitializer>();
      _ = appIitializeScope.ServiceProvider.GetRequiredService<SearchService>();
    }
  }

  private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e) => WriteExcptionLog(e.Exception);

  private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
  {
    if (e.ExceptionObject is Exception ex)
      WriteExcptionLog(ex);
  }

  private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e) => WriteExcptionLog(e.Exception);

  private void WriteExcptionLog(Exception ex)
  {
    Console.WriteLine("{0}: {1}", "Exception", ex);
    var loggingService = Services.GetRequiredService<LoggingService>();
    loggingService.Write(ex);
  }

  protected override void OnLaunched(LaunchActivatedEventArgs args)
  {
    var windowService = Services.GetRequiredService<WindowService>();
    var mainWindow = windowService.GetOrCreateMainWindow();
    mainWindow.Activate();
#if DEBUG
    if (Debugger.IsAttached)
    {
      _ = OpenDebugWindow();
    }
#endif
  }

  public async Task OpenDebugWindow()
  {
    await Task.Delay(1000);
    new DebugWindow().Activate();
    var windowService = Services.GetRequiredService<WindowService>();
    windowService.MainWindow?.Activate();
  }

  private static ServiceProvider ConfigureServices()
  {
    ServiceCollection services = new();

    // ViewModel
    services.AddScoped<MainViewModel>();
    services.AddScoped<SettingsViewModel>();

    services.AddSingleton<NavigationViewModelProvider>();
    services.AddSingleton<DialogViewModelFactory>();
    services.AddSingleton<NoteViewModelProvider>();
    services.AddSingleton<NoteListViewModelProvider>();

    // Service
    services.AddSingleton<LoggingService>();
    services.AddSingleton<DialogService>();
    services.AddSingleton<NavigationService>();
    services.AddSingleton<SettingsService>();
    services.AddSingleton<WindowService>();
    services.AddSingleton<NoteService>();
    services.AddSingleton<SearchService>();

    services.AddKeyedSingleton<ICommandService, NavigationViewModelCommandService>(CommandServiceType.NavigationViewModel);
    services.AddKeyedSingleton<ICommandService, NoteViewModelCommandService>(CommandServiceType.NoteViewModel);

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
        this.UnhandledException -= App_UnhandledException;
        AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
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
