namespace MyNotes.Services.Updates;

internal interface IUpdateCoordinator<TPatch> where TPatch : notnull
{
  public void Submit(string key, TPatch patch, UpdateBatchMode updateDispatchMode);
}

internal interface IUpdateCoordinator<TPatch, TResult> where TPatch : notnull where TResult : notnull
{
  public TResult Submit(string key, TPatch patch, UpdateBatchMode updateDispatchMode);
}