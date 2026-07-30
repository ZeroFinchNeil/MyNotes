using System;

namespace MyNotes.Domain.Navigations;

/// <summary>
/// 앱 Navigation 식별자에 사용하는 Guid 원시값과 Guid 공간 경계값입니다.
/// </summary>
/// <remarks>런타임 유효성 검사는 Bound 값을 직접 비교하지 말고 Domain의 NavigationId를 통해 수행해야 합니다.</remarks>
internal static class NavigationGuids
{
  public static Guid EmptyId => Guid.Empty;
  public static Guid RootId => Guid.Parse("00000000-0000-0000-0000-000000000001");
  public static Guid HomeId => Guid.Parse("00000000-0000-0000-0000-000000000002");
  public static Guid BookmarksId => Guid.Parse("00000000-0000-0000-0000-000000000003");
  public static Guid TagsId => Guid.Parse("00000000-0000-0000-0000-000000000004");
  public static Guid TrashId => Guid.Parse("00000000-0000-0000-0000-000000000005");
  public static Guid SettingsId => Guid.Parse("00000000-0000-0000-0000-000000000006");
  public static Guid AllowedLowerBound => Guid.Parse("00000000-0000-0000-0000-000000000010");
  public static Guid AllowedUpperBound => Guid.Parse("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFEFF");
}
