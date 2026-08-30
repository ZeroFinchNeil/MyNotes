using System.IO;

using MyNotes.Application.Contracts.Media.Persistence;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Contracts.Notes.Persistence;
using MyNotes.Application.Notes.Commands;
using MyNotes.Domain.Media;
using MyNotes.Domain.Notes;

namespace MyNotes.Application.Notes.Services;

internal sealed class NoteImageService
{
  private readonly INoteImageRepository NoteImageRepository;
  private readonly IImageFileStorage ImageFileStorage;

  public NoteImageService(INoteImageRepository noteImageRepository, IImageFileStorage imageFileStorage)
  {
    NoteImageRepository = noteImageRepository;
    ImageFileStorage = imageFileStorage;
  }

  public async Task<NoteImageDto> AttachImageAsync(AttachNoteImageAppCommand appCommand, CancellationToken cancellationToken = default)
  {
    NoteImageDto imageDto = new()
    {
      Id = await NoteImageRepository.GenerateUniqueImageIdAsync(cancellationToken),
      NoteId = appCommand.NoteId,
      OriginalFileName = Path.GetFileNameWithoutExtension(appCommand.OriginalFilePath),
      StoredExtension = Path.GetExtension(appCommand.OriginalFilePath)
    };
    await NoteImageRepository.AttachImageAsync(imageDto, cancellationToken);
    await ImageFileStorage.SaveImage(appCommand.OriginalFilePath, Path.ChangeExtension(imageDto.Id.Name, imageDto.StoredExtension), cancellationToken);

    return imageDto;
  }

  public Task<NoteImageDto?> GetImageAsync(ImageId imageId, CancellationToken cancellationToken = default) => NoteImageRepository.GetImageAsync(imageId, cancellationToken);

  public Task<IReadOnlyList<NoteImageDto>> GetImagesByNoteIdAsync(NoteId noteId, CancellationToken cancellationToken = default) => NoteImageRepository.GetImagesByNoteIdAsync(noteId, cancellationToken);

  public async Task DeleteImageAsync(ImageId imageId, CancellationToken cancellationToken = default)
  {
    await NoteImageRepository.DeleteImageAsync(imageId, cancellationToken);
    await ImageFileStorage.DeleteImage(imageId.Name, cancellationToken);
  }

  public async Task MoveImageAsync(MoveNoteImageAppCommand appCommand, CancellationToken cancellationToken = default)
  {
    await NoteImageRepository.MoveImageAsync(appCommand.SourceId, appCommand.TargetId, cancellationToken);
  }
}