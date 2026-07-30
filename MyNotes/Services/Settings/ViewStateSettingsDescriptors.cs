using MyNotes.Common.Structures;
using MyNotes.Domain.Navigations;
using MyNotes.Models.Navigations;
using MyNotes.Models.UI;

namespace MyNotes.Services.Settings;

internal static class ViewStateSettingsDescriptors
{
  // Settings - Appearance
  public static SettingsDescriptor<ElementTheme> AppTheme { get; } = new()
  {
    Key = "AppTheme",
    DefaultValue = ElementTheme.Default
  };
  public static SettingsDescriptor<AppLanguage> AppLanguage { get; } = new()
  {
    Key = "AppLanguage",
    DefaultValue = new AppLanguage()
  };

  // Settings - General
  public static SettingsDescriptor<InitialPageType> InitialPageType { get; } = new()
  {
    Key = "InitialPageType",
    DefaultValue = Models.Navigations.InitialPageType.Home
  };
  public static SettingsDescriptor<Guid> InitialPageId { get; } = new()
  {
    Key = "InitialPageId",
    DefaultValue = NavigationGuids.HomeId
  };
  public static SettingsDescriptor<bool> ConfirmBeforeDeleting { get; } = new()
  {
    Key = "ConfirmBeforeDeleting",
    DefaultValue = true
  };

  // Settings - Note
  public static SettingsDescriptor<SizeInt32> NoteSize { get; } = new()
  {
    Key = "NoteSize",
    DefaultValue = new SizeInt32(500, 500)
  };

  public static SettingsDescriptor<PointInt32> NotePosition { get; } = new()
  {
    Key = "NotePosition",
    DefaultValue = new PointInt32(0, 0)
  };
  public static SettingsDescriptor<bool> DeleteEmptyNote { get; } = new()
  {
    Key = "DeleteEmptyNote",
    DefaultValue = true
  };

  // Settings - List and Group
  public static SettingsDescriptor<bool> ShowNoteCount { get; } = new()
  {
    Key = "ShowNoteCount",
    DefaultValue = true
  };
  public static SettingsDescriptor<GroupIconBadge> GroupIconBadge { get; } = new()
  {
    Key = "GroupIconBadge",
    DefaultValue = Models.Navigations.GroupIconBadge.Folder
  };

  public static SettingsDescriptor<bool> AllowCustomNoteSortOrder { get; } = new()
  {
    Key = "AllowCustomSortOrder",
    DefaultValue = true
  };

  public static SettingsDescriptor<bool> AllowCustomPreviewLayout { get; } = new()
  {
    Key = "AllowCustomPreviewLayout",
    DefaultValue = true
  };

  // Windows
  public static SettingsDescriptor<bool> IsMainWindowOpen { get; } = new("IsMainWindowOpen", true);
  public static SettingsDescriptor<Size> MainWindowMinimumSize { get; } = new("MainWindowMinimumSize", new(600.0, 600.0));
  public static SettingsDescriptor<Size> MainWindowSize { get; } = new("MainWindowSize", new(600.0, 800.0));
  public static SettingsDescriptor<Point> MainWindowPosition { get; } = new("MainWindowPosition", new(0.0, 0.0));
  public static SettingsDescriptor<string> MainWindowDisplay { get; } = new("MainWindowDisplay", string.Empty);
  public static SettingsDescriptor<int> WindowBorderMargin { get; } = new("WindowBorderMargin", 20);

  // Settings - Note
  public static SettingsDescriptor<Size> NoteWindowMinimumSize { get; } = new("NoteWindowMinimumSize", new(400.0, 300.0));

  public static SettingsDescriptor<int> NoteBodyUpdateFrequency { get; } = new("NoteBodyUpdateFrequency", 2);

  public static Point NoteWindowPosition { get; } = new(32.0, 32.0);
}