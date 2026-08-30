using System.Threading;
using System.Threading.Tasks;

namespace MyNotes.Application.Contracts.Media.Persistence;

internal interface IImageFileStorage
{
  public Task SaveImage(string originalPath, string fileName, CancellationToken cancellationToken = default);
  public Task DeleteImage(string fileNameWithoutExtension, CancellationToken cancellationToken = default);
}