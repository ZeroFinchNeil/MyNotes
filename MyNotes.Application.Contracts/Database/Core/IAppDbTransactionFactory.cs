using System.Threading.Tasks;

namespace MyNotes.Application.Contracts.Database.Core;

internal interface IAppDbTransactionFactory
{
  public Task<IAppDbTransaction> CreateAsync();
}