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
}