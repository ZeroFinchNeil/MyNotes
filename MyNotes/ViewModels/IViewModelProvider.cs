namespace MyNotes.ViewModels;

internal interface IViewModelProvider<TModel, TViewModel> where TModel : notnull where TViewModel : ViewModelBase
{
  public IViewModelLease<TViewModel> Resolve(TModel model);

  public IViewModelLease<TViewModel>? Acquire(TModel model);
}

internal interface IViewModelProvider<TModel, TParam, TViewModel> where TModel : notnull where TParam : allows ref struct where TViewModel : ViewModelBase
{
  public IViewModelLease<TViewModel> Resolve(TModel model, TParam parameter);

  public IViewModelLease<TViewModel>? Acquire(TModel model);
}
