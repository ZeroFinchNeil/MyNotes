using Microsoft.Extensions.DependencyInjection;

using MyNotes.ViewModels;

using Windows.ApplicationModel;

namespace MyNotes;

public partial class App : Application
{
  public static App Instance => (App)Current;
  public static string PackageFamilyName { get; } = Package.Current.Id.FamilyName;

  private Window? _window;

  public App()
  {
    InitializeComponent();
  }

  protected override void OnLaunched(LaunchActivatedEventArgs args)
  {
    _window = new Views.Windows.MainWindow();
    _window.Activate();
  }

  public ServiceProvider Services { get; } = ConfigureServices();

  private static ServiceProvider ConfigureServices()
  {
    ServiceCollection services = new();

    // ViewModels
    services.AddSingleton<MainViewModel>();

    // Services

    return services.BuildServiceProvider();
  }
}
