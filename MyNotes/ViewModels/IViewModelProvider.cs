namespace MyNotes.ViewModels;

internal interface IViewModelProvider<TModel, TViewModel> where TViewModel : class
{
  public TViewModel Resolve(TModel model);
  public bool TryResolve(TModel model, out TViewModel? viewmodel);
}
