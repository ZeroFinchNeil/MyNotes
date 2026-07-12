using System;

using Microsoft.UI.Xaml;

using MyNotes.Common.Querying;
using MyNotes.Common.Structures;

using Windows.Foundation;

namespace MyNotes.Shared.Constants;

internal static class AppSettingsDescriptors
{
  // Windows
  public static SettingsDescriptor<bool> IsMainWindowOpen { get; } = new("IsMainWindowOpen", AppDefaultSettings.IsMainWindowOpen);
  public static SettingsDescriptor<Size> MainWindowMinimumSize { get; } = new("MainWindowMinimumSize", AppDefaultSettings.MainWindowMinimumSize);
  public static SettingsDescriptor<Size> MainWindowSize { get; } = new("MainWindowSize", AppDefaultSettings.MainWindowSize);
  public static SettingsDescriptor<Point> MainWindowPosition { get; } = new("MainWindowPosition", AppDefaultSettings.MainWindowPosition);
  public static SettingsDescriptor<string> MainWindowDisplay { get; } = new("MainWindowDisplay", AppDefaultSettings.MainWindowDisplay);
  public static SettingsDescriptor<int> WindowBorderMargin { get; } = new("WindowBorderMargin", AppDefaultSettings.WindowBorderMargin);

  // Settings - Appearance
  public static SettingsDescriptor<int> AppTheme { get; } = new("AppTheme", (int)AppDefaultSettings.AppTheme);
  public static SettingsDescriptor<string> AppLanguage { get; } = new("AppLanguage", AppDefaultSettings.AppLanguage);

  // Settings - General
  public static SettingsDescriptor<int> InitialPageType { get; } = new("InitialPageType", AppDefaultSettings.InitialPageType);
  public static SettingsDescriptor<Guid> InitialPageId { get; } = new("InitialPageId", AppDefaultSettings.InitialPageId);
  public static SettingsDescriptor<bool> ConfirmBeforeDeleting { get; } = new("ConfirmBeforeDeleting", AppDefaultSettings.ConfirmBeforeDeleting);

  // Settings - Note
  public static SettingsDescriptor<Size> NoteWindowMinimumSize { get; } = new("NoteWindowMinimumSize", AppDefaultSettings.NoteWindowMinimumSize);

  public static SettingsDescriptor<string> NoteBackground { get; } = new("NoteBackground", AppDefaultSettings.NoteBackground);
  public static SettingsDescriptor<int> NoteBackdropKind { get; } = new("NoteBackdropKind", (int)AppDefaultSettings.NoteBackdropKind);
  public static SettingsDescriptor<Size> NoteSize { get; } = new("NoteSize", AppDefaultSettings.NoteSize);
  public static SettingsDescriptor<Point> NotePosition { get; } = new("NotePosition", AppDefaultSettings.NotePosition);

  public static SettingsDescriptor<int> NoteBodyUpdateFrequency { get; } = new("NoteBodyUpdateFrequency", AppDefaultSettings.NoteBodyUpdateFrequency);

  public static SettingsDescriptor<bool> DeleteEmptyNote { get; } = new("DeleteEmptyNote", AppDefaultSettings.DeleteEmptyNote);

  // Settings - List and Group
  public static SettingsDescriptor<bool> ShowNoteCount { get; } = new("ShowNoteCount", AppDefaultSettings.ShowNoteCount);
  public static SettingsDescriptor<int> GroupIconBadge { get; } = new("GroupIconBadge", (int)AppDefaultSettings.GroupIconBadge);

  public static SettingsDescriptor<bool> AllowCustomNoteSortOrder { get; } = new("AllowCustomSortOrder", AppDefaultSettings.AllowCustomNoteSortOrder);
  public static SettingsDescriptor<int> NoteSortKey { get; } = new("NoteSortKey", (int)AppDefaultSettings.NoteSortKey);
  public static SettingsDescriptor<int> NoteSortDirection { get; } = new("NoteSortDirection", (int)AppDefaultSettings.NoteSortDirection);

  public static SettingsDescriptor<bool> AllowCustomPreviewLayout { get; } = new("AllowCustomPreviewLayout", AppDefaultSettings.AllowCustomPreviewLayout);
  public static SettingsDescriptor<int> PreviewLayoutType { get; } = new("PreviewLayoutType", (int)AppDefaultSettings.PreviewLayoutType);
  public static SettingsDescriptor<int> PreviewTileSize { get; } = new("PreviewTileSize", (int)AppDefaultSettings.PreviewTileSize);
  public static SettingsDescriptor<int> PreviewTileRatio { get; } = new("PreviewTileRatio", (int)AppDefaultSettings.PreviewTileRatio);
}