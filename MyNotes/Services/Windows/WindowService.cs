using System.Diagnostics.CodeAnalysis;

using Microsoft.UI.Content;

using MyNotes.Common.Interop;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.Logging;
using MyNotes.Views.Windows;

using WinRT.Interop;

namespace MyNotes.Services.Windows;

internal sealed class WindowService
{
  private readonly LoggingService LoggingService;

  public WindowService(LoggingService loggingService)
  {
    LoggingService = loggingService;
  }

  #region Main Window
  private WeakReference<MainWindow>? _mainWindow;

  /// <summary>
  /// <para>Retrieves the current main window instance if it exists and is not closed; otherwise, creates and returns a new main window.</para>
  /// <para>기존 MainWindow 인스턴스가 남아 있고 종료되지 않았다면 이를 반환하고, 종료되었다면 인스턴스 정리 후 새로운 인스턴스를 생성하여 반환합니다.</para>
  /// </summary>
  /// <param name="navigationId">An optional navigation identifier to set the initial navigation state of the main window. If null, the default navigation is used.</param>
  /// <returns>The existing main window instance if it is open; otherwise, a new main window instance.</returns>
  public MainWindow GetOrCreateMainWindow(NavigationId? navigationId = null)
  {
    if (_mainWindow is not null
        && _mainWindow.TryGetTarget(out var mainWindow))
    {
      if (!mainWindow.IsClosed)
      {
        mainWindow.SetNavigation(navigationId);
        return mainWindow;
      }
      else
      {
        mainWindow.Close();
        _mainWindow = null;
      }
    }

    MainWindow newWindow = new(navigationId);
    _mainWindow = new(newWindow);
    return newWindow;
  }

  public bool TryGetCurrentMainWindow([NotNullWhen(true)] out MainWindow? mainWindow)
  {
    mainWindow = null;

    if (_mainWindow is not null
        && _mainWindow.TryGetTarget(out var m)
        && !m.IsClosed)
    {
      mainWindow = m;
      return true;
    }

    return false;
  }

  public bool TryGetMainWindowInfo(FrameworkElement element, out IntPtr hWnd, [NotNullWhen(true)] out AppWindow? appWindow)
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
      else if (TryGetCurrentMainWindow(out var mainWindow))
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

  public bool TryExecuteOnMainWindow(Action<MainWindow> action)
  {
    if (TryGetCurrentMainWindow(out var mainWindow))
    {
      action.Invoke(mainWindow);
      return true;
    }
    return false;
  }
  #endregion

  #region Note Windows
  public Dictionary<NoteId, WeakReference<NoteWindow>> NoteWindows { get; } = new();

  public bool TryGetNoteWindowInfo(FrameworkElement element, NoteId noteId, out IntPtr hWnd, [NotNullWhen(true)] out AppWindow? appWindow)
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
      else if (NoteWindows.TryGetValue(noteId, out var wr)
        && wr.TryGetTarget(out var noteWindow))
      {
        hWnd = WindowNative.GetWindowHandle(noteWindow);
        appWindow = noteWindow.AppWindow;
      }
    }
    catch
    { }

    return hWnd != IntPtr.Zero && appWindow is not null;
  }

  public bool TryExecuteOnNoteWindow(NoteId noteId, Action<NoteWindow> action)
  {
    if (NoteWindows.TryGetValue(noteId, out var wr)
        && wr.TryGetTarget(out var noteWindow))
    {
      action.Invoke(noteWindow);
      return true;
    }
    return false;
  }
  #endregion

  #region 통합 창 로직
  public bool TryGetFocusedWindow([NotNullWhen(true)] out Window? focusedWindow, out IntPtr hWnd)
  {
    focusedWindow = null;
    IntPtr systemWindowHandle = NativeMethods.GetForegroundWindow();
    hWnd = IntPtr.Zero;

    if (systemWindowHandle != IntPtr.Zero)
    {
      if (TryGetCurrentMainWindow(out var mainWindow)
        && WindowNative.GetWindowHandle(mainWindow) == systemWindowHandle)
      {
        focusedWindow = mainWindow;
        hWnd = systemWindowHandle;
        return true;
      }

      foreach (var wr in NoteWindows.Values)
      {
        if (wr.TryGetTarget(out var noteWindow)
          && !noteWindow.IsClosed
          && WindowNative.GetWindowHandle(noteWindow) == systemWindowHandle)
        {
          focusedWindow = noteWindow;
          hWnd = systemWindowHandle;
          return true;
        }
      }
    }

    return false;
  }
  #endregion
}