using MyNotes.Services.ViewState.Descriptors;

namespace MyNotes.Services.ViewState;

internal interface IViewStatePersistenceCoordinator<TPatch>
{
  public void Submit(string key, TPatch patch, ViewStateSaveMode saveMode);
}