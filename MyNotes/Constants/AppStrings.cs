namespace MyNotes.Constants;

public static class AppStrings
{
  public const string AppDisplayName = "MyNotes";
  public static string PackageFamilyName => Windows.ApplicationModel.Package.Current.Id.FamilyName;
  public static readonly string LocalFolderPath = ApplicationData.Current.LocalFolder.Path;

  public const string AppIconPath = "Assets/AppIcon.ico";

  public const string AppInstanceKey = "MyNotes";

  public const string WidgetProvider_COM_CLSID = "A5423B36-2D5C-45CA-9268-71B560D7269A";
  public const string StartupTaskId = "StartupTaskId";

  public const string NamedPipe_LaunchArguments = """Local\MyNotes.NamedPipe.LaunchArguments""";
  public const string LaunchArgument_JumpList_NewNote = "JumpList_NewNote";
  public const string LaunchArgument_JumpList_MainWindow = "JumpList_MainWindow";
  public const string LaunchArgument_JumpList_Settings = "JumpList_Settings";

  public const string JsonEmptyObject = "{}";
  public const string JsonEmptyArray = "[]";

  public static ImmutableList<string> BitmapImageFileTypeFilter { get; } = [".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".ico"];

  public const string UserRootNavigationName = "Root";

  public const string FallbackImagePath = "ms-appx:///Assets/Images/failure.png";
  public static readonly string ImageFolderPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Images");

  public static Uri GetPhotosUri(string filePath) => new($"ms-photos:viewer?fileName={filePath}");
}