using System;
using System.Threading;
using System.Threading.Tasks;

namespace MyNotes.Application.Contracts.Database.Repositories;

internal interface IDbTransaction : IAsyncDisposable
{
  public Task CommitAsync(CancellationToken cancellationToken = default);
  
  public Task RollbackAsync(CancellationToken cancellationToken = default);

  public bool IsCommitted { get; }

  public bool IsRolledBack { get; }
}