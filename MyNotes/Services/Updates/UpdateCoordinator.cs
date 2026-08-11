namespace MyNotes.Services.Updates;

internal sealed class UpdateCoordinator<TKey, TPatch>(IUpdateDispatcher<TPatch> Dispatcher, IUpdateBatcher<TKey, TPatch> Batcher) : IUpdateCoordinator<TKey, TPatch> where TKey : notnull where TPatch : notnull
{
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

internal sealed class UpdateCoordinator<TKey, TPatch, TResult>(IUpdateDispatcher<TPatch, TResult> Dispatcher, IUpdateBatcher<TKey, TPatch, TResult> Batcher) : IUpdateCoordinator<TKey, TPatch, TResult> where TKey : notnull where TPatch : notnull where TResult : notnull
{
  public async Task<TResult> Submit(TKey key, TPatch patch, UpdateBatchMode updateDispatchMode, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(patch);

    return updateDispatchMode switch
    {
      UpdateBatchMode.Unbatched => await Dispatcher.DispatchAsync(patch, cancellationToken),
      UpdateBatchMode.Batched => await Batcher.AddOrMergeAsync(key, patch, cancellationToken),
      _ => throw new InvalidOperationException(),
    };
  }
}