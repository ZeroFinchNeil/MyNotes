namespace MyNotes.Services.Updates;

internal interface IUpdateBatcher<TKey, TPatch> : IAsyncDisposable where TKey : notnull where TPatch : notnull
{
  public void AddOrMerge(TKey key, TPatch patch);

  public void Flush();
}

internal interface IUpdateBatcher<TKey, TPatch, TResult> : IAsyncDisposable where TKey : notnull where TPatch : notnull where TResult : notnull
{
  public TResult AddOrMerge(TKey key, TPatch patch);

  public TResult Flush();
}