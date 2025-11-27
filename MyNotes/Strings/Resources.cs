using Windows.ApplicationModel.Resources;

namespace MyNotes.Strings;

public class Resources
{
  public const string WidgetProvider_COM_CLSID = "A5423B36-2D5C-45CA-9268-71B560D7269A";
  public const string StartupTaskId = "StartupTaskId";

  public static ResourceLoader ResourceLoader { get; } = ResourceLoader.GetForViewIndependentUse();

  public static readonly string NavigationHomeTitle = ResourceLoader.GetString("NavigationHome_Title");
  public static readonly string NavigationBookmarksTitle = ResourceLoader.GetString("NavigationBookmarks_Title");
  public static readonly string NavigationTrashTitle = ResourceLoader.GetString("NavigationTrash_Title");
  public static readonly string NavigationSettingsTitle = ResourceLoader.GetString("NavigationSettings_Title");

  public static readonly string SettingsDefaultLanguage = ResourceLoader.GetString("Settings_DefaultLanguage");
}
