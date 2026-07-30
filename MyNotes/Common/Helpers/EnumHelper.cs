using MyNotes.Application.Contracts.Notes.Models;

namespace MyNotes.Common.Helpers;

internal static class EnumHelper
{
  public static int ToInt(BackdropKind backdropKind) => (int)backdropKind;
  public static BackdropKind ToBackdropKind(int num) => (BackdropKind)num;

  public static int? ToInt<TEnum>(TEnum? enumValue) where TEnum : struct, Enum => enumValue.HasValue ? (int)(ValueType)enumValue.Value : null;
  public static int? AsInt<TEnum>(this TEnum? enumValue) where TEnum : struct, Enum => ToInt(enumValue);
  public static int ToInt<TEnum>(TEnum enumValue) where TEnum : struct, Enum => (int)(ValueType)enumValue;
  public static int AsInt<TEnum>(this TEnum enumValue) where TEnum : struct, Enum => ToInt(enumValue);

  public static TEnum? ToEnum<TEnum>(int? intValue) where TEnum : struct, Enum => intValue.HasValue && Enum.IsDefined(typeof(TEnum), intValue.Value) ? (TEnum)Enum.ToObject(typeof(TEnum), intValue.Value) : null;
  public static TEnum? AsEnum<TEnum>(this int? intValue) where TEnum : struct, Enum => ToEnum<TEnum>(intValue);
  public static TEnum ToEnum<TEnum>(int intValue, TEnum defaultValue = default) where TEnum : struct, Enum => Enum.IsDefined(typeof(TEnum), intValue) ? (TEnum)Enum.ToObject(typeof(TEnum), intValue) : defaultValue;
  public static TEnum AsEnum<TEnum>(this int intValue, TEnum defaultValue = default) where TEnum : struct, Enum => ToEnum(intValue, defaultValue);
}
