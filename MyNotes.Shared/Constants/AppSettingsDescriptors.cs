using System;

using Microsoft.UI.Xaml;

using MyNotes.Common.Querying;
using MyNotes.Common.Structures;

using Windows.Foundation;

namespace MyNotes.Shared.Constants;

internal static class AppSettingsDescriptors
{
  // Windows
  public static SettingsDescriptor<bool> IsMainWindowOpen { get; } = new("MainWindowMinimumSize", true);
  public static SettingsDescriptor<Size> MainWindowMinimumSize { get; } = new("MainWindowMinimumSize", new Size(600.0, 600.0));
  public static SettingsDescriptor<Size> MainWindowSize { get; } = new("MainWindowSize", new Size(600.0, 800.0));
  public static SettingsDescriptor<Point> MainWindowPosition { get; } = new("MainWindowPosition", new Point(0.0, 0.0));
  public static SettingsDescriptor<string> MainWindowDisplay { get; } = new("MainWindowDisplay", string.Empty);
  public static SettingsDescriptor<int> WindowBorderMargin { get; } = new("WindowBorderMargin", 20);

  // Settings - Appearance
  public static SettingsDescriptor<int> AppTheme { get; } = new("AppTheme", (int)ElementTheme.Default);
  public static SettingsDescriptor<string> AppLanguage { get; } = new("AppLanguage", string.Empty);

  // Settings - General
  public static SettingsDescriptor<int> InitialPageType { get; } = new("InitialPageType", 0);
  public static SettingsDescriptor<Guid> InitialPageId { get; } = new("InitialPageId", AppNavigationGuids.HomeId);
  public static SettingsDescriptor<bool> ConfirmBeforeDeleting { get; } = new("ConfirmBeforeDeleting", true);

  // Settings - Note
  public static SettingsDescriptor<Size> NoteWindowMinimumSize { get; } = new("NoteWindowMinimumSize", new Size(400.0, 300.0));

  public static SettingsDescriptor<string> NoteBackground { get; } = new("NoteBackground", "#fff2e28d");
  public static SettingsDescriptor<int> NoteBackdropKind { get; } = new("NoteBackdropKind", (int)Enums.Notes.BackdropKind.None);
  public static SettingsDescriptor<Size> NoteSize { get; } = new("NoteSize", new Size(500.0, 500.0));
  public static SettingsDescriptor<Point> NotePosition { get; } = new("NotePosition", new Point(0, 0));

  public static SettingsDescriptor<int> NoteBodyUpdateFrequency { get; } = new("NoteBodyUpdateFrequency", 2);

  public static SettingsDescriptor<bool> DeleteEmptyNote { get; } = new("DeleteEmptyNote", true);

  // Settings - List and Group
  public static SettingsDescriptor<bool> ShowNoteCount { get; } = new("ShowNoteCount", true);
  public static SettingsDescriptor<int> GroupIconBadge { get; } = new("GroupIconBadge", (int)Enums.Settings.GroupIconBadge.Folder);

  public static SettingsDescriptor<bool> AllowCustomNoteSortOrder { get; } = new("AllowCustomSortOrder", true);
  public static SettingsDescriptor<int> NoteSortKey { get; } = new("NoteSortKey", (int)Enums.Notes.NoteSortKey.Created);
  public static SettingsDescriptor<int> NoteSortDirection { get; } = new("NoteSortDirection", (int)SortDirection.Descending);

  public static SettingsDescriptor<bool> AllowCustomPreviewLayout { get; } = new("AllowCustomPreviewLayout", true);
  public static SettingsDescriptor<int> PreviewLayoutType { get; } = new("PreviewLayoutType", (int)Enums.Navigations.PreviewLayoutType.Grid);
  public static SettingsDescriptor<int> PreviewTileSize { get; } = new("PreviewTileSize", (int)Enums.Navigations.PreviewTileSize.Medium);
  public static SettingsDescriptor<int> PreviewTileRatio { get; } = new("PreviewTileRatio", (int)Enums.Navigations.PreviewTileRatio.Square);
}