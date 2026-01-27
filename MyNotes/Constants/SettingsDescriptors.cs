using MyNotes.Common.Collections;
using MyNotes.Models.Navigations;
using MyNotes.Services.Settings;

namespace MyNotes.Constants;

internal static class SettingsDescriptors
{
  // Windows
  public static readonly SettingsDescriptor<Size> MainWindowMinimumSize = new("MainWindowMininumSize", new Size(600.0, 600.0));
  public static readonly SettingsDescriptor<Size> MainWindowSize = new("MainWindowSize", new Size(600.0, 800.0));
  public static readonly SettingsDescriptor<Point> MainWindowPosition = new("MainWindowPosition", new Point(0.0, 0.0));
  public static readonly SettingsDescriptor<string> MainWindowDisplay = new("MainWindowDisplay", string.Empty);
  public static readonly SettingsDescriptor<int> WindowBorderMargin = new("WindowBorderMargin", 20);

  // Settings - Appearence
  public static readonly SettingsDescriptor<int> AppTheme = new("AppTheme", (int)ElementTheme.Default);
  public static readonly SettingsDescriptor<string> AppLanguage = new("AppLanguage", string.Empty);

  // Settings - General
  public static readonly SettingsDescriptor<int> InitialPageType = new("InitialPageType", 0);
  public static readonly SettingsDescriptor<Guid> InitialPageId = new("InitialPageId", NavigationId.Home.Value);

  // Settings - Note
  public static readonly SettingsDescriptor<Size> NoteWindowMinimumSize = new("NoteWindowMininumSize", new Size(400.0, 300.0));

  public static readonly SettingsDescriptor<string> NoteBackground = new("NoteBackground", "#fff2e28d");
  public static readonly SettingsDescriptor<int> NoteBackdrop = new("NoteBackdrop", (int)Models.Notes.BackdropKind.None);
  public static readonly SettingsDescriptor<Size> NoteSize = new("NoteSize", new Size(500.0, 500.0));
  public static readonly SettingsDescriptor<Point> NotePosition = new("NotePosition", new Point(0, 0));

  public static readonly SettingsDescriptor<int> NoteBodyUpdateFrequency = new("NoteBodyUpdateFrequency", 2);

  // Settings - List and Group
  public static readonly SettingsDescriptor<bool> ShowNoteCount = new("ShowNoteCount", true);
  public static readonly SettingsDescriptor<int> GroupIconBadge = new("GroupIconBadge", (int)Models.Settings.GroupIconBadge.Folder);

  public static readonly SettingsDescriptor<bool> AllowCustomNoteSortOrder = new("AllowCustomSortOrder", true);
  public static readonly SettingsDescriptor<int> NoteSortKey = new("NoteSortKey", (int)Models.Notes.NoteSortKey.Created);
  public static readonly SettingsDescriptor<int> NoteSortDirection = new("NoteSortDirection", (int)SortDirection.Descending);

  public static readonly SettingsDescriptor<bool> AllowCustomPreviewLayout = new("AllowCustomPreviewLayout", true);
  public static readonly SettingsDescriptor<int> PreviewLayoutType = new("PreviewLayoutType", (int)Models.Navigations.PreviewLayoutType.Grid);
  public static readonly SettingsDescriptor<int> PreviewTileSize = new("PreviewTileSize", (int)Models.Navigations.PreviewTileSize.Medium);
  public static readonly SettingsDescriptor<int> PreviewTileRatio = new("PreviewTileRatio", (int)Models.Navigations.PreviewTileRatio.Square);
}