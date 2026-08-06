using MyNotes.Debugging.Attributes;
using MyNotes.Services.ViewState.Batching;
using MyNotes.Services.ViewState.Descriptors;
using MyNotes.Services.ViewState.Dispatcher;

namespace MyNotes.Services.ViewState;

[ReferenceTracker]
internal sealed partial class ViewStatePersistenceCoordinator<TPatch> : IViewStatePersistenceCoordinator<TPatch> where TPatch : notnull
{
  private readonly IViewStatePersistenceDispatcher<TPatch> Dispatcher;
  private readonly IViewStatePatchBatcher<string, TPatch> Batcher;

  public ViewStatePersistenceCoordinator(IViewStatePersistenceDispatcher<TPatch> dispatcher, IViewStatePatchBatcher<string, TPatch> batcher)
  {
    Dispatcher = dispatcher;
    Batcher = batcher;
    TrackReference();
  }

  public void Submit(string key, TPatch patch, ViewStateSaveMode saveMode)
  {
    ArgumentNullException.ThrowIfNull(patch);

    switch (saveMode)
    {
      case ViewStateSaveMode.Immediate:
        Dispatcher.TryEnqueue(patch);
        break;
      case ViewStateSaveMode.Batched:
        Batcher.AddOrMerge(key, patch);
        break;
    }
  }
}