using System.IO.Pipes;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Application.Services.App;
using MyNotes.Application.Services.Notes;
using MyNotes.Domain.ValueObjects;
using MyNotes.Infrastructure.Database.Core;
using MyNotes.Infrastructure.Logging;
using MyNotes.Infrastructure.Search.Core;
using MyNotes.Services;
using MyNotes.Services.Commands;
using MyNotes.Services.Dialogs;
using MyNotes.Services.Navigations;
using MyNotes.Services.Settings;
using MyNotes.Services.Windows;
using MyNotes.Shared.Constants;

namespace MyNotes;

public sealed partial class App : Microsoft.UI.Xaml.Application, IAsyncDisposable
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
    _ = appInitializeScope.ServiceProvider.GetRequiredService<AppSearchContext>();

    var navigationController = appInitializeScope.ServiceProvider.GetRequiredService<NavigationController>();
    await navigationController.InitializationTask;

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
    var mainWindowService = Services.GetRequiredService<MainWindowService>();
    var noteService = Services.GetRequiredService<NoteService>();
    var settingsService = Services.GetRequiredService<SettingsService>();

    AppActivationArguments appActivationArguments = AppInstance.GetCurrent().GetActivatedEventArgs();
    switch (appActivationArguments.Kind)
    {
      case ExtendedActivationKind.Launch or ExtendedActivationKind.StartupTask:
        var noteWindowsCount = await noteService.OpenNoteWindowsForOpenEntities();
        if (noteWindowsCount == 0 || settingsService.Load(AppSettingsDescriptors.IsMainWindowOpen))
        {
          var mainWindow = await mainWindowService.GetOrCreate();
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

  private async ValueTask DisposeAsync(bool disposing)
  {
    if (!Disposed)
    {
      if (disposing)
      {
        this.UnhandledException -= App_UnhandledException;
        AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;

        await Services.DisposeAsync();
      }
      Disposed = true;
    }
  }

  public async ValueTask DisposeAsync()
  {
    await DisposeAsync(disposing: true);
    GC.SuppressFinalize(this);
  }
  #endregion

  internal static ServiceProvider Services { get; } = ConfigureServices();

  private static ServiceProvider ConfigureServices()
  {
    ServiceCollection services = new();

    // Service
    services.AddSingleton<JumpListService>();
    services.AddSingleton<AppLogger>();
    services.AddSingleton<DialogService>();
    services.AddSingleton<SettingsService>();

    services.AddWindowServices();
    services.AddNavigationServices();
    services.AddNoteServices();
    services.AddCommandServices();

    services.AddDbCoreServices();
    services.AddSearchCoreServices();

    // ViewModel
    services.AddViewModelProviders();
    services.AddViewModels();

    return services.BuildServiceProvider();
  }
  private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e) => WriteExceptionLog(e.Exception);

  private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
  {
    if (e.ExceptionObject is Exception ex)
    {
      WriteExceptionLog(ex);
    }
  }

  private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e) => WriteExceptionLog(e.Exception);

  private void WriteExceptionLog(Exception ex)
  {
    Console.WriteLine("{0}: {1}", "Exception", ex);
    var loggingService = Services.GetRequiredService<AppLogger>();
    loggingService.Write(ex);
  }

  public async Task OpenDebugWindow()
  {
    await Task.Delay(1000);
    new Views.Windows.DebugWindow().Activate();
  }

  private async Task LaunchArgumentsPipeServerStreamAsync()
  {
    while (!Disposed)
    {
      using NamedPipeServerStream pipeServerStream = new(AppStrings.NamedPipe_LaunchArguments, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);

      await pipeServerStream.WaitForConnectionAsync();

      using StreamReader sr = new(pipeServerStream);

      var mainWindowService = Services.GetRequiredService<MainWindowService>();

      string? arg;
      while ((arg = sr.ReadLine()?.Trim()) is not null)
      {
        Console.WriteLine("{0}: {1}", "arg", arg);
        switch (arg)
        {
          case AppStrings.LaunchArgument_JumpList_MainWindow:
            (await mainWindowService.GetOrCreate(NavigationId.Home)).Activate();
            break;
          case AppStrings.LaunchArgument_JumpList_NewNote:
            var noteCommandService = Services.GetRequiredKeyedService<ICommandService>(CommandServiceType.Note) as NoteCommandService;
            noteCommandService?.CreateNewNoteCommand.Execute(null);
            break;
          case AppStrings.LaunchArgument_JumpList_Settings:
            (await mainWindowService.GetOrCreate(NavigationId.Settings)).Activate();
            break;
        }
      }
    }
  }
}
