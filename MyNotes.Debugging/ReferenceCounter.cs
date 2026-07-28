using System;
using System.Threading;

namespace MyNotes.Debugging;

internal sealed partial class ReferenceCounter<T> where T : class, IDisposable
{
  public required T Instance { get; init; }

  private int _referenceCount;
  public int ReferenceCount => Volatile.Read(ref _referenceCount);
  public bool HasNoReferences => ReferenceCount <= 0;

  public ReferenceCounter() { }

  public void Increment()
  {
    Interlocked.Increment(ref _referenceCount);
    Console.WriteLine("{0}: {1}", "Reference Count (Increased)", ReferenceCount);
  }

  public bool Decrement(bool dispose = true)
  {
    var newCount = Interlocked.Decrement(ref _referenceCount);
    Console.WriteLine("{0}: {1}", "Reference Count (Decreased)", ReferenceCount);
    if (newCount == 0)
    {
      if (dispose)
      {
        Instance.Dispose();
      }
      return true;
    }
    return false;
  }
}