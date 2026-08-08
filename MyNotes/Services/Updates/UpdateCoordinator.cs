namespace MyNotes.Services.Updates;

internal sealed class UpdateCoordinator<TKey, TPatch> : IUpdateCoordinator<TKey, TPatch> where TKey : notnull where TPatch : notnull
{
  private readonly IUpdateDispatcher<TPatch> Dispatcher;
  private readonly IUpdateBatcher<TKey, TPatch> Batcher;

  public UpdateCoordinator(IUpdateDispatcher<TPatch> dispatcher, IUpdateBatcher<TKey, TPatch> batcher)
  {
    Dispatcher = dispatcher;
    Batcher = batcher;
  }

  public void Submit(TKey key, TPatch patch, UpdateBatchMode updateDispatchMode)
  {
    ArgumentNullException.ThrowIfNull(patch);

    switch (updateDispatchMode)
    {
      case UpdateBatchMode.Unbatched:
        Dispatcher.TryDispatch(patch);
        break;
      case UpdateBatchMode.Batched:
        Batcher.AddOrMerge(key, patch);
        break;
    }
  }
}

internal sealed class UpdateCoordinator<TKey, TPatch, TResult> : IUpdateCoordinator<TKey, TPatch, TResult> where TKey : notnull where TPatch : notnull where TResult : notnull
{
  public TResult Submit(TKey key, TPatch patch, UpdateBatchMode updateDispatchMode) => throw new NotImplementedException();
}