using System.Diagnostics.CodeAnalysis;

using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Views.Windows;

namespace MyNotes.Services.Windows;

internal sealed class WindowService
{
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

  public Dictionary<NoteId, WeakReference<NoteWindow>> NoteWindows { get; } = new();
}