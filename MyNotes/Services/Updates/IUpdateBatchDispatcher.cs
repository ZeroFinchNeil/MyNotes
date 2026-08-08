namespace MyNotes.Services.Updates;

internal interface IUpdateBatchDispatcher<TPatch> : IAsyncDisposable where TPatch : notnull
{
  public bool TryEnqueue(TPatch patch);
}

internal interface IUpdateBatchDispatcher<TPatch, TResult> : IAsyncDisposable where TPatch : notnull where TResult : notnull
{
}