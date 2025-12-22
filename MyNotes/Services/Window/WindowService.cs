using MyNotes.Views.Windows;

using XamlWindow = Microsoft.UI.Xaml.Window;

namespace MyNotes.Services.Window;

internal sealed partial class WindowService
{
  public MainWindow MainWindow => field ??= new();
}
