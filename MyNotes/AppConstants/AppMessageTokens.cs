using MyNotes.Common.Messages;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;

namespace MyNotes.AppConstants;

internal static class AppMessageTokens
{
  // MainWindow -> MainPage
  public static readonly MessageToken MainWindowActivationChangedToken = new() { Key = "MainWindowActivationChanged" };

  // SettingsViewModel -> MainWindow
  public static readonly MessageToken ChangeAppThemeToken = new() { Key = "ChangeAppTheme" };

  // NoteWindow -> NotePage
  public static MessageToken<NoteId> NoteWindowActivationChangedToken(NoteId id) => new() { Key = "NoteWindowActivationChanged", Context = id };

  // UserNavigationViewModel -> UserRootNavigationViewModel
  public static MessageToken GetAllGroupNavigationViewModelsToken = new() { Key = "GetAllGroupNavigationViewModels" };

  // NoteViewModel, SettingsPage -> UserRootNavigationViewModel
  public static MessageToken GetAllListNavigationViewModelsToken = new() { Key = "GetAllListNavigationViewModels" };

  // SettingsViewModel -> UserNavigationViewModel(s)
  public static MessageToken ChangeNavigationViewModelIconImageToken = new() { Key = "ChangeNavigationViewModelIconImage" };

  // NoteViewModel -> NoteListViewModel
  public static MessageToken ChangeNoteIsBookmarkedStateToken = new() { Key = "ChangeNoteIsBookmarkedState" };

  // NoteEditorViewModel -> NoteViewModel
  public static MessageToken<NoteId> UpdateNotePreviewToken(NoteId id) => new() { Key = "UpdateNotePreview", Context = id };

  // NotePage -> NoteListViewModel
  public static MessageToken<INavigationNoteList> IsNoteInListToken(INavigationNoteList navigation) => new() { Key = "IsNoteInListToken", Context = navigation };

  // NotePage -> NoteListViewModel
  public static MessageToken<INavigationNoteList> RemoveNoteFromListToken(INavigationNoteList navigation) => new() { Key = "RemoveNoteFromListToken", Context = navigation };

  public static MessageToken NavigationCollectionChangedToken = new() { Key = "NavigationCollectionChanged" };
}
