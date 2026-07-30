using System;

using Windows.Graphics;

namespace MyNotes.Shell.Contracts.Windowing;

internal interface INativeWindowing
{
  public PointInt32 GetNewWindowPositionOnMonitor(IntPtr hWnd, SizeInt32 windowSize);
}
