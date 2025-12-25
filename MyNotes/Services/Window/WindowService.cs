using MyNotes.Views.Windows;

using XamlWindow = Microsoft.UI.Xaml.Window;

namespace MyNotes.Services.Window;

internal sealed class WindowService
{
  public MainWindow MainWindow => field ??= new();
}
