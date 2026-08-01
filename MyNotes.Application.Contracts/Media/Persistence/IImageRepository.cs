using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MyNotes.Application.Contracts.Media.Models;
using MyNotes.Domain.Media;
using MyNotes.Domain.Notes;

namespace MyNotes.Application.Contracts.Media.Persistence;

internal interface IImageRepository
{
  public Task<ImageId> GenerateUniqueImageIdAsync(CancellationToken cancellationToken = default);

  public Task AttachImageAsync(ImageDto imageDto, CancellationToken cancellationToken = default);

  public Task<IReadOnlyList<ImageDto>> GetImagesAsync(NoteId noteId, CancellationToken cancellationToken = default);

  public Task DeleteImageAsync(ImageId imageId, CancellationToken cancellationToken = default);

  public Task MoveImageAsync(ImageId sourceImageId, ImageId targetImageId, CancellationToken cancellationToken = default);
}