namespace MyNotes.ViewModels;

internal abstract class AsyncViewModelBase : ViewModelBase, IAsyncDisposable
{
  protected bool _disposeStarted;

  protected abstract ValueTask DisposeAsyncCore();

  public async ValueTask DisposeAsync()
  {
    await DisposeAsyncCore().ConfigureAwait(false);
    Dispose(disposing: false);
  }
}