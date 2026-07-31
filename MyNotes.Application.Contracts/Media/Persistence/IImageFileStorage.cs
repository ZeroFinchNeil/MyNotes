using System.Threading;
using System.Threading.Tasks;

namespace MyNotes.Application.Contracts.Media.Persistence;

internal interface IImageFileStorage
{
  public Task Save(string originalPath, string fileName, CancellationToken cancellationToken = default);
}