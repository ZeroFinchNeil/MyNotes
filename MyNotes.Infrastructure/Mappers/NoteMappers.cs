using MyNotes.Application.Contracts.Database.Dtos.Notes.Common;
using MyNotes.Application.Contracts.Database.Dtos.Notes.Creation;
using MyNotes.Application.Contracts.Search.Dtos.Notes;
using MyNotes.Debugging.Attributes;
using MyNotes.Domain.ValueObjects;
using MyNotes.Infrastructure.Database.Entities.Notes;
using MyNotes.Infrastructure.Search.Documents.Notes;

namespace MyNotes.Infrastructure.Mappers;

[AssemblyLocal]
internal static class NoteMappers
{
  public static NoteEntity ToEntity(CreateNoteDbRequestDto createDbDto) => new()
  {
    Id = createDbDto.Id.Value,
    Parent = createDbDto.NavigationId.Value,
    Created = createDbDto.Created,
    Modified = createDbDto.Modified,
    Title = createDbDto.Title,
    Body = createDbDto.Body,
    BackgroundColor = createDbDto.BackgroundColor,
    IsBookmarked = createDbDto.IsBookmarked,
    IsDeleted = createDbDto.IsDeleted
  };

  public static NoteViewStateEntity ToEntity(CreateNoteViewStateDbRequestDto viewStateDbDto) => new()
  {
    Id = viewStateDbDto.Id.Value,
    ShowBackgroundImage = viewStateDbDto.ShowBackgroundImage,
    BackgroundImagePath = viewStateDbDto.BackgroundImagePath,
    BackgroundImageOpacity = viewStateDbDto.BackgroundImageOpacity,
    BackgroundImageBlur = viewStateDbDto.BackgroundImageBlur,
    BackdropKind = viewStateDbDto.BackdropKind,
    BackdropTintOpacity = viewStateDbDto.BackdropTintOpacity,
    BackdropLuminosityOpacity = viewStateDbDto.BackdropLuminosityOpacity,
    Images = "{}",
    ShowImagePanel = viewStateDbDto.ShowImagePanel,
    ImagePanelHeight = viewStateDbDto.ImagePanelHeight,
    Width = viewStateDbDto.Width,
    Height = viewStateDbDto.Height,
    PositionX = viewStateDbDto.PositionX,
    PositionY = viewStateDbDto.PositionY,
    IsWindowOpen = viewStateDbDto.IsWindowOpen,
    IsAlwaysOnTop = viewStateDbDto.IsAlwaysOnTop
  };

  public static NoteDbResponseDto ToDto(NoteEntity noteEntity) => new()
  {
    Id = NoteId.Create(noteEntity.Id),
    NavigationId = NavigationId.Create(noteEntity.Parent),
    Created = noteEntity.Created,
    Modified = noteEntity.Modified,
    Title = noteEntity.Title,
    Body = noteEntity.Body,
    BackgroundColor = noteEntity.BackgroundColor,
    IsBookmarked = noteEntity.IsBookmarked,
    IsDeleted = noteEntity.IsDeleted
  };

  public static NoteViewStateDbResponseDto ToDto(NoteViewStateEntity noteViewStateEntity) => new()
  {
    Id = NoteId.Create(noteViewStateEntity.Id),
    ShowBackgroundImage = noteViewStateEntity.ShowBackgroundImage,
    BackgroundImagePath = noteViewStateEntity.BackgroundImagePath,
    BackgroundImageOpacity = noteViewStateEntity.BackgroundImageOpacity,
    BackgroundImageBlur = noteViewStateEntity.BackgroundImageBlur,
    BackdropKind = noteViewStateEntity.BackdropKind,
    BackdropTintOpacity = noteViewStateEntity.BackdropTintOpacity,
    BackdropLuminosityOpacity = noteViewStateEntity.BackdropLuminosityOpacity,
    Images = [],
    ShowImagePanel = noteViewStateEntity.ShowImagePanel,
    ImagePanelHeight = noteViewStateEntity.ImagePanelHeight,
    Width = noteViewStateEntity.Width,
    Height = noteViewStateEntity.Height,
    PositionX = noteViewStateEntity.PositionX,
    PositionY = noteViewStateEntity.PositionY,
    IsWindowOpen = noteViewStateEntity.IsWindowOpen,
    IsAlwaysOnTop = noteViewStateEntity.IsAlwaysOnTop
  };

  public static NoteBundleDbResponseDto ToDto(NoteEntity noteEntity, NoteViewStateEntity noteViewStateEntity) => new(ToDto(noteEntity), ToDto(noteViewStateEntity));

  public static NoteSearchDocument ToEntity(NoteSearchDocumentDto noteSearchDocumentDto) => new()
  {
    Id = noteSearchDocumentDto.Id,
    Title = noteSearchDocumentDto.Title,
    Body = noteSearchDocumentDto.Body
  };
}

internal static class NoteMappingExtensions
{
  extension(NoteSearchDocumentDto dto)
  {
    public NoteSearchDocument ToEntity() => NoteMappers.ToEntity(dto);
  }
}
