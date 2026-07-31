using System.Threading;
using System.Threading.Tasks;

using MyNotes.Application.Contracts.Media.Models;
using MyNotes.Domain.Media;

namespace MyNotes.Application.Contracts.Media.Persistence;

internal interface IImageRepository
{
  public Task<ImageId> GenerateUniqueImageIdAsync(CancellationToken cancellationToken = default);

  public Task AttachImageAsync(ImageDto imageDto, CancellationToken cancellationToken = default);
}