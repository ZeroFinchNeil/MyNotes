namespace MyNotes.Common.Structures;

public interface IEnumerable<T, TLeft, TRight>
  where TLeft : notnull
  where TRight : notnull
{
}