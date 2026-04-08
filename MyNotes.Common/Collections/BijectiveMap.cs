using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using MyNotes.Common.Structures;

namespace MyNotes.Common.Collections;

public class BijectiveMap<TLeft, TRight> : IEnumerable<BijectivePair<TLeft, TRight>>, IReadOnlyBijectiveMap<TLeft, TRight> where TLeft : notnull where TRight : notnull
{
  private readonly Dictionary<TLeft, TRight> _leftToRight = new();
  private readonly Dictionary<TRight, TLeft> _rightToLeft = new();

  public IReadOnlyCollection<TLeft> Lefts => _leftToRight.Keys;
  public IReadOnlyCollection<TRight> Rights => _rightToLeft.Keys;

  public int Count => _leftToRight.Count;

  public void Add(TLeft left, TRight right)
  {
    if (_leftToRight.ContainsKey(left))
    {
      throw new ArgumentException($"The specified Left('{left}') value already exists.", nameof(left));
    }

    if (_rightToLeft.ContainsKey(right))
    {
      throw new ArgumentException($"The specified Right('{right}') value already exists.", nameof(right));
    }

    _leftToRight[left] = right;
    _rightToLeft[right] = left;
  }

  public bool TryAdd(TLeft left, TRight right)
  {
    if (_leftToRight.ContainsKey(left) || _rightToLeft.ContainsKey(right))
    {
      return false;
    }

    _leftToRight[left] = right;
    _rightToLeft[right] = left;
    return true;
  }

  public bool Remove(TLeft left)
  {
    if (_leftToRight.TryGetValue(left, out var right))
    {
      _leftToRight.Remove(left);
      _rightToLeft.Remove(right);
      return true;
    }

    return false;
  }

  public bool Remove(TRight right)
  {
    if (_rightToLeft.TryGetValue(right, out var left))
    {
      _leftToRight.Remove(left);
      _rightToLeft.Remove(right);
      return true;
    }

    return false;
  }

  public TRight? RightFromLeft(TLeft left)
  {
    _leftToRight.TryGetValue(left, out var right);
    return right;
  }

  public TLeft? LeftFromRight(TRight right)
  {
    _rightToLeft.TryGetValue(right, out var left);
    return left;
  }

  public bool TryGetRight(TLeft left, [NotNullWhen(true)] out TRight? right) => _leftToRight.TryGetValue(left, out right);
  public bool TryGetLeft(TRight right, [NotNullWhen(true)] out TLeft? left) => _rightToLeft.TryGetValue(right, out left);

  public bool ContainsLeft(TLeft left) => _leftToRight.ContainsKey(left);
  public bool ContainsRight(TRight right) => _rightToLeft.ContainsKey(right);

  public IEnumerator<BijectivePair<TLeft, TRight>> GetEnumerator() => _leftToRight.Select(kv => new BijectivePair<TLeft, TRight>(kv.Key, kv.Value)).GetEnumerator();

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}