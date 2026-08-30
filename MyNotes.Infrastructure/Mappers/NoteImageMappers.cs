using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Debugging.Attributes;
using MyNotes.Domain.Media;
using MyNotes.Domain.Notes;
using MyNotes.Infrastructure.Database.Entities.Notes;

namespace MyNotes.Infrastructure.Mappers;

[AssemblyLocal]
internal static class NoteImageMappers
{
  public static NoteImageEntity ToEntity(NoteImageDto imageDto, int position) => new()
  {
    Id = imageDto.Id.Value,
    NoteId = imageDto.NoteId.Value,
    OriginalFileName = imageDto.OriginalFileName,
    StoredExtension = imageDto.StoredExtension,
    Position = position
  };

  public static NoteImageDto ToDto(NoteImageEntity imageEntity) => new()
  {
    Id = ImageId.Create(imageEntity.Id),
    NoteId = NoteId.Create(imageEntity.NoteId),
    OriginalFileName = imageEntity.OriginalFileName,
    StoredExtension = imageEntity.StoredExtension
  };
}

internal static class NoteImageMappersExtensions
{
  extension(NoteImageEntity entity)
  {
    public NoteImageDto ToDto() => NoteImageMappers.ToDto(entity);
  }
}