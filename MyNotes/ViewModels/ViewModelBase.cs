using CommunityToolkit.Mvvm.ComponentModel;

namespace MyNotes.ViewModels;

[Debugging.ReferenceTracker]
internal abstract partial class ViewModelBase : ObservableObject, IViewModel, IDisposable
{
  protected ViewModelBase()
  {
    TrackReference();
  }

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
    {
      return;
    }

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