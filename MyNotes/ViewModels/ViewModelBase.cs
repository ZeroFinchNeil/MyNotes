using CommunityToolkit.Mvvm.ComponentModel;

using MyNotes.Debugging;

namespace MyNotes.ViewModels;

internal abstract class ViewModelBase : ObservableObject, IViewModel, IDisposable
{
#if DEBUG
  public ViewModelBase()
  {
    ReferenceTracker.ViewModelReference.Add(this, $"{GetType().Name.Replace("ViewModel", ""), 20}: {GetHashCode()}");
  }
#endif

  public bool IsDisposed => _disposed;

  protected bool _disposed;

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (_disposed)
      return;

    if (disposing)
    { }

    _disposed = true;
  }

  // 파생 클래스 IDisposable 패턴 예시
  //protected override void Dispose(bool disposing)
  //{
  //  if (_disposed)
  //    return;

  //  if (disposing)
  //  { }

  //  _disposed = true;
  //}
}