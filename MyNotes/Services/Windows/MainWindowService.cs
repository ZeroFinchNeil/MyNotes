using System.Diagnostics.CodeAnalysis;

using Microsoft.UI.Content;

using MyNotes.Application.Logging.Services;
using MyNotes.Domain.Navigations;
using MyNotes.Shell.Contracts.Windowing;
using MyNotes.Views.Windows;

using WinRT.Interop;

namespace MyNotes.Services.Windows;

internal class MainWindowService : IWindowService<MainWindow>
{
  private readonly INativeWindowing NativeWindowing;
  private WeakReference<MainWindow>? _mainWindowReference;

  public MainWindowService(INativeWindowing nativeWindowing)
  {
    NativeWindowing = nativeWindowing;
  }

  /// <summary>
  /// <para>Retrieves the current main window instance if it exists and is not closed; otherwise, creates and returns a new main window.</para>
  /// <para>기존 MainWindow 인스턴스가 남아 있고 종료되지 않았다면 이를 반환하고, 종료되었다면 인스턴스 정리 후 새로운 인스턴스를 생성하여 반환합니다.</para>
  /// </summary>
  /// <param name="navigationId">
  /// <para>An optional navigation identifier to set the initial navigation state of the main window. If null, the default navigation is used.</para>
  /// <para>MainWindow의 초기 탐색 상태를 설정니다. null인 경우 기본 탐색이 사용됩니다.</para>
  /// </param>
  public async Task<MainWindow> GetOrCreate(NavigationId? navigationId = null)
  {
    if (_mainWindowReference is not null
        && _mainWindowReference.TryGetTarget(out var mainWindow))
    {
      if (!mainWindow.IsClosed)
      {
        await mainWindow.LoadTask;
        mainWindow.SetNavigation(navigationId);
        return mainWindow;
      }
      else
      {
        mainWindow.Close();
        _mainWindowReference = null;
      }
    }

    MainWindow newWindow = new(navigationId);
    _mainWindowReference = new(newWindow);
    await newWindow.LoadTask;
    return newWindow;
  }

  public bool TryGetCurrentWindow([NotNullWhen(true)] out MainWindow? mainWindow)
  {
    mainWindow = null;

    if (_mainWindowReference is not null
        && _mainWindowReference.TryGetTarget(out var m)
        && !m.IsClosed)
    {
      mainWindow = m;
      return true;
    }

    return false;
  }

  public bool TryGetWindowInfo(FrameworkElement element, out IntPtr hWnd, [NotNullWhen(true)] out AppWindow? appWindow)
  {
    hWnd = IntPtr.Zero;
    appWindow = null;

    try
    {
      if (element.XamlRoot is XamlRoot xamlRoot
        && xamlRoot.ContentIslandEnvironment is ContentIslandEnvironment env)
      {
        var windowId = env.AppWindowId;
        hWnd = Win32Interop.GetWindowFromWindowId(windowId);
        appWindow = AppWindow.GetFromWindowId(windowId);
      }
      else if (TryGetCurrentWindow(out var mainWindow))
      {
        hWnd = WindowNative.GetWindowHandle(mainWindow);
        appWindow = mainWindow.AppWindow;
      }
    }
    catch (Exception e)
    {
      LoggingService.Write(e);
    }

    return hWnd != IntPtr.Zero && appWindow is not null;
  }

  public bool TryExecuteOnWindow(Action<MainWindow> action)
  {
    if (TryGetCurrentWindow(out var mainWindow))
    {
      action.Invoke(mainWindow);
      return true;
    }
    return false;
  }

  public PointInt32? GetNewWindowPosition(SizeInt32 newWindowSize) => TryGetCurrentWindow(out var mainWindow)
    ? NativeWindowing.GetNewWindowPositionOnMonitor(WindowNative.GetWindowHandle(mainWindow), newWindowSize)
    : null;
}
