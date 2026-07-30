using System;

using MyNotes.Application.Contracts.Media.Models;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Debugging.Attributes;
using MyNotes.Domain.Navigations;
using MyNotes.Domain.Notes;
using MyNotes.Infrastructure.Database.Entities.Notes;
using MyNotes.Infrastructure.Search.Documents.Notes;

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
    BackgroundImageAlignment = (int)noteViewStateDbDto.BackgroundImageAlignment,
    BackgroundImageOpacity = noteViewStateDbDto.BackgroundImageOpacity,
    BackgroundImageBlur = noteViewStateDbDto.BackgroundImageBlur,
    BackdropKind = (int)noteViewStateDbDto.BackdropKind,
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

  public static NoteViewStateDto ToDto(NoteViewStateEntity noteViewStateEntity) => new()
  {
    Id = NoteId.Create(noteViewStateEntity.Id),
    ShowBackgroundImage = noteViewStateEntity.ShowBackgroundImage,
    BackgroundImageStretch = noteViewStateEntity.BackgroundImageStretch,
    BackgroundImageAlignment = (AlignmentPosition)noteViewStateEntity.BackgroundImageAlignment,
    BackgroundImageOpacity = noteViewStateEntity.BackgroundImageOpacity,
    BackgroundImageBlur = noteViewStateEntity.BackgroundImageBlur,
    BackdropKind = (BackdropKind)noteViewStateEntity.BackdropKind,
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

  public static NoteDto ToDto(NoteEntity noteEntity, NoteViewStateEntity noteViewStateEntity) => new()
  {
    Id = NoteId.Create(noteEntity.Id),
    NavigationId = NavigationId.Create(noteEntity.Navigation ?? throw new InvalidOperationException()),
    Created = noteEntity.Created,
    Modified = noteEntity.Modified,
    Title = noteEntity.Title,
    Body = noteEntity.Body,
    BackgroundColor = noteEntity.BackgroundColor,
    BackgroundImagePath = noteEntity.BackgroundImagePath,
    IsBookmarked = noteEntity.IsBookmarked,
    IsDeleted = noteEntity.IsDeleted,
    ViewStateDto = ToDto(noteViewStateEntity)
  };

  public static NoteSearchDocument ToEntity(NoteSearchDocumentDto noteSearchDocumentDto) => new()
  {
    Id = noteSearchDocumentDto.Id,
    Title = noteSearchDocumentDto.Title,
    Body = noteSearchDocumentDto.Body
  };
}
