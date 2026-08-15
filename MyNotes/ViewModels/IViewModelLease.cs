namespace MyNotes.ViewModels;

internal interface IViewModelLease<out TViewModel> : IDisposable where TViewModel : ViewModelBase
{
  public TViewModel ViewModel { get; }
}