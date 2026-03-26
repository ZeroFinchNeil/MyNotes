using System.IO.Pipes;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.AppConstants;
using MyNotes.Debugging;
using MyNotes.Models.Navigations;
using MyNotes.Services.App;
using MyNotes.Services.Commands;
using MyNotes.Services.Database;
using MyNotes.Services.Dialogs;
using MyNotes.Services.Logging;
using MyNotes.Services.Navigations;
using MyNotes.Services.Notes;
using MyNotes.Services.Search;
using MyNotes.Services.Settings;
using MyNotes.ViewModels;
using MyNotes.ViewModels.Dialogs;
using MyNotes.ViewModels.Media;
using MyNotes.ViewModels.Media.Providers;
using MyNotes.ViewModels.Navigations.Providers;
using MyNotes.ViewModels.Notes.Providers;

namespace MyNotes;

public sealed partial class App : Application, IDisposable
{
  internal static App Instance => (App)Current;

  #region Object Lifetime Management
  internal App()
  {
    InitializeComponent();

    this.UnhandledException += App_UnhandledException;
    AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

    _ = InitializeServicesAsync();
  }

  private async Task InitializeServicesAsync()
  {
    using var appInitializeScope = Services.CreateScope();
    _ = appInitializeScope.ServiceProvider.GetRequiredService<AppDbContextInitializer>();
    _ = appInitializeScope.ServiceProvider.GetRequiredService<SearchService>();

    var navigationService = appInitializeScope.ServiceProvider.GetRequiredService<NavigationService>();
    await navigationService.InitializationTask;

    var noteService = appInitializeScope.ServiceProvider.GetRequiredService<NoteService>();
    await noteService.InitializationTask;

    InitializationTCS.TrySetResult();
  }

  private readonly TaskCompletionSource InitializationTCS = new();
  public Task InitializationTask => InitializationTCS.Task;

  protected override async void OnLaunched(LaunchActivatedEventArgs args)
  {
    await InitializationTask;

    _ = LaunchArgumentsPipeServerStreamAsync();
    var windowService = Services.GetRequiredService<WindowService>();
    var noteService = Services.GetRequiredService<NoteService>();
    var settingsService = Services.GetRequiredService<SettingsService>();

    AppActivationArguments appActivationArguments = AppInstance.GetCurrent().GetActivatedEventArgs();
    switch (appActivationArguments.Kind)
    {
      case ExtendedActivationKind.Launch or ExtendedActivationKind.StartupTask:
        var noteWindowsCount = await noteService.OpenNoteWindowsForOpenEntities();
        if (noteWindowsCount == 0 || settingsService.Load(AppSettingsDescriptors.IsMainWindowOpen))
        {
          var mainWindow = await windowService.GetOrCreateMainWindow();
          mainWindow.Activate();
        }
        break;
      case ExtendedActivationKind.File:
        break;
      case ExtendedActivationKind.Protocol:
        break;
      default:
        break;
    }

#if DEBUG
    if (Debugger.IsAttached)
    {
      _ = OpenDebugWindow();
    }
#endif
  }

  public bool Disposed { get; private set; }

  private void Dispose(bool disposing)
  {
    if (!Disposed)
    {
      if (disposing)
      {
        Services.Dispose();
        this.UnhandledException -= App_UnhandledException;
        AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
      }
      Disposed = true;
    }
  }

  public void Dispose()
  {
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }
  #endregion

  internal static ServiceProvider Services { get; } = ConfigureServices();

  private static ServiceProvider ConfigureServices()
  {
    ServiceCollection services = new();

    // ViewModel
    services.AddScoped<MainViewModel>();
    services.AddScoped<SettingsViewModel>();

    services.AddSingleton<NavigationViewModelProvider>();
    services.AddSingleton<DialogViewModelFactory>();
    services.AddSingleton<NoteViewModelProvider>();
    services.AddSingleton<NoteEditorViewModelProvider>();
    services.AddSingleton<NoteListViewModelProvider>();

    services.AddSingleton<ImageViewModelProvider>();
    services.AddSingleton<ImageCollectionViewModelProvider>();

    // Service
    services.AddSingleton<WindowService>();
    services.AddSingleton<JumpListService>();
    services.AddSingleton<LoggingService>();
    services.AddSingleton<DialogService>();
    services.AddSingleton<NavigationService>();
    services.AddSingleton<SettingsService>();
    services.AddSingleton<NoteService>();
    services.AddSingleton<SearchService>();

    services.AddKeyedSingleton<ICommandService, NavigationCommandService>(CommandServiceType.Navigation);
    services.AddKeyedSingleton<ICommandService, NoteCommandService>(CommandServiceType.Note);

    // DbContext
    services.AddSingleton<AppDbContextTaskDispatcher>();
    services.AddDbContextFactory<AppDbContext>();
    services.AddScoped<AppDbContextInitializer>();

    return services.BuildServiceProvider();
  }
  private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e) => WriteExceptionLog(e.Exception);

  private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
  {
    if (e.ExceptionObject is Exception ex)
    { WriteExceptionLog(ex); }
  }

  private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e) => WriteExceptionLog(e.Exception);

  private void WriteExceptionLog(Exception ex)
  {
    Console.WriteLine("{0}: {1}", "Exception", ex);
    var loggingService = Services.GetRequiredService<LoggingService>();
    loggingService.Write(ex);
  }

  public async Task OpenDebugWindow()
  {
    await Task.Delay(1000);
    new DebugWindow().Activate();
  }

  private async Task LaunchArgumentsPipeServerStreamAsync()
  {
    while (!Disposed)
    {
      using NamedPipeServerStream pipeServerStream = new(AppStrings.NamedPipe_LaunchArguments, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);

      await pipeServerStream.WaitForConnectionAsync();

      using StreamReader sr = new(pipeServerStream);

      var windowService = Services.GetRequiredService<WindowService>();

      string? arg;
      while ((arg = sr.ReadLine()?.Trim()) is not null)
      {
        Console.WriteLine("{0}: {1}", "arg", arg);
        switch (arg)
        {
          case AppStrings.LaunchArgument_JumpList_MainWindow:
            (await windowService.GetOrCreateMainWindow(NavigationId.Home)).Activate();
            break;
          case AppStrings.LaunchArgument_JumpList_NewNote:
            var noteCommandService = Services.GetRequiredKeyedService<ICommandService>(CommandServiceType.Note) as NoteCommandService;
            noteCommandService?.CreateNewNoteCommand.Execute(null);
            break;
          case AppStrings.LaunchArgument_JumpList_Settings:
            (await windowService.GetOrCreateMainWindow(NavigationId.Settings)).Activate();
            break;
        }
      }
    }
  }
}
