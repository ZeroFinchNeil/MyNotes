namespace MyNotes.Common.Collections;

public interface IEnumerable<T, TLeft, TRight>
  where TLeft : notnull
  where TRight : notnull
{
}