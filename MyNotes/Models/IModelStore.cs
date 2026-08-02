namespace MyNotes.Models;

internal interface IModelStore<TKey, TModel> where TKey : notnull where TModel : class
{
  public TModel GetOrAdd(TKey key, Func<TKey, TModel> factory);

  public TModel AddOrUpdate(TKey key, Func<TKey, TModel> factory, Action<TModel> updater);

  public bool TryGet(TKey key, out TModel? model);
}