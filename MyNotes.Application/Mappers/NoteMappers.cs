using MyNotes.Application.Contracts.Database.Enums.Notes;
using MyNotes.Application.Contracts.Notes.Models.Common;
using MyNotes.Application.Contracts.Notes.Models.Creation;
using MyNotes.Application.Contracts.Notes.Models.Modification;
using MyNotes.Application.Contracts.Notes.Models.Queries;
using MyNotes.Application.Contracts.Notes.Models.Search;
using MyNotes.Application.Dtos.Notes.Common;
using MyNotes.Application.Dtos.Notes.Modification;
using MyNotes.Application.Dtos.Notes.Queries;
using MyNotes.Debugging.Attributes;
using MyNotes.Domain.Entities.Notes;
using MyNotes.Domain.ValueObjects;
using MyNotes.Shared.Queries.Conditions;

namespace MyNotes.Application.Mappers;

[AssemblyLocal]
internal static class NoteMappers
{
  public static NoteBundleAppResponseDto ToAppDto(NoteBundleDbResponseDto dbResponseDto) => new(ToAppDto(dbResponseDto.NoteDto), ToAppDto(dbResponseDto.ViewStateDto));

  public static NoteAppResponseDto ToAppDto(NoteDbResponseDto dbResponseDto) => new()
  {
    Id = dbResponseDto.Id,
    NavigationId = dbResponseDto.NavigationId,
    Created = dbResponseDto.Created,
    Modified = dbResponseDto.Modified,
    Title = dbResponseDto.Title,
    Body = dbResponseDto.Body,
    BodyImagePaths = dbResponseDto.BodyImagePaths,
    BackgroundColor = dbResponseDto.BackgroundColor,
    BackgroundImagePath = dbResponseDto.BackgroundImagePath,
    IsBookmarked = dbResponseDto.IsBookmarked,
    IsDeleted = dbResponseDto.IsDeleted,
  };

  public static NoteViewStateAppResponseDto ToAppDto(NoteViewStateDbResponseDto dbResponseDto) => new()
  {
    Id = dbResponseDto.Id,
    ShowBackgroundImage = dbResponseDto.ShowBackgroundImage,
    BackgroundImageStretch = dbResponseDto.BackgroundImageStretch,
    BackgroundImageAlignment = dbResponseDto.BackgroundImageAlignment,
    BackgroundImageOpacity = dbResponseDto.BackgroundImageOpacity,
    BackgroundImageBlur = dbResponseDto.BackgroundImageBlur,
    BackdropKind = dbResponseDto.BackdropKind,
    BackdropTintOpacity = dbResponseDto.BackdropTintOpacity,
    BackdropLuminosityOpacity = dbResponseDto.BackdropLuminosityOpacity,
    ShowImagePanel = dbResponseDto.ShowImagePanel,
    ImagePanelHeight = dbResponseDto.ImagePanelHeight,
    Width = dbResponseDto.Width,
    Height = dbResponseDto.Height,
    PositionX = dbResponseDto.PositionX,
    PositionY = dbResponseDto.PositionY,
    IsTextEditorReadOnly = dbResponseDto.IsTextEditorReadOnly,
    IsWindowOpen = dbResponseDto.IsWindowOpen,
    IsAlwaysOnTop = dbResponseDto.IsAlwaysOnTop
  };

  public static UpdateNoteAppResponseDto ToAppDto(UpdateNoteDbResponseDto dbResponseDto) => new()
  {
    Id = dbResponseDto.Id,
    NavigationId = dbResponseDto.NavigationId,
    Modified = dbResponseDto.Modified,
    Title = dbResponseDto.Title,
    Body = dbResponseDto.Body,
    BodyImagePaths = dbResponseDto.BodyImagePaths,
    BackgroundColor = dbResponseDto.BackgroundColor,
    BackgroundImagePath = dbResponseDto.BackgroundImagePath,
    IsBookmarked = dbResponseDto.IsBookmarked,
    IsDeleted = dbResponseDto.IsDeleted
  };

  public static UpdateNoteViewStateAppResponseDto ToAppDto(UpdateNoteViewStateDbResponseDto dbResponseDto) => new()
  {
    Id = dbResponseDto.Id,
  };

  public static CreateNoteDbRequestDto ToCreateDbDto(Note note) => new()
  {
    Id = note.Id,
    NavigationId = note.NavigationId,
    Created = note.Created,
    Modified = note.Modified,
    Title = note.Title,
    Body = note.Body,
    BodyImagePaths = note.BodyImagePaths,
    BackgroundColor = note.BackgroundColor,
    BackgroundImagePath = note.BackgroundImagePath,
    IsBookmarked = note.IsBookmarked,
    IsDeleted = note.IsDeleted
  };

  public static FindNotesDbQuery ToDbQuery(FindNotesAppQuery query)
  {
    var noteFindFields = query.FindFields;

    if (noteFindFields == NoteFindFields.None)
    {
      throw new ArgumentException("", nameof(query));
    }
    query.ThrowIfInvalid();
    return new()
    {
      NoteFindFields = noteFindFields,
      AggregationMode = query.AggregationMode,
      NoteIdCondition = query.NoteIdCondition is null ? null : EqualityQueryCondition<Guid>.Create(query.NoteIdCondition.Target.Value, query.NoteIdCondition.Condition),
      ParentIdCondition = query.ParentIdCondition is null ? null : EqualityQueryCondition<Guid>.Create(query.ParentIdCondition.Target.Value, query.ParentIdCondition.Condition),
      TitleConditions = query.TitleConditions,
      CreatedConditions = query.CreatedConditions,
      ModifiedConditions = query.ModifiedConditions,
      BackgroundColorConditions = query.BackgroundColorConditions,
      BookmarkedCondition = query.BookmarkedCondition,
      DeletedCondition = query.DeletedCondition
    };
  }

  public static UpdateNoteDbRequestDto ToDbDto(UpdateNoteAppRequestDto updateAppRequestDto, DateTimeOffset modified) => new()
  {
    Id = updateAppRequestDto.Id,
    NavigationId = updateAppRequestDto.NavigationId,
    Modified = modified,
    Title = updateAppRequestDto.Title,
    Body = updateAppRequestDto.Body,
    BodyImagePaths = updateAppRequestDto.BodyImagePaths,
    BackgroundColor = updateAppRequestDto.BackgroundColor,
    BackgroundImagePath = updateAppRequestDto.BackgroundImagePath,
    IsBookmarked = updateAppRequestDto.IsBookmarked,
    IsDeleted = updateAppRequestDto.IsDeleted
  };

  public static DeleteNoteDbRequestDto ToDbDto(DeleteNoteAppRequestDto deleteAppRequestDto) => new()
  {
    Id = deleteAppRequestDto.Id,
    DeleteMode = deleteAppRequestDto.DeleteMode
  };

  public static WriteNoteSearchDocumentRequestDto ToSearchDocumentDto(NoteId noteId, string title, string bodyPlainText) => new()
  {
    Id = noteId.Value,
    Title = title,
    Body = bodyPlainText
  };

  public static UpdateNoteViewStateDbRequestDto ToDbDto(UpdateNoteViewStateAppRequestDto updateAppRequestDto) => new()
  {
    Id = updateAppRequestDto.Id,
    ShowBackgroundImage = updateAppRequestDto.ShowBackgroundImage,
    BackgroundImageStretch = updateAppRequestDto.BackgroundImageStretch,
    BackgroundImageAlignment = updateAppRequestDto.BackgroundImageAlignment,
    BackgroundImageOpacity = updateAppRequestDto.BackgroundImageOpacity,
    BackgroundImageBlur = updateAppRequestDto.BackgroundImageBlur,
    BackdropKind = updateAppRequestDto.BackdropKind,
    BackdropTintOpacity = updateAppRequestDto.BackdropTintOpacity,
    BackdropLuminosityOpacity = updateAppRequestDto.BackdropLuminosityOpacity,
    ShowImagePanel = updateAppRequestDto.ShowImagePanel,
    ImagePanelHeight = updateAppRequestDto.ImagePanelHeight,
    Width = updateAppRequestDto.Width,
    Height = updateAppRequestDto.Height,
    PositionX = updateAppRequestDto.PositionX,
    PositionY = updateAppRequestDto.PositionY,
    IsTextEditorReadOnly = updateAppRequestDto.IsTextEditorReadOnly,
    IsWindowOpen = updateAppRequestDto.IsWindowOpen,
    IsAlwaysOnTop = updateAppRequestDto.IsAlwaysOnTop
  };
}

internal static class NoteMappingExtensions
{
  extension(Note note)
  {
    public CreateNoteDbRequestDto ToCreateDbDto() => NoteMappers.ToCreateDbDto(note);
  }

  extension(NoteBundleDbResponseDto dto)
  {
    public NoteBundleAppResponseDto ToAppDto() => NoteMappers.ToAppDto(dto);
  }
}