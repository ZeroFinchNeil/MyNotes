namespace MyNotes.ViewModels.Media;

internal sealed partial class LeasedImageViewModelCollection : LeasedViewModelCollection<ImageViewModel, ObservableCollection<ImageViewModel>>
{
  public LeasedImageViewModelCollection() : base((viewmodels) => new ObservableCollection<ImageViewModel>(viewmodels))
  {
  }

  public LeasedImageViewModelCollection(IEnumerable<IViewModelLease<ImageViewModel>> leases) : base(leases, (viewmodels) => new ObservableCollection<ImageViewModel>(viewmodels))
  {
  }
}