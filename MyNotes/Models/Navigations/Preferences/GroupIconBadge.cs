using MyNotes.Common.Structures;

namespace MyNotes.Models.Navigations.Preferences;

public enum GroupIconBadge
{
  None,
  Folder
}

public sealed class GroupIconBadgeSettingsCodec : ISettingsCodec<GroupIconBadge, int>
{
  public static GroupIconBadgeSettingsCodec Default => field ??= new();
  private GroupIconBadgeSettingsCodec() { }

  public int Encode(GroupIconBadge input) => (int)input;

  public GroupIconBadge Decode(int output) => (GroupIconBadge)output;
}