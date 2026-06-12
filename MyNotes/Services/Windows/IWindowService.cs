using System.Diagnostics.CodeAnalysis;

namespace MyNotes.Services.Windows;

internal interface IWindowService<T> where T : Window
{
  public bool TryGetCurrentWindow([NotNullWhen(true)] out T? window);
  public bool TryGetWindowInfo(FrameworkElement element, out IntPtr hWnd, [NotNullWhen(true)] out AppWindow? appWindow);
  public bool TryExecuteOnWindow(Action<T> action);
}