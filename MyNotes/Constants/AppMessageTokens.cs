using MyNotes.Common.Messages;
using MyNotes.Domain.Notes;
using MyNotes.Models.Navigations;

namespace MyNotes.Constants;

internal static class AppMessageTokens
{
  // MainWindow -> MainPage
  public static MessageToken MainWindowActivationChangedToken { get; } = new() { Key = "MainWindowActivationChanged" };

  // SettingsViewModel -> MainWindow
  public static MessageToken ChangeAppThemeToken { get; } = new() { Key = "ChangeAppTheme" };

  // NoteWindow -> NotePage
  public static MessageToken<NoteId> NoteWindowActivationChangedToken(NoteId id) => new() { Key = "NoteWindowActivationChanged", Context = id };

  // SettingsViewModel -> UserNavigationViewModel(s)
  public static MessageToken ChangeNavigationViewModelIconImageToken { get; } = new() { Key = "ChangeNavigationViewModelIconImage" };

  // NoteViewModel -> NotePreviewListViewModel
  public static MessageToken ChangeNoteIsBookmarkedStateToken { get; } = new() { Key = "ChangeNoteIsBookmarkedState" };
  public static MessageToken NoteTitleChangedToken { get; } = new() { Key = "NoteTitleChangedToken" };

  // NoteEditorViewModel -> NoteViewModel
  public static MessageToken<NoteId> UpdateNotePreviewToken(NoteId id) => new() { Key = "UpdateNotePreview", Context = id };

  // NotePage -> NoteListViewModel
  public static MessageToken<INavigationNoteList> IsNoteInListToken(INavigationNoteList navigation) => new() { Key = "IsNoteInListToken", Context = navigation };

  // NotePage -> NoteListViewModel
  public static MessageToken<INavigationNoteList> AddNoteToListToken(INavigationNoteList navigation) => new() { Key = "AddNoteToListToken", Context = navigation };

  // UserGroupNavigationViewModel -> SettingsViewModel
  public static MessageToken NavigationCollectionChangedToken { get; } = new() { Key = "NavigationCollectionChanged" };
}
