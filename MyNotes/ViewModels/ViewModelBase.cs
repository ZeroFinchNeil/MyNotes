using CommunityToolkit.Mvvm.ComponentModel;

namespace MyNotes.ViewModels;

internal abstract class ViewModelBase : ObservableObject, IViewModel
{ }

internal abstract class DisposableViewModelBase : ObservableObject, IViewModel, IDisposable
{
  protected bool _disposed;

  protected abstract void Dispose(bool disposing);

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  // IDisposable 패턴 예시
  // protected override void Dispose(bool disposing)
  // {
  //   if (_disposed)
  //     return;

  //   if (disposing)
  //   { }

  //   _disposed = true;
  // }
}