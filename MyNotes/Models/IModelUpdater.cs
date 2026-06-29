namespace MyNotes.Models;

internal interface IModelUpdater<in TSource, in TTarget> where TSource : notnull where TTarget : class
{
  public void Update(TTarget target, TSource source);
}