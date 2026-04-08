using System;
using System.Threading;

namespace MyNotes.Common.Lifetime;

[Debugging.ReferenceTracker]
internal sealed partial class ReferenceCounter<T> where T : class, IDisposable
{
  public required T Instance { get; init; }

  private int _referenceCount;
  public int ReferenceCount => Volatile.Read(ref _referenceCount);
  public bool HasNoReferences => ReferenceCount <= 0;

  public ReferenceCounter() { TrackReference(); }

  public void Increment()
  {
    Interlocked.Increment(ref _referenceCount);
    Console.WriteLine("{0}: {1}", "Reference Count", ReferenceCount);
  }

  public bool Decrement()
  {
    var newCount = Interlocked.Decrement(ref _referenceCount);
    Console.WriteLine("{0}: {1}", "Reference Count", ReferenceCount);
    if (newCount == 0)
    {
      Instance.Dispose();
      return true;
    }
    return false;
  }
}