namespace MyNotes.Services.Updates;

internal interface IUpdateHandler<TPatch> where TPatch : notnull
{
  public Task HandleAsync(TPatch patch, CancellationToken cancellationToken = default);
}

internal interface IUpdateHandler<TPatch, TResult> where TPatch : notnull where TResult : notnull
{
  public Task<TResult> HandleAsync(TPatch patch, CancellationToken cancellationToken = default);
}