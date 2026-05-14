using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using MyNotes.Application.Contracts.Database.Repositories;
using MyNotes.Infrastructure.Database.Core;

namespace MyNotes.Infrastructure.Database.Repositories;

internal sealed partial class DbTransaction : IDbTransaction
{
  private readonly IDbContextFactory<AppDbContext> DbContextFactory;

  public DbContext? DbContext { get; private set; }

  public IDbContextTransaction? Transaction { get; private set; }

  public DbTransaction(IDbContextFactory<AppDbContext> dbContextFactory)
  {
    DbContextFactory = dbContextFactory;
  }

  public async Task InitializeAsync()
  {
    DbContext = await DbContextFactory.CreateDbContextAsync();
    Transaction = await DbContext.Database.BeginTransactionAsync();
  }

  public async Task CommitAsync(CancellationToken cancellationToken = default)
  {
    //todo: 컨텍스트나 트랜잭션이 없을 때 예외 구현
    if (DbContext is null || Transaction is null)
    {
      throw new System.Exception();
    }

    await DbContext.SaveChangesAsync(cancellationToken);
    await Transaction.CommitAsync(cancellationToken);
    IsCommitted = true;
  }

  public async Task RollbackAsync(CancellationToken cancellationToken = default)
  {
    //todo: 컨텍스트나 트랜잭션이 없거나 이미 커밋되었을 때 예외 구현
    if (DbContext is null || Transaction is null || IsCommitted)
    {
      throw new System.Exception();
    }

    await Transaction.RollbackAsync(cancellationToken);
    IsRolledBack = true;
  }

  public bool IsCommitted { get; private set; } = false;

  public bool IsRolledBack { get; private set; } = false;

  public async ValueTask DisposeAsync()
  {
    if (Transaction is not null)
    {
      await Transaction.DisposeAsync();
    }

    if (DbContext is not null)
    {
      await DbContext.DisposeAsync();
    }
  }
}