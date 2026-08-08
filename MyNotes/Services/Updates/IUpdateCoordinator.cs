namespace MyNotes.Services.Updates;

internal interface IUpdateCoordinator<TKey, TPatch> where TKey : notnull where TPatch : notnull
{
  public void Submit(TKey key, TPatch patch, UpdateBatchMode updateDispatchMode);
}

internal interface IUpdateCoordinator<TKey, TPatch, TResult> where TKey : notnull where TPatch : notnull where TResult : notnull
{
  public TResult Submit(TKey key, TPatch patch, UpdateBatchMode updateDispatchMode);
}