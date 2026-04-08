namespace MyNotes.Common.Structures;

public readonly struct BijectivePair<TLeft, TRight>(TLeft left, TRight right) where TLeft : notnull where TRight : notnull
{
  public TLeft Left { get; } = left;
  public TRight Right { get; } = right;

  public void Deconstruct(out TLeft left, out TRight right)
  {
    left = Left;
    right = Right;
  }
}