using System;
using System.Collections.Generic;
using System.Text.Json;

using MyNotes.Application.Contracts.Models.Notes;
using MyNotes.Debugging.Attributes;
using MyNotes.Domain.ValueObjects;
using MyNotes.Infrastructure.Database.Entities.Notes;
using MyNotes.Infrastructure.Search.Documents.Notes;
using MyNotes.Shared.Constants;

namespace MyNotes.Infrastructure.Mappers;

[AssemblyLocal]
internal static class NoteMappers
{
  public static NoteEntity ToEntity(NoteDto noteDto) => new()
  {
    Id = noteDto.Id.Value,
    Navigation = noteDto.NavigationId.Value,
    Created = noteDto.Created,
    Modified = noteDto.Modified,
    Title = noteDto.Title,
    Body = noteDto.Body,
    BodyImagePaths = JsonSerializer.Serialize(noteDto.BodyImagePaths, AppJson.JsonSerializerOptions),
    BackgroundColor = noteDto.BackgroundColor,
    BackgroundImagePath = noteDto.BackgroundImagePath,
    IsBookmarked = noteDto.IsBookmarked,
    IsDeleted = noteDto.IsDeleted
  };

  public static NoteViewStateEntity ToEntity(NoteViewStateDto noteViewStateDbDto) => new()
  {
    Id = noteViewStateDbDto.Id.Value,
    ShowBackgroundImage = noteViewStateDbDto.ShowBackgroundImage,
    BackgroundImageStretch = noteViewStateDbDto.BackgroundImageStretch,
    BackgroundImageAlignment = noteViewStateDbDto.BackgroundImageAlignment,
    BackgroundImageOpacity = noteViewStateDbDto.BackgroundImageOpacity,
    BackgroundImageBlur = noteViewStateDbDto.BackgroundImageBlur,
    BackdropKind = noteViewStateDbDto.BackdropKind,
    BackdropTintOpacity = noteViewStateDbDto.BackdropTintOpacity,
    BackdropLuminosityOpacity = noteViewStateDbDto.BackdropLuminosityOpacity,
    ShowImagePanel = noteViewStateDbDto.ShowImagePanel,
    ImagePanelHeight = noteViewStateDbDto.ImagePanelHeight,
    Width = noteViewStateDbDto.Width,
    Height = noteViewStateDbDto.Height,
    PositionX = noteViewStateDbDto.PositionX,
    PositionY = noteViewStateDbDto.PositionY,
    IsTextEditorReadOnly = noteViewStateDbDto.IsTextEditorReadOnly,
    IsWindowOpen = noteViewStateDbDto.IsWindowOpen,
    IsAlwaysOnTop = noteViewStateDbDto.IsAlwaysOnTop
  };

  public static NoteDto ToDto(NoteEntity noteEntity) => new()
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

  public static NoteViewStateDto ToDto(NoteViewStateEntity noteViewStateEntity) => new()
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

  public static NoteBundleDto ToDto(NoteEntity noteEntity, NoteViewStateEntity noteViewStateEntity) => new(ToDto(noteEntity), ToDto(noteViewStateEntity));

  public static NoteSearchDocument ToEntity(NoteSearchDocumentDto noteSearchDocumentDto) => new()
  {
    Id = noteSearchDocumentDto.Id,
    Title = noteSearchDocumentDto.Title,
    Body = noteSearchDocumentDto.Body
  };
}
