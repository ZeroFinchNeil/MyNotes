using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using MyNotes.Debugging;

namespace MyNotes.Common.Lifetime;

public sealed class ReferenceCounter<T>(T instance) where T : class
{
  private T? _instance = instance;

  private readonly Lock _syncRoot = new();

  private bool _isDetached = false;

  private int _referenceCount;
  public int ReferenceCount => Volatile.Read(ref _referenceCount);

  public bool TryAcquire([NotNullWhen(true)] out T? reference)
  {
    lock (_syncRoot)
    {
      if (_isDetached)
      {
        reference = null;
        return false;
      }

      if (_instance is null)
      {
        throw new InvalidOperationException("Instance is null");
      }

      _referenceCount++;
      ConsoleHelper.WriteLine(true, "{0}: {1} ({2})", "Reference Increased", _referenceCount, typeof(T).Name);

      reference = _instance;
      return true;
    }
  }

  public bool ReleaseOrDetach(out T reference)
  {
    lock (_syncRoot)
    {
      if (_isDetached)
      {
        throw new InvalidOperationException("Detached");
      }

      if (_instance is null)
      {
        throw new InvalidOperationException("Instance is null");
      }

      if (_referenceCount <= 0)
      {
        throw new InvalidOperationException("획득하지 않은 참조를 해제할 수 없습니다.");
      }

      try
      {
        reference = _instance;

        if (--_referenceCount == 0)
        {
          _instance = null;
          _isDetached = true;
          return true;
        }

        return false;
      }
      finally
      {
        ConsoleHelper.WriteLine(true, "{0}: {1} ({2})", "Reference Decreased", _referenceCount, typeof(T).Name);
      }
    }
  }
}