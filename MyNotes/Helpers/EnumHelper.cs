using MyNotes.Models.Notes;

namespace MyNotes.Helpers;

internal static class EnumHelper
{
  public static int ToInt(BackdropKind backdropKind) => (int)backdropKind;
  public static BackdropKind ToBackdropKind(int num) => (BackdropKind)num;
}
