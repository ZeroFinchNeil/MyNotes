using Windows.Foundation;

namespace MyNotes.Services.Settings;

public static class SettingsDescriptors
{
  // Windows
  public static readonly SettingsDescriptor<Size> MainWindowMinimumSize = new("MainWindowMininumSize", new Size(600.0, 600.0));
  public static readonly SettingsDescriptor<Size> MainWindowSize = new("MainWindowSize", new Size(600.0, 800.0));
  public static readonly SettingsDescriptor<Point> MainWindowPosition = new("MainWindowPosition", new Point(0.0, 0.0));
  public static readonly SettingsDescriptor<string> MainWindowDisplay = new("MainWindowDisplay", string.Empty);
  public static readonly SettingsDescriptor<int> WindowBorderMargin = new("WindowBorderMargin", 20);

  // Settings - Appearence
  public static readonly SettingsDescriptor<int> AppTheme = new("AppTheme", 0);
  public static readonly SettingsDescriptor<string> AppLanguage = new("AppLanguage", string.Empty);

  // Settings - Note
  public static readonly SettingsDescriptor<string> NoteBackground = new("NoteBackground", "#fff2e28d");
  public static readonly SettingsDescriptor<int> NoteBackdrop = new("NoteBackdrop", 0);
  public static readonly SettingsDescriptor<Size> NoteSize = new("NoteSize", new Size(300.0, 200.0));

  // Settings - List
  public static readonly SettingsDescriptor<bool> ShowNoteCount = new("ShowNoteCount", true);
}