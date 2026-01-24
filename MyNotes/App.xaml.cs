using Microsoft.Extensions.DependencyInjection;

using MyNotes.Debugging;
using MyNotes.Services.Commands;
using MyNotes.Services.Database;
using MyNotes.Services.Dialogs;
using MyNotes.Services.Navigations;
using MyNotes.Services.Notes;
using MyNotes.Services.Search;
using MyNotes.Services.Settings;
using MyNotes.Services.Window;
using MyNotes.ViewModels;
using MyNotes.ViewModels.Dialogs;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Notes;

namespace MyNotes;

public sealed partial class App : Application, IDisposable
{
  internal static App Instance => (App)Current;
  internal static readonly string PackageFamilyName = "ZeroFinchNeil.MyNotesbyZeroFinchNeil_trdr6c7cjqx0g";

  internal App()
  {
    InitializeComponent();
    this.UnhandledException += App_UnhandledException;

    using (var appIitializeScope = Services.CreateScope())
    {
      _ = appIitializeScope.ServiceProvider.GetRequiredService<AppDbContextInitializer>();
      _ = appIitializeScope.ServiceProvider.GetRequiredService<SearchService>();
    }
  }

  private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
  {
    Console.WriteLine("{0} ({1}): {2}", "Unhandled Exception", e.Exception, e.Message);
  }

  protected override void OnLaunched(LaunchActivatedEventArgs args)
  {
    var windowService = Services.GetRequiredService<WindowService>();
    var mainWindow = windowService.MainWindow;
    mainWindow.Activate();
  }

  public void OpenDebugWindow()
  {
    new DebugWindow().Activate();
    var windowService = Services.GetRequiredService<WindowService>();
    var mainWindow = windowService.MainWindow;
    mainWindow.Activate();
  }

  internal ServiceProvider Services { get; } = ConfigureServices();

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
    services.AddSingleton<DialogService>();
    services.AddSingleton<NavigationService>();
    services.AddSingleton<SettingsService>();
    services.AddSingleton<WindowService>();
    services.AddSingleton<NoteService>();
    services.AddSingleton<SearchService>();

    services.AddKeyedSingleton<ICommandService, NavigationViewModelCommandService>(CommandServiceType.NavigationViewModel);
    services.AddKeyedSingleton<ICommandService, NoteViewModelCommandService>(CommandServiceType.NoteViewModel);

    //services.AddSingleton<CommandServiceFactory>(sp => new()
    //{
    //  ResolveMap = new Dictionary<CommandServiceType, ICommandService?>()
    //  {
    //    { CommandServiceType.NavigationViewModel, sp.GetRequiredKeyedService<ICommandService>(CommandServiceType.NavigationViewModel) },
    //    { CommandServiceType.NoteViewModel, sp.GetRequiredKeyedService<ICommandService>(CommandServiceType.NoteViewModel) }
    //  }
    //});

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
