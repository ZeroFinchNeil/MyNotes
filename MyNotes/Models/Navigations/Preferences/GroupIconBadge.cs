namespace MyNotes.Models.Navigations.Preferences;

public enum GroupIconBadge
{
  None,
  Folder
}

public static class GroupIconBadgeSettingsCodec
{
  public static int Encode(GroupIconBadge input) => (int)input;

  public static GroupIconBadge Decode(int output) => (GroupIconBadge)output;
}