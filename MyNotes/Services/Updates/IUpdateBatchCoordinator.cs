namespace MyNotes.Services.Updates;

internal interface IUpdateBatchCoordinator<TPatch> where TPatch : notnull
{
  public void Submit(string key, TPatch patch, UpdateDispatchMode updateDispatchMode);
}

internal interface IUpdateBatchCoordinator<TPatch, TResult> where TPatch : notnull where TResult : notnull
{
  public TResult Submit(string key, TPatch patch, UpdateDispatchMode updateDispatchMode);
}