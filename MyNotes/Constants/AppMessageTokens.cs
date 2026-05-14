using MyNotes.Common.Messages;
using MyNotes.Domain.ValueObjects;
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

  // NoteViewModel -> NoteListViewModel
  public static MessageToken ChangeNoteIsBookmarkedStateToken { get; } = new() { Key = "ChangeNoteIsBookmarkedState" };

  // NoteEditorViewModel -> NoteViewModel
  public static MessageToken<NoteId> UpdateNotePreviewToken(NoteId id) => new() { Key = "UpdateNotePreview", Context = id };

  // NotePage -> NoteListViewModel
  public static MessageToken<INavigationNoteList> IsNoteInListToken(INavigationNoteList navigation) => new() { Key = "IsNoteInListToken", Context = navigation };

  // NotePage -> NoteListViewModel
  public static MessageToken<INavigationNoteList> RemoveNoteFromListToken(INavigationNoteList navigation) => new() { Key = "RemoveNoteFromListToken", Context = navigation };

  public static MessageToken NavigationCollectionChangedToken { get; } = new() { Key = "NavigationCollectionChanged" };
}
