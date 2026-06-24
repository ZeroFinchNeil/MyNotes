namespace MyNotes.Models;

internal interface IModelStore<TKey, TModel> where TKey : notnull where TModel : class
{
  public TModel GetOrAdd(TKey key, Func<TKey, TModel> factory);

  public TModel AddOrUpdate<TSource>(TKey key, Func<TKey, TModel> factory, Action<TModel, TSource> updater);

  public bool TryGetModel(TKey key, out TModel? model);
}