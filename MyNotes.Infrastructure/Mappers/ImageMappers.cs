using MyNotes.Application.Contracts.Media.Models;
using MyNotes.Debugging.Attributes;
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
}