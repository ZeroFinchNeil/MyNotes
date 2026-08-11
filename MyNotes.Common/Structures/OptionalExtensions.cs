using DotNext;

namespace MyNotes.Common.Structures;

public static class OptionalExtensions
{
  extension<T>(Optional<T?> optional) where T : struct
  {
    public bool TryGetSpecifiedValue(out T value)
    {
      if (optional.TryGet(out T? nullableValue))
      {
        value = nullableValue.Value;
        return true;
      }

      value = default;
      return false;
    }
  }

  extension<T>(Optional<T> optional)
  {
    public Optional<T> Overlay(Optional<T> other) => other.IsUndefined ? optional : other;
  }

  extension<T>(Optional<T>)
  {
    public static Optional<T> operator <<(Optional<T> o1, Optional<T> o2) => o2.IsUndefined ? o1 : o2;

    public static Optional<T> operator >>(Optional<T> o1, Optional<T> o2) => o1.IsUndefined ? o2 : o1;
  }
}