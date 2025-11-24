using Windows.Foundation;

namespace MyNotes.Services.Settings;

public static class SettingsDescriptors
{
  public static readonly SettingsDescriptor<Size> MainWindowMinimumSize = new("MainWindowMininumSize", new Size(600.0, 600.0));
  public static readonly SettingsDescriptor<Size> MainWindowSize = new("MainWindowSize", new Size(600.0, 800.0));
  public static readonly SettingsDescriptor<Point> MainWindowPosition = new("MainWindowPosition", new Point(0.0, 0.0));
  public static readonly SettingsDescriptor<string> MainWindowDisplay = new("MainWindowDisplay", string.Empty);
  public static readonly SettingsDescriptor<int> WindowBorderMargin = new("WindowBorderMargin", 20);
}