namespace MyNotes.Services.Updates;

internal interface IUpdateBatchDispatcher<TPatch> where TPatch : notnull
{
  public bool TryEnqueue(TPatch patch);
}

internal interface IUpdateBatchDispatcher<TPatch, TResult> where TPatch : notnull where TResult : notnull
{
}