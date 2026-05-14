namespace MyNotes.ViewModels;

internal interface IViewModelFactory<TKey, TViewModel> where TKey : notnull where TViewModel : class
{
  public TViewModel Create(TKey key, params object[] parameters);
}
