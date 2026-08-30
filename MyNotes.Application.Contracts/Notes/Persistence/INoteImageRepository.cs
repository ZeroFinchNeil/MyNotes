using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Domain.Media;
using MyNotes.Domain.Notes;

namespace MyNotes.Application.Contracts.Notes.Persistence;

internal interface INoteImageRepository
{
  public Task<ImageId> GenerateUniqueImageIdAsync(CancellationToken cancellationToken = default);

  public Task AttachImageAsync(NoteImageDto imageDto, CancellationToken cancellationToken = default);

  public Task<NoteImageDto?> GetImageAsync(ImageId imageId, CancellationToken cancellationToken = default);

  public Task<IReadOnlyList<NoteImageDto>> GetImagesByNoteIdAsync(NoteId noteId, CancellationToken cancellationToken = default);

  public Task DeleteImageAsync(ImageId imageId, CancellationToken cancellationToken = default);

  public Task MoveImageAsync(ImageId sourceImageId, ImageId targetImageId, CancellationToken cancellationToken = default);
}