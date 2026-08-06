namespace MyNotes.Services.ViewState.Batching;

internal interface IViewStatePatchBatcher<TKey, TPatch> : IAsyncDisposable
{
  public void AddOrMerge(TKey key, TPatch patch);

  public void Flush();
}