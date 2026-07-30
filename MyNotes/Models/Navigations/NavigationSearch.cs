using CommunityToolkit.Mvvm.ComponentModel;

using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Contracts.Querying.Models;
using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigations;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class NavigationSearch : ObservableObject, INavigation, INavigationNoteList
{
  public NavigationSearch()
  {
    TrackReference();
  }

  [ObservableProperty]
  public required partial string Title { get; set; }

  [ObservableProperty]
  public required partial string SearchText { get; set; }

  public Type PageType { get; } = typeof(SearchResultsPage);

  [ObservableProperty]
  public partial NoteSortKey NoteSortKey { get; set; }

  [ObservableProperty]
  public partial SortDirection NoteSortDirection { get; set; }

  [ObservableProperty]
  public partial PreviewLayoutType PreviewLayoutType { get; set; }

  [ObservableProperty]
  public partial PreviewTileSize PreviewTileSize { get; set; }

  [ObservableProperty]
  public partial PreviewTileRatio PreviewTileRatio { get; set; }
}
