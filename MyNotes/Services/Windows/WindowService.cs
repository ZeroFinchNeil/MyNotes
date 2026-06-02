using System.Diagnostics.CodeAnalysis;

using MyNotes.Application.Contracts.Windows;
using MyNotes.Common.Interop;

namespace MyNotes.Services.Windows;

internal class WindowService : IWindowService
{
  public bool TryGetFocusedWindow([NotNullWhen(true)] out Window? focusedWindow, out IntPtr hWnd)
  {
    throw new NotImplementedException();
    //focusedWindow = null;
    //IntPtr systemWindowHandle = NativeMethods.GetForegroundWindow();
    //hWnd = IntPtr.Zero;

    //if (systemWindowHandle != IntPtr.Zero)
    //{
    //  if (TryGetCurrentMainWindow(out var mainWindow)
    //    && WindowNative.GetWindowHandle(mainWindow) == systemWindowHandle)
    //  {
    //    focusedWindow = mainWindow;
    //    hWnd = systemWindowHandle;
    //    return true;
    //  }

    //  foreach (var wr in NoteWindowTable.Values)
    //  {
    //    if (wr.TryGetTarget(out var noteWindow)
    //      && !noteWindow.IsClosed
    //      && WindowNative.GetWindowHandle(noteWindow) == systemWindowHandle)
    //    {
    //      focusedWindow = noteWindow;
    //      hWnd = systemWindowHandle;
    //      return true;
    //    }
    //  }
    //}

    //return false;
  }

  public PointInt32 GetPosition(SizeInt32 windowSize)
  {
    if (TryGetFocusedWindow(out var focusedWindow, out var hWnd)
        && NativeMethods.GetMonitorInfoForWindow(hWnd) is NativeMethods.MONITORINFOEX monitorInfo)
    {
      var rect = monitorInfo.rcWork;
      int monitorWidth = rect.Right - rect.Left;
      int monitorHeight = rect.Bottom - rect.Top;
      int padding = 10;
      Range horizontal = new(rect.Left + padding, rect.Left + (monitorWidth - windowSize.Width) / 2);
      Range vertical = new(rect.Top + padding, rect.Top + (monitorHeight - windowSize.Height) / 2);

      Random random = new();
      int positionX = horizontal.Start.Value < horizontal.End.Value ? random.Next(horizontal.Start.Value, horizontal.End.Value) : horizontal.Start.Value;
      int positionY = vertical.Start.Value < vertical.End.Value ? random.Next(vertical.Start.Value, vertical.End.Value) : vertical.Start.Value;

      return new(positionX, positionY);
    }

    return default;
  }
}
