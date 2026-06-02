using MyNotes.Application.Contracts.Database.Dtos.Notes;
using MyNotes.Application.Contracts.Search.Dtos.Notes;
using MyNotes.Application.Services.App;
using MyNotes.Shared.Constants;
using MyNotes.Common.Helpers;
using MyNotes.Domain.Entities.Notes;
using MyNotes.Domain.ValueObjects;
using MyNotes.Services.Settings;

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
    Title = string.Empty,
    Body = string.Empty,
    BodyPlainText = string.Empty,
    BackgroundColor = SettingsService.Load(AppSettingsDescriptors.NoteBackground),
    IsBookmarked = false,
    IsDeleted = false
  };

  public CreateNoteViewStateDbRequestDto CreateDefaultNoteViewStateDto(NoteId noteId)
  {
    throw new NotImplementedException();
#if false
    var defaultSize = SettingsService.Load(AppSettingsDescriptors.NoteSize);
    var defaultPosition = WindowService.GetPosition(defaultSize.SizeInt32);

    return new()
    {
      Id = noteId,
      ShowBackgroundImage = false,
      BackgroundImagePath = string.Empty,
      BackgroundImageOpacity = 1.0,
      BackgroundImageBlur = 100,
      BackdropKind = SettingsService.Load(AppSettingsDescriptors.NoteBackdropKind),
      BackdropTintOpacity = 1.0,
      BackdropLuminosityOpacity = 1.0,
      Images = [],
      ShowImagePanel = false,
      ImagePanelHeight = 120.0,
      Width = defaultSize.SizeInt32.Width,
      Height = defaultSize.SizeInt32.Height,
      PositionX = defaultPosition.X,
      PositionY = defaultPosition.Y,
      IsWindowOpen = true,
      IsAlwaysOnTop = false,
    };
#endif
  }

  public NoteSearchDocumentDto CreateDefaultNoteSearchDocumentDto(NoteId noteId) => new()
  {
    Id = noteId.Value,
    Title = string.Empty,
    Body = string.Empty
  };
}
