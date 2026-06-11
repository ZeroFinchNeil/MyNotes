using System;
using System.Diagnostics.CodeAnalysis;

using Microsoft.UI.Xaml;

using Windows.Graphics;

namespace MyNotes.Shell.Contracts.Windows;

internal interface IWindowService
{
  public bool TryGetFocusedWindow([NotNullWhen(true)] out Window? focusedWindow, out IntPtr hWnd);

  public PointInt32 GetPosition(SizeInt32 windowSize);
}
