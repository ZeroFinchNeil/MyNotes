using MyNotes.Application.Contracts.Database.Dtos.Notes.Creation;
using MyNotes.Application.Contracts.Search.Dtos.Notes;
using MyNotes.Application.Dtos.Notes.Creation;
using MyNotes.Common.Helpers;
using MyNotes.Domain.Entities.Notes;
using MyNotes.Domain.ValueObjects;
using MyNotes.Services.Settings;
using MyNotes.Shared.Constants;

using Windows.Graphics;

namespace MyNotes.Application.Services.Notes;

internal sealed partial class NoteFactory
{
  private readonly SettingsService SettingsService;

  public NoteFactory(SettingsService settingsService)
  {
    SettingsService = settingsService;
  }

  public Note CreateDefaultNote(NoteId noteId, NavigationId navigationId) => new()
  {
    Id = noteId,
    ParentId = navigationId,
    Created = DateTimeOffset.UtcNow,
    Modified = DateTimeOffset.UtcNow,
    Title = AppDefaultSettings.NoteTitle,
    Body = AppDefaultSettings.NoteBodyRtfText,
    BodyPlainText = AppDefaultSettings.NoteBodyPlainText,
    BackgroundColor = SettingsService.Load(AppSettingsDescriptors.NoteBackground),
    IsBookmarked = AppDefaultSettings.IsNoteBookmarked,
    IsDeleted = AppDefaultSettings.IsNoteDeleted
  };

  public CreateNoteViewStateDbRequestDto CreateDefaultNoteViewStateDto(NoteId id, SizeInt32 size, PointInt32 position)
  {
    return new()
    {
      Id = id,
      ShowBackgroundImage = AppDefaultSettings.ShowNoteBackgroundImage,
      BackgroundImagePath = AppDefaultSettings.NoteBackgroundImagePath,
      BackgroundImageOpacity = AppDefaultSettings.NoteBackgroundImageOpacity,
      BackgroundImageBlur = AppDefaultSettings.NoteBackgroundImageBlur,
      BackdropKind = SettingsService.Load(AppSettingsDescriptors.NoteBackdropKind),
      BackdropTintOpacity = AppDefaultSettings.NoteBackdropTintOpacity,
      BackdropLuminosityOpacity = AppDefaultSettings.NoteBackdropLuminosityOpacity,
      Images = [],
      ShowImagePanel = AppDefaultSettings.ShowNoteImagePanel,
      ImagePanelHeight = AppDefaultSettings.NoteImagePanelHeight,
      Width = size.Width,
      Height = size.Height,
      PositionX = position.X,
      PositionY = position.Y,
      IsWindowOpen = AppDefaultSettings.IsNoteWindowOpen,
      IsAlwaysOnTop = AppDefaultSettings.IsNoteWindowAlwaysOnTop,
    };
  }

  public NoteSearchDocumentDto CreateDefaultNoteSearchDocumentDto(NoteId noteId) => new()
  {
    Id = noteId.Value,
    Title = AppDefaultSettings.NoteTitle,
    Body = AppDefaultSettings.NoteBodyPlainText
  };
}
