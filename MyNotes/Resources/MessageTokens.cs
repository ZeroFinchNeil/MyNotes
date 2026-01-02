using MyNotes.Common.Messages;
using MyNotes.Models.Notes;

namespace MyNotes.Resources;

internal static class MessageTokens
{
  // MainWindow -> MainPage
  public static readonly MessageToken MainWindowActivationChangedToken = new() { Key = "MainWindowActivationChanged" };

  // SettingsViewModel -> MainWindow
  public static readonly MessageToken AppThmeChangedToken = new() { Key = "ChangeAppTheme" };

  // NoteWindow -> NotePage
  public static MessageToken<NoteId> NoteWindowActivationChangedToken(NoteId id) => new() { Key = "NoteWindowActivationChanged", Context = id };

  // UserNavigationViewModel -> UserRootNavigationViewModel
  public static MessageToken GetAllGroupNavigationViewModelsToken = new() { Key = "GetAllGroupNavigationViewModels" };
}
