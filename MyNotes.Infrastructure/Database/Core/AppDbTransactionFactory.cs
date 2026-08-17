using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MyNotes.Application.Contracts.Database.Core;

namespace MyNotes.Infrastructure.Database.Core;

internal sealed class AppDbTransactionFactory : IAppDbTransactionFactory
{
  private readonly IDbContextFactory<AppDbContext> DbContextFactory;

  public AppDbTransactionFactory(IDbContextFactory<AppDbContext> dbContextFactory)
  {
    DbContextFactory = dbContextFactory;
  }

  public async Task<IAppDbTransaction> CreateAsync(CancellationToken cancellationToken = default)
  {
    IAppDbTransaction transaction = new AppDbTransaction(DbContextFactory);
    try
    {
      await transaction.InitializeAsync(cancellationToken);
      return transaction;
    }
    catch
    {
      await transaction.DisposeAsync();
      throw;
    }
  }
}
