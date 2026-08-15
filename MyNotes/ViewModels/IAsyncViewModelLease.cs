namespace MyNotes.ViewModels;

internal interface IAsyncViewModelLease<out TViewModel> : IAsyncDisposable where TViewModel : ViewModelBase, IAsyncDisposable
{
  public TViewModel ViewModel { get; }
}