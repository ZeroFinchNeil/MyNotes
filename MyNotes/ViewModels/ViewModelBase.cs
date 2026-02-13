using CommunityToolkit.Mvvm.ComponentModel;

using MyNotes.Debugging;

namespace MyNotes.ViewModels;

internal abstract class ViewModelBase : ObservableObject, IViewModel, IDisposable
{
#if DEBUG
  public ViewModelBase()
  {
    if (Debugger.IsAttached)
    {
      ReferenceTracker.ViewModelReference.Add(this, $"{GetType().Name.Replace("ViewModel", ""),20}: {GetHashCode()}");
    }
  }
#endif

  public bool Disposed { get; protected set; }

  public event EventHandler? Disposing;

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (Disposed)
      return;

    if (disposing)
    {
      Disposing?.Invoke(this, EventArgs.Empty);
      Disposing = null;
    }

    Disposed = true;
  }

  // 파생 클래스 IDisposable 패턴 예시
  //protected override void Dispose(bool disposing)
  //{
  //  if (Disposed)
  //    return;

  //  if (disposing)
  //  { }

  //  base.Dispose(disposing);
  //}
}