using MyNotes.Application.Contracts.Media.Models;
using MyNotes.Application.Contracts.Media.Persistence;
using MyNotes.Application.Media.Commands;
using MyNotes.Application.Media.Results;
using MyNotes.Domain.Media;
using MyNotes.Domain.Notes;

namespace MyNotes.Application.Media.Services;

internal sealed class ImageService
{
  private readonly IImageRepository ImageRepository;
  private readonly IImageFileStorage ImageFileStorage;

  public ImageService(IImageRepository imageRepository, IImageFileStorage imageFileStorage)
  {
    ImageRepository = imageRepository;
    ImageFileStorage = imageFileStorage;
  }

  public async Task<ImageDto> AttachImageAsync(AttachImageAppCommand appCommand, CancellationToken cancellationToken = default)
  {
    ImageDto imageDto = new()
    {
      Id = await ImageRepository.GenerateUniqueImageIdAsync(cancellationToken),
      NoteId = appCommand.NoteId,
      OriginalFileName = System.IO.Path.GetFileNameWithoutExtension(appCommand.OriginalFilePath),
      StoredExtension = System.IO.Path.GetExtension(appCommand.OriginalFilePath)
    };
    await ImageRepository.AttachImageAsync(imageDto, cancellationToken);
    await ImageFileStorage.SaveImage(appCommand.OriginalFilePath, imageDto.Id.Name, cancellationToken);

    return imageDto;
  }

  public async Task<IReadOnlyList<ImageDto>> GetImagesAsync(NoteId noteId, CancellationToken cancellationToken = default)
  {
    return await ImageRepository.GetImagesAsync(noteId, cancellationToken);
  }

  public async Task DeleteImageAsync(ImageId imageId, CancellationToken cancellationToken = default)
  {
    await ImageRepository.DeleteImageAsync(imageId, cancellationToken);
    await ImageFileStorage.DeleteImage(imageId.Name, cancellationToken);
  }

  public async Task MoveImageAsync(MoveImageAppCommand appCommand, CancellationToken cancellationToken = default)
  {
    await ImageRepository.MoveImageAsync(appCommand.SourceId, appCommand.TargetId, cancellationToken);
  }
}