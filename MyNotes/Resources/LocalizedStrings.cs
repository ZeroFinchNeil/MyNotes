using Windows.ApplicationModel.Resources;

namespace MyNotes.Resources;

public class LocalizedStrings
{
  public static ResourceLoader ResourceLoader { get; } = ResourceLoader.GetForViewIndependentUse();

  public static readonly string NavigationHomeTitle = ResourceLoader.GetString("NavigationHome_Title");
  public static readonly string NavigationBookmarksTitle = ResourceLoader.GetString("NavigationBookmarks_Title");
  public static readonly string NavigationTrashTitle = ResourceLoader.GetString("NavigationTrash_Title");
  public static readonly string NavigationSettingsTitle = ResourceLoader.GetString("NavigationSettings_Title");

  public static readonly string SettingsDefaultLanguage = ResourceLoader.GetString("Settings_DefaultLanguage");
}
