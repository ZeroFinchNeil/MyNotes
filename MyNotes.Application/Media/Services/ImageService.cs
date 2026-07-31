using MyNotes.Application.Contracts.Media.Models;
using MyNotes.Application.Contracts.Media.Persistence;
using MyNotes.Application.Media.Commands;

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
    await ImageFileStorage.Save(appCommand.OriginalFilePath, imageDto.Id.Name, cancellationToken);

    return imageDto;
  }
}