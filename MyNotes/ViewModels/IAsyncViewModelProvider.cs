namespace MyNotes.ViewModels;

internal interface IAsyncViewModelProvider<TModel, TViewModel> where TViewModel : class
{
  public Task<TViewModel> Resolve(TModel model);

  /// <summary>
  /// Attempts to resolve a ViewModel instance from the specified model.
  /// </summary>
  /// <param name="model">The Model object to resolve from. Cannot be null.</param>
  /// <param name="viewmodel">When this method returns, contains the resolved ViewModel if the operation succeeds; otherwise, contains null.
  /// This parameter is passed uninitialized.</param>
  /// <returns>true if the ViewModel was successfully resolved; otherwise, false.</returns>
  public bool TryResolve(TModel model, out TViewModel? viewmodel);

  public bool Release(TModel model);
}
