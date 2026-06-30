namespace MyNotes.Models;

internal interface IModelFactory<in TSource, out TModel> where TSource : notnull where TModel : class
{
  public TModel Create(TSource source);
}