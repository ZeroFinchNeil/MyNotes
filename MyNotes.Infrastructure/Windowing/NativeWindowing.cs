using System;

using MyNotes.Common.Interop;
using MyNotes.Shell.Contracts.Windowing;

using Windows.Graphics;

namespace MyNotes.Infrastructure.Windowing;

internal class NativeWindowing : INativeWindowing
{
  public NativeWindowing()
  {

  }

  public PointInt32 GetInitialPositionOnMonitor(IntPtr hWnd, SizeInt32 windowSize)
  {
    if (NativeMethods.GetMonitorInfoForWindow(hWnd) is NativeMethods.MONITORINFOEX monitorInfo)
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
