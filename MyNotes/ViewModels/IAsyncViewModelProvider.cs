namespace MyNotes.ViewModels;

internal interface IAsyncViewModelProvider<TModel, TViewModel> where TModel : notnull where TViewModel : ViewModelBase, IAsyncDisposable
{
  public Task<IAsyncViewModelLease<TViewModel>> ResolveAsync(TModel model);

  public Task<IAsyncViewModelLease<TViewModel>?> AcquireAsync(TModel model);
}

internal interface IAsyncViewModelProvider<TModel, TParam, TViewModel> where TModel : notnull where TParam : allows ref struct where TViewModel : ViewModelBase, IAsyncDisposable 
{
  public Task<IAsyncViewModelLease<TViewModel>> ResolveAsync(TModel model, TParam parameter);

  public Task<IAsyncViewModelLease<TViewModel>?> AcquireAsync(TModel model);
}