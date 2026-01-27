using System.Diagnostics.CodeAnalysis;

using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Views.Windows;

namespace MyNotes.Services.Windows;

internal sealed class WindowService
{
  private WeakReference<MainWindow>? _mainWindow;
  public MainWindow? MainWindow =>
    (_mainWindow is not null && _mainWindow.TryGetTarget(out var mainWindow) && !mainWindow.IsClosed)
      ? mainWindow : null;

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