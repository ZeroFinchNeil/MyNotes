using MyNotes.Common.Structures;

namespace MyNotes.Models.Navigations.Preferences;

internal enum InitialPageType
{
  Home,
  Bookmarks,
  LastOpened,
  Preferred,
}

internal sealed class InitialPageTypeSettingsCodec : ISettingsCodec<InitialPageType, int>
{
  public static InitialPageTypeSettingsCodec Default => field ??= new();
  private InitialPageTypeSettingsCodec() { }

  public int Encode(InitialPageType input) => (int)input;

  public InitialPageType Decode(int output) => (InitialPageType)output;
}