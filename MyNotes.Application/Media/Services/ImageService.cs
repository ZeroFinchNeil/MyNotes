using MyNotes.Application.Contracts.Media.Persistence;

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
}