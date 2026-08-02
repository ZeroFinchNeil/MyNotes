using MyNotes.Application.Contracts.Media.Models;
using MyNotes.Models.Media;

namespace MyNotes.Common.Mappers;

internal static class ImageMapper
{
  public static ImageDescriptor ToModel(ImageDto imageDto) => new()
  {
    Id = imageDto.Id
  };
}