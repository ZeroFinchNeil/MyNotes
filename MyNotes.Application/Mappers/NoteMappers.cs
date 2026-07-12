using MyNotes.Application.Contracts.Database.Dtos.Notes.Common;
using MyNotes.Application.Contracts.Database.Dtos.Notes.Creation;
using MyNotes.Application.Contracts.Database.Dtos.Notes.Modification;
using MyNotes.Application.Contracts.Database.Dtos.Notes.Queries;
using MyNotes.Application.Contracts.Database.Enums.Notes;
using MyNotes.Application.Contracts.Search.Dtos.Notes;
using MyNotes.Application.Dtos.Notes.Common;
using MyNotes.Application.Dtos.Notes.Modification;
using MyNotes.Application.Dtos.Notes.Queries;
using MyNotes.Debugging.Attributes;
using MyNotes.Domain.Entities.Notes;
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
    BodyPlainText = dbResponseDto.Body,
    BackgroundColor = dbResponseDto.BackgroundColor,
    IsBookmarked = dbResponseDto.IsBookmarked,
    IsDeleted = dbResponseDto.IsDeleted,
  };

  public static NoteViewStateAppResponseDto ToAppDto(NoteViewStateDbResponseDto dbResponseDto) => new()
  {
    Id = dbResponseDto.Id,
    ShowBackgroundImage = dbResponseDto.ShowBackgroundImage,
    BackgroundImagePath = dbResponseDto.BackgroundImagePath,
    BackgroundImageOpacity = dbResponseDto.BackgroundImageOpacity,
    BackgroundImageBlur = dbResponseDto.BackgroundImageBlur,
    BackdropKind = dbResponseDto.BackdropKind,
    BackdropTintOpacity = dbResponseDto.BackdropTintOpacity,
    BackdropLuminosityOpacity = dbResponseDto.BackdropLuminosityOpacity,
    Images = dbResponseDto.Images,
    ShowImagePanel = dbResponseDto.ShowImagePanel,
    ImagePanelHeight = dbResponseDto.ImagePanelHeight,
    Width = dbResponseDto.Width,
    Height = dbResponseDto.Height,
    PositionX = dbResponseDto.PositionX,
    PositionY = dbResponseDto.PositionY,
    IsWindowOpen = dbResponseDto.IsWindowOpen,
    IsAlwaysOnTop = dbResponseDto.IsAlwaysOnTop
  };

  public static CreateNoteDbRequestDto ToCreateDbDto(Note note) => new()
  {
    Id = note.Id,
    NavigationId = note.ParentId,
    Created = note.Created,
    Modified = note.Modified,
    Title = note.Title,
    Body = note.Body,
    BackgroundColor = note.BackgroundColor,
    IsBookmarked = note.IsBookmarked,
    IsDeleted = note.IsDeleted
  };

  public static FindNotesDbQuery ToDbQuery(FindNotesAppQuery query)
  {
    var noteFindFields = query.NoteFindFields;

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

  public static UpdateNoteDbRequestDto ToDbDto(UpdateNoteAppRequestDto updateNoteAppDto) => new()
  {
    Id = updateNoteAppDto.Id,
    NoteUpdateField = updateNoteAppDto.NoteUpdateField,
    NavigationId = updateNoteAppDto.ParentId,
    Created = updateNoteAppDto.Created,
    Modified = updateNoteAppDto.Modified,
    Title = updateNoteAppDto.Title,
    Body = updateNoteAppDto.Body,
    BodyPlainText = updateNoteAppDto.BodyPlainText,
    IsBookmarked = updateNoteAppDto.IsBookmarked,
    IsDeleted = updateNoteAppDto.IsDeleted
  };

  public static NoteSearchDocumentDto ToSearchDocumentDto(NoteDbResponseDto noteDbResponseDto) => new()
  {
    Id = noteDbResponseDto.Id.Value,
    Title = noteDbResponseDto.Title,
    Body = RtfTextConverter.ToPlainText(noteDbResponseDto.Body)
  };

  public static UpdateNoteViewStateDbRequestDto ToDbDto(UpdateNoteViewStateAppRequestDto updateNoteViewStateDto) => throw new NotImplementedException();
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