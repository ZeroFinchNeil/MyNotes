using MyNotes.Application.Contracts.Models.Notes;
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
    NavigationId = navigationId,
    Created = DateTimeOffset.UtcNow,
    Modified = DateTimeOffset.UtcNow,
    Title = AppDefaultSettings.NoteTitle,
    Body = AppDefaultSettings.NoteBodyRtfText,
    BackgroundColor = SettingsService.Load(AppSettingsDescriptors.NoteBackground),
    BackgroundImagePath = AppDefaultSettings.NoteBackgroundImagePath,
    IsBookmarked = AppDefaultSettings.IsNoteBookmarked,
    IsDeleted = AppDefaultSettings.IsNoteDeleted
  };

  public NoteViewStateDto CreateDefaultNoteViewStateDto(NoteId id, SizeInt32 size, PointInt32 position)
  {
    return new()
    {
      Id = id,
      ShowBackgroundImage = AppDefaultSettings.ShowNoteBackgroundImage,
      BackgroundImageStretch = (int)AppDefaultSettings.NoteBackgroundImageStretch,
      BackgroundImageAlignment = (int)AppDefaultSettings.NoteBackgroundImageAlignment,
      BackgroundImageOpacity = AppDefaultSettings.NoteBackgroundImageOpacity,
      BackgroundImageBlur = AppDefaultSettings.NoteBackgroundImageBlur,
      BackdropKind = SettingsService.Load(AppSettingsDescriptors.NoteBackdropKind),
      BackdropTintOpacity = AppDefaultSettings.NoteBackdropTintOpacity,
      BackdropLuminosityOpacity = AppDefaultSettings.NoteBackdropLuminosityOpacity,
      ShowImagePanel = AppDefaultSettings.ShowNoteImagePanel,
      ImagePanelHeight = AppDefaultSettings.NoteImagePanelHeight,
      Width = size.Width,
      Height = size.Height,
      PositionX = position.X,
      PositionY = position.Y,
      IsTextEditorReadOnly = AppDefaultSettings.IsNoteTextEditorReadOnly,
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
