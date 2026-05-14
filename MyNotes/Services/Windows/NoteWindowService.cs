using System.Diagnostics.CodeAnalysis;

using MyNotes.Application.Contracts.Windows;
using MyNotes.Domain.ValueObjects;
using MyNotes.Models.Notes;
using MyNotes.Views.Windows;

using WinRT.Interop;

namespace MyNotes.Services.Windows;

internal class NoteWindowService : WindowService, INoteWindowService
{
  public Dictionary<NoteId, WeakReference<NoteWindow>> NoteWindowTable { get; } = new();

  public bool TryGetNoteWindow(NoteId noteId, [NotNullWhen(true)] out NoteWindow? noteWindow)
  {
    if (NoteWindowTable.TryGetValue(noteId, out var wr)
      && wr.TryGetTarget(out var window)
      && !window.IsClosed)
    {
      noteWindow = window;
      return true;
    }

    noteWindow = null;
    return false;
  }

  public bool TryGetNoteWindowInfo(NoteId noteId, out IntPtr hWnd, [NotNullWhen(true)] out AppWindow? appWindow)
  {
    hWnd = IntPtr.Zero;
    appWindow = null;

    try
    {
      if (NoteWindowTable.TryGetValue(noteId, out var wr)
        && wr.TryGetTarget(out var noteWindow)
        && !noteWindow.IsClosed)
      {
        hWnd = WindowNative.GetWindowHandle(noteWindow);
        appWindow = noteWindow.AppWindow;
      }
    }
    catch
    { }

    return hWnd != IntPtr.Zero && appWindow is not null;
  }

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
      else if (NoteWindowTable.TryGetValue(noteId, out var wr)
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
    if (NoteWindowTable.TryGetValue(noteId, out var wr)
        && wr.TryGetTarget(out var noteWindow))
    {
      action.Invoke(noteWindow);
      return true;
    }
    return false;
  }

  public async Task<NoteWindow> OpenNoteWindow(NoteModel noteModel, bool activate = true)
  {
    NoteWindow noteWindow =
      NoteWindowTable.TryGetValue(noteModel.Id, out var wr)
      && wr.TryGetTarget(out var existingNoteWindow)
      && !existingNoteWindow.IsClosed
      ? existingNoteWindow
      : new(noteModel);

    if (activate)
    {
      await noteWindow.LoadTask;
      noteWindow.Activate();
    }

    return noteWindow;
  }
}
