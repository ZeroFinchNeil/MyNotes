using MyNotes.Common.Collections;

namespace MyNotes.ViewModels.Notes;

internal sealed partial class LeasedNotePreviewViewModelCollection : AsyncLeasedViewModelCollection<NotePreviewViewModel, SortedObservableCollection<NotePreviewViewModel>>
{
  public LeasedNotePreviewViewModelCollection(Comparer<NotePreviewViewModel> comparer) : base((viewmodels) => new SortedObservableCollection<NotePreviewViewModel>(comparer)) { }

  public LeasedNotePreviewViewModelCollection(IEnumerable<IAsyncViewModelLease<NotePreviewViewModel>> leases, Comparer<NotePreviewViewModel> comparer) : base(leases, (viewmodels) => new SortedObservableCollection<NotePreviewViewModel>(comparer)) { }

  public override void Move(int oldIndex, int newIndex) => throw new NotSupportedException();

  public void ReorderItem(NotePreviewViewModel viewmodel) => InnerViewModels.ReorderItem(viewmodel);

  public void Rearrange(Comparer<NotePreviewViewModel> comparer) => InnerViewModels.Rearrange(comparer);
}