using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using MyNotes.Common.Structures;

namespace MyNotes.Common.Collections;

public interface IReadOnlyBijectiveMap<TLeft, TRight> : IEnumerable<BijectivePair<TLeft, TRight>>, IReadOnlyCollection<BijectivePair<TLeft, TRight>> where TLeft : notnull where TRight : notnull
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
