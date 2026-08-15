using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MyNotes.Debugging;

public static class ReferenceTracker
{
  private static readonly ConcurrentDictionary<Type, ConcurrentBag<WeakReference>> _referenceTable = new();

  public static void Register(object obj)
  {
    Type t = obj.GetType();
    if (_referenceTable.TryGetValue(t, out var references))
    {
      references.Add(new WeakReference(obj));
    }
    else
    {
      _referenceTable[t] = [new WeakReference(obj)];
    }
  }

  public static IReadOnlyDictionary<Type, IReadOnlyList<object?>> GetAliveReferences()
  {
    Dictionary<Type, IReadOnlyList<object?>> table = new();
    foreach (var key in _referenceTable.Keys)
    {
      var references = _referenceTable[key].Where(wr => wr.IsAlive).Select(wr => wr.Target);
      if (references.Any())
      {
        table[key] = [.. references];
      }
    }
    return table;
  }
}
