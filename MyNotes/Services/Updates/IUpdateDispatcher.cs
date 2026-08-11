namespace MyNotes.Services.Updates;

internal interface IUpdateDispatcher<TPatch> : IAsyncDisposable where TPatch : notnull
{
  public bool TryDispatch(TPatch patch);
}

internal interface IUpdateDispatcher<TPatch, TResult> : IAsyncDisposable where TPatch : notnull where TResult : notnull
{
  public Task<TResult> DispatchAsync(TPatch patch, CancellationToken cancellationToken = default);
}