using System;
using System.Collections.Generic;
using System.Text.Json;

using MyNotes.Application.Contracts.Notes.Models.Common;
using MyNotes.Application.Contracts.Notes.Models.Creation;
using MyNotes.Application.Contracts.Notes.Models.Search;
using MyNotes.Debugging.Attributes;
using MyNotes.Domain.ValueObjects;
using MyNotes.Infrastructure.Database.Entities.Notes;
using MyNotes.Infrastructure.Search.Documents.Notes;
using MyNotes.Shared.Constants;

namespace MyNotes.Infrastructure.Mappers;

[AssemblyLocal]
internal static class NoteMappers
{
  public static NoteEntity ToEntity(CreateNoteDbRequestDto createDbDto) => new()
  {
    Id = createDbDto.Id.Value,
    Navigation = createDbDto.NavigationId.Value,
    Created = createDbDto.Created,
    Modified = createDbDto.Modified,
    Title = createDbDto.Title,
    Body = createDbDto.Body,
    BodyImagePaths = JsonSerializer.Serialize(createDbDto.BodyImagePaths, AppJson.JsonSerializerOptions),
    BackgroundColor = createDbDto.BackgroundColor,
    BackgroundImagePath = createDbDto.BackgroundImagePath,
    IsBookmarked = createDbDto.IsBookmarked,
    IsDeleted = createDbDto.IsDeleted
  };

  public static NoteViewStateEntity ToEntity(CreateNoteViewStateDbRequestDto viewStateDbDto) => new()
  {
    Id = viewStateDbDto.Id.Value,
    ShowBackgroundImage = viewStateDbDto.ShowBackgroundImage,
    BackgroundImageStretch = viewStateDbDto.BackgroundImageStretch,
    BackgroundImageAlignment = viewStateDbDto.BackgroundImageAlignment,
    BackgroundImageOpacity = viewStateDbDto.BackgroundImageOpacity,
    BackgroundImageBlur = viewStateDbDto.BackgroundImageBlur,
    BackdropKind = viewStateDbDto.BackdropKind,
    BackdropTintOpacity = viewStateDbDto.BackdropTintOpacity,
    BackdropLuminosityOpacity = viewStateDbDto.BackdropLuminosityOpacity,
    ShowImagePanel = viewStateDbDto.ShowImagePanel,
    ImagePanelHeight = viewStateDbDto.ImagePanelHeight,
    Width = viewStateDbDto.Width,
    Height = viewStateDbDto.Height,
    PositionX = viewStateDbDto.PositionX,
    PositionY = viewStateDbDto.PositionY,
    IsTextEditorReadOnly = viewStateDbDto.IsTextEditorReadOnly,
    IsWindowOpen = viewStateDbDto.IsWindowOpen,
    IsAlwaysOnTop = viewStateDbDto.IsAlwaysOnTop
  };

  public static NoteDbResponseDto ToDto(NoteEntity noteEntity) => new()
  {
    Id = NoteId.Create(noteEntity.Id),
    NavigationId = NavigationId.Create(noteEntity.Navigation ?? throw new InvalidOperationException()),
    Created = noteEntity.Created,
    Modified = noteEntity.Modified,
    Title = noteEntity.Title,
    Body = noteEntity.Body,
    BodyImagePaths = JsonSerializer.Deserialize<IReadOnlyList<string>>(noteEntity.BodyImagePaths, AppJson.JsonSerializerOptions) ?? [],
    BackgroundColor = noteEntity.BackgroundColor,
    BackgroundImagePath = noteEntity.BackgroundImagePath,
    IsBookmarked = noteEntity.IsBookmarked,
    IsDeleted = noteEntity.IsDeleted
  };

  public static NoteViewStateDbResponseDto ToDto(NoteViewStateEntity noteViewStateEntity) => new()
  {
    Id = NoteId.Create(noteViewStateEntity.Id),
    ShowBackgroundImage = noteViewStateEntity.ShowBackgroundImage,
    BackgroundImageStretch = noteViewStateEntity.BackgroundImageStretch,
    BackgroundImageAlignment = noteViewStateEntity.BackgroundImageAlignment,
    BackgroundImageOpacity = noteViewStateEntity.BackgroundImageOpacity,
    BackgroundImageBlur = noteViewStateEntity.BackgroundImageBlur,
    BackdropKind = noteViewStateEntity.BackdropKind,
    BackdropTintOpacity = noteViewStateEntity.BackdropTintOpacity,
    BackdropLuminosityOpacity = noteViewStateEntity.BackdropLuminosityOpacity,
    ShowImagePanel = noteViewStateEntity.ShowImagePanel,
    ImagePanelHeight = noteViewStateEntity.ImagePanelHeight,
    Width = noteViewStateEntity.Width,
    Height = noteViewStateEntity.Height,
    PositionX = noteViewStateEntity.PositionX,
    PositionY = noteViewStateEntity.PositionY,
    IsTextEditorReadOnly = noteViewStateEntity.IsTextEditorReadOnly,
    IsWindowOpen = noteViewStateEntity.IsWindowOpen,
    IsAlwaysOnTop = noteViewStateEntity.IsAlwaysOnTop
  };

  public static NoteBundleDbResponseDto ToDto(NoteEntity noteEntity, NoteViewStateEntity noteViewStateEntity) => new(ToDto(noteEntity), ToDto(noteViewStateEntity));

  public static NoteSearchDocument ToEntity(WriteNoteSearchDocumentRequestDto noteSearchDocumentDto) => new()
  {
    Id = noteSearchDocumentDto.Id,
    Title = noteSearchDocumentDto.Title,
    Body = noteSearchDocumentDto.Body
  };
}

internal static class NoteMappingExtensions
{
  extension(WriteNoteSearchDocumentRequestDto dto)
  {
    public NoteSearchDocument ToEntity() => NoteMappers.ToEntity(dto);
  }
}
