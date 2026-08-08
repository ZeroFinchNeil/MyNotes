namespace MyNotes.Services.Updates;

internal sealed class UpdateCoordinator<TPatch> : IUpdateCoordinator<TPatch> where TPatch : notnull
{
  private readonly IUpdateDispatcher<TPatch> Dispatcher;
  private readonly IUpdateBatcher<string, TPatch> Batcher;

  public UpdateCoordinator(IUpdateDispatcher<TPatch> dispatcher, IUpdateBatcher<string, TPatch> batcher)
  {
    Dispatcher = dispatcher;
    Batcher = batcher;
  }

  public void Submit(string key, TPatch patch, UpdateBatchMode updateDispatchMode)
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

internal sealed class UpdateCoordinator<TPatch, TResult> : IUpdateCoordinator<TPatch, TResult> where TPatch : notnull where TResult : notnull
{
  public TResult Submit(string key, TPatch patch, UpdateBatchMode updateDispatchMode) => throw new NotImplementedException();
}