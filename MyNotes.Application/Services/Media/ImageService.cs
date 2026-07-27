using MyNotes.Application.Contracts.Persistence.Media;

namespace MyNotes.Application.Services.Media;

internal sealed class ImageService
{
  private readonly IImageRepository ImageRepository;
  private readonly IImageFileStorage ImageFileStorage;

  public ImageService(IImageRepository imageRepository, IImageFileStorage imageFileStorage)
  {
    ImageRepository = imageRepository;
    ImageFileStorage = imageFileStorage;
  }
}