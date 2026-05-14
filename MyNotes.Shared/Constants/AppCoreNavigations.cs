using System;
using System.Collections.Generic;
using System.Text;

namespace MyNotes.Shared.Constants;

internal static class AppCoreNavigations
{
  public static Guid EmptyId => Guid.Empty;
  public static Guid RootId => Guid.Parse("00000000-0000-0000-0000-000000000001");
  public static Guid HomeId => Guid.Parse("00000000-0000-0000-0000-000000000002");
  public static Guid BookmarksId => Guid.Parse("00000000-0000-0000-0000-000000000003");
  public static Guid TagsId => Guid.Parse("00000000-0000-0000-0000-000000000004");
  public static Guid TrashId => Guid.Parse("00000000-0000-0000-0000-000000000005");
  public static Guid SettingsId => Guid.Parse("00000000-0000-0000-0000-000000000006");
  public static Guid AllowedLowerBound => Guid.Parse("00000000-0000-0000-0000-000000000010");
}
