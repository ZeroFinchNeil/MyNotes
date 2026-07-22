namespace MyNotes.ViewModels;

internal interface IViewModelProvider<TModel, TViewModel> where TModel : notnull where TViewModel : ViewModelBase
{
  /// <summary>
  /// <para>Gets or creates a ViewModel instance from the specified Model. If an instance of the Model already exists, this method returns the existing instance. If no instance exists, a new Model is created and returned.</para>
  /// <para>모델로부터 해당하는 뷰모델 인스턴스를 가져오거나 생성합니다. 모델 인스턴스가 이미 존재하면, 이 메서드는 기존 뷰모델 인스턴스를 반환합니다. 존재하지 않을 경우, 새 뷰모델 인스턴스를 생성하여 반환합니다.</para>
  /// </summary>
  /// <param name="model">
  /// <para>The Model object to be converted into a ViewModel. Cannot be null.</para>
  /// <para>뷰모델을 가져올 모델 객체입니다.</para>
  /// </param>
  /// <returns>
  /// <para>A ViewModel instance that represents the specified Model.</para>
  /// <para>주어진 모델에 해당하는 뷰모델 인스턴스입니다.</para>
  /// </returns>
  public TViewModel? Resolve(TModel model);

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
