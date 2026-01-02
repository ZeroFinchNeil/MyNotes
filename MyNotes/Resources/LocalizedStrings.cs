using Windows.ApplicationModel.Resources;

namespace MyNotes.Resources;

internal class LocalizedStrings
{
  public static ResourceLoader DefaultResourceLoader { get; } = ResourceLoader.GetForViewIndependentUse();
  public static ResourceLoader SettingsPageResourceLoader { get; } = ResourceLoader.GetForViewIndependentUse("SettingsPage");
  public static ResourceLoader CreateUserNavigationDialogResourceLoader { get; } = ResourceLoader.GetForViewIndependentUse("CreateUserNavigationDialog");
  public static ResourceLoader UpdateUserNavigationDialogResourceLoader { get; } = ResourceLoader.GetForViewIndependentUse("UpdateUserNavigationDialog");

  public static readonly string NavigationHomeTitle = DefaultResourceLoader.GetString("NavigationHome_Title");
  public static readonly string NavigationBookmarksTitle = DefaultResourceLoader.GetString("NavigationBookmarks_Title");
  public static readonly string NavigationTrashTitle = DefaultResourceLoader.GetString("NavigationTrash_Title");
  public static readonly string NavigationSettingsTitle = DefaultResourceLoader.GetString("NavigationSettings_Title");

  public static readonly string AppLanguageUseSystem = DefaultResourceLoader.GetString("AppLanguage_UseSystem");

  public static readonly string NavigationUserCompositeNode_DisplayName = DefaultResourceLoader.GetString("NavigationUserCompositeNode_DisplayName");
  public static readonly string NavigationUserLeafNodeDisplayName = DefaultResourceLoader.GetString("NavigationUserLeafNode_DisplayName");
  public static readonly string NavigationUserRootNodeDisplayName = DefaultResourceLoader.GetString("NavigationUserRootNode_DisplayName");

  public static readonly string CreateUserNavigationDialogTitleText = CreateUserNavigationDialogResourceLoader.GetString("ContentDialog_TitleText");
  public static readonly string CreateUserNavigationDialogSubTitleTextBlockText = CreateUserNavigationDialogResourceLoader.GetString("SubTitleTextBlock_Text");

  public static readonly string UpdateUserNavigationDialogTitleText = UpdateUserNavigationDialogResourceLoader.GetString("ContentDialog_TitleText");
  public static readonly string UpdateUserNavigationDialogSubTitleTextBlockText = UpdateUserNavigationDialogResourceLoader.GetString("SubTitleTextBlock_Text");
}
