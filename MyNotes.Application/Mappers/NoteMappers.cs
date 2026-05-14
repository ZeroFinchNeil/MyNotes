using MyNotes.Application.Contracts.Database.Dtos.Notes;
using MyNotes.Application.Contracts.Database.Queries.Notes;
using MyNotes.Application.Contracts.Search.Dtos.Notes;
using MyNotes.Application.Dtos.Notes;
using MyNotes.Application.Queries.Notes;
using MyNotes.Domain.Entities.Notes;

namespace MyNotes.Application.Mappers;

internal static class NoteMappers
{
  public static NoteAppResponseDto ToAppDto(NoteDbResponseDto noteDbResponseDto, NoteViewStateDbResponseDto noteViewStateDbResponseDto) => new()
  {
    Id = noteDbResponseDto.Id,
    NavigationId = noteDbResponseDto.NavigationId,
    Created = noteDbResponseDto.Created,
    Modified = noteDbResponseDto.Modified,
    Title = noteDbResponseDto.Title,
    Body = noteDbResponseDto.Body,
    BodyPlainText = RtfTextConverter.ToPlainText(noteDbResponseDto.Body),
    IsBookmarked = noteDbResponseDto.IsBookmarked,
    IsDeleted = noteDbResponseDto.IsDeleted,
    ShowBackgroundImage = noteViewStateDbResponseDto.ShowBackgroundImage,
    BackgroundImagePath = noteViewStateDbResponseDto.BackgroundImagePath,
    BackgroundImageOpacity = noteViewStateDbResponseDto.BackgroundImageOpacity,
    BackgroundImageBlur = noteViewStateDbResponseDto.BackgroundImageBlur,
    BackdropKind = noteViewStateDbResponseDto.BackdropKind,
    BackdropTintOpacity = noteViewStateDbResponseDto.BackdropTintOpacity,
    BackdropLuminosityOpacity = noteViewStateDbResponseDto.BackdropLuminosityOpacity,
    Images = noteViewStateDbResponseDto.Images,
    ShowImagePanel = noteViewStateDbResponseDto.ShowImagePanel,
    ImagePanelHeight = noteViewStateDbResponseDto.ImagePanelHeight,
    Width = noteViewStateDbResponseDto.Width,
    Height = noteViewStateDbResponseDto.Height,
    PositionX = noteViewStateDbResponseDto.PositionX,
    PositionY = noteViewStateDbResponseDto.PositionY,
    IsWindowOpen = noteViewStateDbResponseDto.IsWindowOpen,
    IsAlwaysOnTop = noteViewStateDbResponseDto.IsAlwaysOnTop
  };

  public static CreateNoteDbRequestDto ToDbDto(Note note) => new()
  {
    Id = note.Id,
    NavigationId = note.NavigationId,
    Created = note.Created,
    Modified = note.Modified,
    Title = note.Title,
    Body = note.Body,
    BackgroundColor = note.BackgroundColor,
    IsBookmarked = note.IsBookmarked,
    IsDeleted = note.IsDeleted
  };

  public static FindNotesDbQuery ToDbQuery(FindNotesAppQuery findNotesQuery) => new()
  {
    NoteId = findNotesQuery.NoteId,
    NavigationId = findNotesQuery.NavigationId,
    TitleConditions = findNotesQuery.TitleConditions,
    CreatedConditions = findNotesQuery.CreatedConditions,
    ModifiedConditions = findNotesQuery.ModifiedConditions
  };

  public static UpdateNoteDbRequestDto ToDbDto(UpdateNoteAppRequestDto updateNoteAppDto) => new()
  {
    Id = updateNoteAppDto.Id,
    NoteUpdateField = updateNoteAppDto.NoteUpdateField,
    NavigationId = updateNoteAppDto.NavigationId,
    Created = updateNoteAppDto.Created,
    Modified = updateNoteAppDto.Modified,
    Title = updateNoteAppDto.Title,
    Body = updateNoteAppDto.Body,
    BodyPlainText = updateNoteAppDto.BodyPlainText,
    IsBookmarked = updateNoteAppDto.IsBookmarked,
    IsDeleted = updateNoteAppDto.IsDeleted
  };

  public static UpdateNoteViewStateDbRequestDto ToDbDto(UpdateNoteViewStateAppRequestDto updateNoteViewStateDto) => new()
  {
    Id = updateNoteViewStateDto.Id,
    NoteViewStateUpdateField = updateNoteViewStateDto.NoteViewStateUpdateField,
    ShowBackgroundImage = updateNoteViewStateDto.ShowBackgroundImage,
    BackgroundImagePath = updateNoteViewStateDto.BackgroundImagePath,
    BackgroundImageOpacity = updateNoteViewStateDto.BackgroundImageOpacity,
    BackgroundImageBlur = updateNoteViewStateDto.BackgroundImageBlur,
    BackdropKind = updateNoteViewStateDto.BackdropKind,
    BackdropTintOpacity = updateNoteViewStateDto.BackdropTintOpacity,
    BackdropLuminosityOpacity = updateNoteViewStateDto.BackdropLuminosityOpacity,
    Images = updateNoteViewStateDto.Images,
    ShowImagePanel = updateNoteViewStateDto.ShowImagePanel,
    ImagePanelHeight = updateNoteViewStateDto.ImagePanelHeight,
    Width = updateNoteViewStateDto.Width,
    Height = updateNoteViewStateDto.Height,
    PositionX = updateNoteViewStateDto.PositionX,
    PositionY = updateNoteViewStateDto.PositionY,
    IsWindowOpen = updateNoteViewStateDto.IsWindowOpen,
    IsAlwaysOnTop = updateNoteViewStateDto.IsAlwaysOnTop
  };

  public static NoteSearchDocumentDto ToSearchDocumentDto(NoteDbResponseDto noteDbResponseDto) => new()
  {
    Id = noteDbResponseDto.Id.Value,
    Title = noteDbResponseDto.Title,
    Body = RtfTextConverter.ToPlainText(noteDbResponseDto.Body)
  };
}
