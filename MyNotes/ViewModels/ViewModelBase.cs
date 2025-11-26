using CommunityToolkit.Mvvm.ComponentModel;

namespace MyNotes.ViewModels;

public abstract class ViewModelBase : ObservableObject, IViewModel
{ }

public abstract class DisposableViewModelBase : IDisposable
{
  private bool _disposed;

  protected virtual void Dispose(bool disposing)
  {
    if (_disposed)
      return;

    if (disposing)
    { }

    _disposed = true;
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }
}