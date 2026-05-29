using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using MyNotes.Application.Contracts.Database.Core;

namespace MyNotes.Infrastructure.Database.Core;

internal sealed partial class AppDbTransaction : IAppDbTransaction, IAppDbTransactionContext
{
  private readonly IDbContextFactory<AppDbContext> DbContextFactory;

  public AppDbContext AppDbContext { get; private set; } = null!;

  public DbContext DbContext => AppDbContext;

  public IDbContextTransaction? Transaction { get; private set; }

  public bool IsCompleted { get; private set; } = false;

  public bool IsRolledBack { get; private set; } = false;

  public AppDbTransaction(IDbContextFactory<AppDbContext> dbContextFactory)
  {
    DbContextFactory = dbContextFactory;
  }

  public async Task InitializeAsync()
  {
    if (Transaction is not null)
    {
      throw new InvalidOperationException("트랜잭션이 이미 초기화되었습니다.");
    }

    AppDbContext = await DbContextFactory.CreateDbContextAsync();
    Transaction = await AppDbContext.Database.BeginTransactionAsync();
  }

  public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    _ = EnsureActiveTransaction();
    return await DbContext.SaveChangesAsync(cancellationToken);
  }

  public async Task CompleteAsync(bool saveTrackedChanges = true, CancellationToken cancellationToken = default)
  {
    var transaction = EnsureActiveTransaction();

    if (saveTrackedChanges)
    {
      await DbContext.SaveChangesAsync(cancellationToken);
    }

    await transaction.CommitAsync(cancellationToken);
    IsCompleted = true;
  }

  public async Task RollbackAsync(CancellationToken cancellationToken = default)
  {
    var transaction = EnsureActiveTransaction();
    await transaction.RollbackAsync(cancellationToken);
    IsRolledBack = true;
  }

  public async ValueTask DisposeAsync()
  {
    try
    {
      if (Transaction is not null && !IsCompleted && !IsRolledBack)
      {
        await Transaction.RollbackAsync();
        IsRolledBack = true;
      }
    }
    finally
    {
      if (Transaction is not null)
      {
        await Transaction.DisposeAsync();
      }

      if (AppDbContext is not null)
      {
        await AppDbContext.DisposeAsync();
      }
    }
  }

  [SuppressMessage("Style", "IDE0046:Use conditional expression for return", Justification = "상태 검증 예외 메시지를 guard clause 형태로 유지")]
  private IDbContextTransaction EnsureActiveTransaction()
  {
    if (Transaction is null)
    {
      throw new InvalidOperationException("트랜잭션이 초기화되지 않았습니다.");
    }

    if (IsRolledBack)
    {
      throw new InvalidOperationException("이미 롤백된 트랜잭션입니다.");
    }

    if (IsCompleted)
    {
      throw new InvalidOperationException("이미 커밋된 트랜잭션입니다.");
    }

    return Transaction;
  }
}