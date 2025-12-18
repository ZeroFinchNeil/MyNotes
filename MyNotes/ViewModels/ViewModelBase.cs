using CommunityToolkit.Mvvm.ComponentModel;

namespace MyNotes.ViewModels;

internal abstract class ViewModelBase : ObservableObject, IViewModel, IDisposable
{
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