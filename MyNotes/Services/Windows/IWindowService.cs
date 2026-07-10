using System.Diagnostics.CodeAnalysis;

namespace MyNotes.Services.Windows;

internal interface IWindowService
{
}

internal interface IWindowService<TWindow> : IWindowService where TWindow : Window
{
  public bool TryGetCurrentWindow([NotNullWhen(true)] out TWindow? window);

  public bool TryGetWindowInfo(FrameworkElement element, out IntPtr hWnd, [NotNullWhen(true)] out AppWindow? appWindow);

  public bool TryExecuteOnWindow(Action<TWindow> action);
}

internal interface IWindowService<TKey, TWindow> : IWindowService where TKey : notnull where TWindow : Window
{
  public bool TryGetCurrentWindow(TKey key, [NotNullWhen(true)] out TWindow? window);

  public bool TryGetWindowInfo(TKey key, out IntPtr hWnd, [NotNullWhen(true)] out AppWindow? appWindow);

  public bool TryExecuteOnWindow(TKey key, Action<TWindow> action);
}