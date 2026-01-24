using MyNotes.Constants;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Services.Notes;
using MyNotes.Services.Search;
using MyNotes.Services.Settings;
using MyNotes.ViewModels.Notes;

namespace MyNotes.ViewModels.Navigations;

internal sealed class SearchNavigationViewModel : NavigationViewModelBase
{
  private readonly SearchService SearchService;
  private readonly NoteService NoteService;
  private readonly NoteViewModelProvider NoteViewModelProvider;
  private readonly SettingsService SettingsService;

  public override NavigationSearch Navigation { get; }

  public ObservableCollection<NoteViewModel> NoteViewModels { get; } = new();

  public SearchNavigationViewModel(SearchService searchService, NoteService noteService, NoteViewModelProvider noteViewModelProvider, SettingsService settingsService, NavigationSearch navigation)
  {
    SearchService = searchService;
    NoteService = noteService;
    NoteViewModelProvider = noteViewModelProvider;
    SettingsService = settingsService;
    Navigation = navigation;
  }
}
