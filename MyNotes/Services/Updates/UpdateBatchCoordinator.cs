using MyNotes.Debugging.Attributes;

namespace MyNotes.Services.Updates;

[ReferenceTracker]
internal sealed partial class UpdateBatchCoordinator<TPatch> : IUpdateBatchCoordinator<TPatch> where TPatch : notnull
{
  private readonly IUpdateBatchDispatcher<TPatch> Dispatcher;
  private readonly IUpdateBatcher<string, TPatch> Batcher;

  public UpdateBatchCoordinator(IUpdateBatchDispatcher<TPatch> dispatcher, IUpdateBatcher<string, TPatch> batcher)
  {
    Dispatcher = dispatcher;
    Batcher = batcher;
    TrackReference();
  }

  public void Submit(string key, TPatch patch, UpdateDispatchMode updateDispatchMode)
  {
    ArgumentNullException.ThrowIfNull(patch);

    switch (updateDispatchMode)
    {
      case UpdateDispatchMode.Immediate:
        Dispatcher.TryEnqueue(patch);
        break;
      case UpdateDispatchMode.Batched:
        Batcher.AddOrMerge(key, patch);
        break;
    }
  }
}