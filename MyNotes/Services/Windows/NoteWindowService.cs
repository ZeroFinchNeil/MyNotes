using System.Diagnostics.CodeAnalysis;

using Microsoft.UI.Content;

using MyNotes.Domain.Notes;
using MyNotes.Models.Notes;
using MyNotes.Shell.Contracts.Windowing;
using MyNotes.Views.Windows;

using WinRT.Interop;

namespace MyNotes.Services.Windows;

internal class NoteWindowService : IWindowService<NoteId, NoteWindow>
{
  private readonly INativeWindowing NativeWindowing;
  public Dictionary<NoteId, WeakReference<NoteWindow>> NoteWindowTable { get; } = new();

  public NoteWindowService(INativeWindowing nativeWindowing)
  {
    NativeWindowing = nativeWindowing;
  }

  public bool TryGetCurrentWindow(NoteId noteId, [NotNullWhen(true)] out NoteWindow? noteWindow)
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

  public bool TryGetWindowInfo(NoteId noteId, out IntPtr hWnd, [NotNullWhen(true)] out AppWindow? appWindow)
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

  public bool TryGetWindowInfo(FrameworkElement element, NoteId noteId, out IntPtr hWnd, [NotNullWhen(true)] out AppWindow? appWindow)
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

  public bool TryExecuteOnWindow(NoteId noteId, Action<NoteWindow> action)
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
      : await NoteWindow.CreateAsync(noteModel);

    if (activate)
    {
      noteWindow.Activate();
    }

    return noteWindow;
  }
}
