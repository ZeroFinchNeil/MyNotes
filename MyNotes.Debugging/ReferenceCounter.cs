using System;
using System.Threading;

using MyNotes.Debugging;

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
    ConsoleHelper.WriteLine(true, "{0}: {1} ({2})", "Reference Increased", ReferenceCount, typeof(T).Name);
  }

  public bool Decrement(bool dispose = true)
  {
    var newCount = Interlocked.Decrement(ref _referenceCount);
    ConsoleHelper.WriteLine(true, "{0}: {1} ({2})", "Reference Decreased", ReferenceCount, typeof(T).Name);
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