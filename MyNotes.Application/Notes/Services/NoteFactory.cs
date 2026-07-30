using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Contracts.Settings;
using MyNotes.Application.Settings;
using MyNotes.Application.Settings.Services;
using MyNotes.Domain.Navigations;
using MyNotes.Domain.Notes;

using Windows.Graphics;

namespace MyNotes.Application.Notes.Services;

internal sealed partial class NoteFactory
{
  private readonly AppSettingsService SettingsService;

  public NoteFactory(AppSettingsService settingsService)
  {
    SettingsService = settingsService;
  }

  public Note CreateDefaultNote(NoteId noteId, NavigationId navigationId) => new()
  {
    Id = noteId,
    NavigationId = navigationId,
    Created = DateTimeOffset.UtcNow,
    Modified = DateTimeOffset.UtcNow,
    Title = NoteSettingsDescriptors.NoteTitle,
    Body = NoteSettingsDescriptors.NoteBodyRtfText,
    BackgroundColor = SettingsService.Load(NoteSettingsDescriptors.NoteBackground),
    BackgroundImagePath = NoteSettingsDescriptors.NoteBackgroundImagePath,
    IsBookmarked = NoteSettingsDescriptors.IsNoteBookmarked,
    IsDeleted = NoteSettingsDescriptors.IsNoteDeleted
  };

  public NoteViewStateDto CreateDefaultNoteViewStateDto(NoteId id, SizeInt32 size, PointInt32 position)
  {
    return new()
    {
      Id = id,
      ShowBackgroundImage = NoteSettingsDescriptors.ShowNoteBackgroundImage,
      BackgroundImageStretch = NoteSettingsDescriptors.NoteBackgroundImageStretch,
      BackgroundImageAlignment = NoteSettingsDescriptors.NoteBackgroundImageAlignment,
      BackgroundImageOpacity = NoteSettingsDescriptors.NoteBackgroundImageOpacity,
      BackgroundImageBlur = NoteSettingsDescriptors.NoteBackgroundImageBlur,
      BackdropKind = SettingsService.Load<BackdropKind, int>(BackdropKindSettingsCodec.Decode, NoteSettingsDescriptors.NoteBackdropKind),
      BackdropTintOpacity = NoteSettingsDescriptors.NoteBackdropTintOpacity,
      BackdropLuminosityOpacity = NoteSettingsDescriptors.NoteBackdropLuminosityOpacity,
      ShowImagePanel = NoteSettingsDescriptors.ShowNoteImagePanel,
      ImagePanelHeight = NoteSettingsDescriptors.NoteImagePanelHeight,
      Width = size.Width,
      Height = size.Height,
      PositionX = position.X,
      PositionY = position.Y,
      IsTextEditorReadOnly = NoteSettingsDescriptors.IsNoteTextEditorReadOnly,
      IsWindowOpen = NoteSettingsDescriptors.IsNoteWindowOpen,
      IsAlwaysOnTop = NoteSettingsDescriptors.IsNoteWindowAlwaysOnTop,
    };
  }

  public NoteSearchDocumentDto CreateDefaultNoteSearchDocumentDto(NoteId noteId) => new()
  {
    Id = noteId.Value,
    Title = NoteSettingsDescriptors.NoteTitle,
    Body = NoteSettingsDescriptors.NoteBodyPlainText
  };
}