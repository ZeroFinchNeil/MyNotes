namespace MyNotes.Models.Navigations.Preferences;

internal enum InitialPageType
{
  Home,
  Bookmarks,
  LastOpened,
  Preferred,
}

internal static class InitialPageTypeSettingsCodec
{
  public static int Encode(InitialPageType input) => (int)input;

  public static InitialPageType Decode(int output) => (InitialPageType)output;
}