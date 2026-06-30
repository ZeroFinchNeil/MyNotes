using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Application.Contracts.Database.Core;

namespace MyNotes.Infrastructure.Database.Core;

internal sealed class AppDbTransactionFactory : IAppDbTransactionFactory
{
  private readonly IServiceProvider ServiceProvider;

  public AppDbTransactionFactory(IServiceProvider serviceProvider)
  {
    ServiceProvider = serviceProvider;
  }

  public async Task<IAppDbTransaction> CreateAsync(CancellationToken cancellationToken = default)
  {
    IAppDbTransaction transaction = ServiceProvider.GetRequiredService<IAppDbTransaction>();
    await transaction.InitializeAsync(cancellationToken);
    return transaction;
  }
}
