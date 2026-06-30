using System;
using System.Threading;
using System.Threading.Tasks;

namespace MyNotes.Application.Contracts.Database.Core;

internal interface IAppDbTransaction : IAppDbTransactionContext, IAsyncDisposable
{
  public Task InitializeAsync(CancellationToken cancellationToken = default);

  public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

  public Task CompleteAsync(bool saveTrackedChanges = true, CancellationToken cancellationToken = default);

  public Task RollbackAsync(CancellationToken cancellationToken = default);

  public bool IsCompleted { get; }

  public bool IsRolledBack { get; }
}