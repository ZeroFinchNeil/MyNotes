using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MyNotes.Common.Collections;

public interface IReadOnlyBijectiveMap<TLeft, TRight> : IEnumerable<BijectiveMapPair<TLeft, TRight>>, IReadOnlyCollection<BijectiveMapPair<TLeft, TRight>> where TLeft : notnull where TRight : notnull
{
  public IReadOnlyCollection<TLeft> Lefts { get; }
  public IReadOnlyCollection<TRight> Rights { get; }

  public bool ContainsLeft(TLeft left);
  public bool ContainsRight(TRight right);

  public TRight? RightFromLeft(TLeft left);
  public TLeft? LeftFromRight(TRight right);

  public bool TryGetRight(TLeft left, [NotNullWhen(true)] out TRight? right);
  public bool TryGetLeft(TRight right, [NotNullWhen(true)] out TLeft? left);
}
