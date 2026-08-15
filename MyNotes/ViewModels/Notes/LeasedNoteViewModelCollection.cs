using MyNotes.Common.Collections;

namespace MyNotes.ViewModels.Notes;

internal sealed partial class LeasedNoteViewModelCollection : AsyncLeasedViewModelCollection<NoteViewModel, SortedObservableCollection<NoteViewModel>>
{
  public LeasedNoteViewModelCollection(Comparer<NoteViewModel> comparer) : base((viewmodels) => new SortedObservableCollection<NoteViewModel>(comparer)) { }

  public LeasedNoteViewModelCollection(IEnumerable<IAsyncViewModelLease<NoteViewModel>> leases, Comparer<NoteViewModel> comparer) : base(leases, (viewmodels) => new SortedObservableCollection<NoteViewModel>(comparer)) { }

  public override void Move(int oldIndex, int newIndex) => throw new NotSupportedException();

  public void ReorderItem(NoteViewModel viewmodel) => InnerViewModels.ReorderItem(viewmodel);

  public void Rearrange(Comparer<NoteViewModel> comparer) => InnerViewModels.Rearrange(comparer);
}