using Microsoft.Extensions.DependencyInjection;

using MyNotes.Debugging;
using MyNotes.Services.Commands;
using MyNotes.Services.Database;
using MyNotes.Services.Dialogs;
using MyNotes.Services.Navigations;
using MyNotes.Services.Notes;
using MyNotes.Services.Settings;
using MyNotes.Services.Window;
using MyNotes.ViewModels;
using MyNotes.ViewModels.Dialogs;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Notes;

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
    // XAML 라이브 미리 보기를 활성화하려면 창을 하나만 띄워야 함
#if DEBUG
    new DebugWindow().Activate();
#endif
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
    services.AddSingleton<SettingsViewModel>();

    services.AddSingleton<NavigationViewModelProvider>();
    services.AddSingleton<DialogViewModelFactory>();
    services.AddSingleton<NoteViewModelProvider>();

    // Service
    services.AddSingleton<DialogService>();
    services.AddSingleton<NavigationService>();
    services.AddSingleton<SettingsService>();
    services.AddSingleton<WindowService>();
    services.AddSingleton<NoteService>();

    services.AddKeyedSingleton<ICommandService, NavigationViewModelCommandService>(CommandServiceType.NavigationViewModel);
    services.AddKeyedSingleton<ICommandService, NoteViewModelCommandService>(CommandServiceType.NoteViewModel);
    services.AddSingleton<CommandServiceFactory>(sp => new(sp)
    {
      ResolveMap = new Dictionary<CommandServiceType, ICommandService?>()
      {
        { CommandServiceType.NavigationViewModel, sp.GetRequiredKeyedService<ICommandService>(CommandServiceType.NavigationViewModel) },
        { CommandServiceType.NoteViewModel, sp.GetRequiredKeyedService<ICommandService>(CommandServiceType.NoteViewModel) }
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
