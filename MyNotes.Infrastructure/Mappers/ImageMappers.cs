using MyNotes.Application.Contracts.Media.Models;
using MyNotes.Debugging.Attributes;
using MyNotes.Domain.Media;
using MyNotes.Domain.Notes;
using MyNotes.Infrastructure.Database.Entities.Media;

namespace MyNotes.Infrastructure.Mappers;

[AssemblyLocal]
internal static class ImageMappers
{
  public static ImageEntity ToEntity(ImageDto imageDto, int position) => new()
  {
    Id = imageDto.Id.Value,
    NoteId = imageDto.NoteId.Value,
    OriginalFileName = imageDto.OriginalFileName,
    StoredExtension = imageDto.StoredExtension,
    Position = position
  };

  public static ImageDto ToDto(ImageEntity imageEntity) => new()
  {
    Id = ImageId.Create(imageEntity.Id),
    NoteId = NoteId.Create(imageEntity.NoteId),
    OriginalFileName = imageEntity.OriginalFileName,
    StoredExtension = imageEntity.StoredExtension
  };
}

internal static class ImageMappersExtensions
{
  extension(ImageEntity entity)
  {
    public ImageDto ToDto() => ImageMappers.ToDto(entity);
  }
}