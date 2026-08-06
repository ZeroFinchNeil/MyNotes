namespace MyNotes.Services.ViewState.Dispatcher;

internal interface IViewStatePersistenceDispatcher<TPatch> : IAsyncDisposable
{
  public bool TryEnqueue(TPatch patch);
}