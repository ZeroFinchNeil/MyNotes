using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MyNotes.Common.Collections;
using MyNotes.Debugging;
using MyNotes.Models.Notes;
using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigations;

internal sealed partial class NavigationSearch : ObservableObject, INavigation, INavigationNoteList
{
  public NavigationSearch()
  {
#if DEBUG
    if (Debugger.IsAttached)
    {
      ReferenceTracker.NavigationReference.Add(this, $"{GetType().Name.Replace("Navigation", ""),15}: {GetHashCode()}");
    }
#endif
  }

  [ObservableProperty]
  public required partial string Title { get; set; }

  [ObservableProperty]
  public required partial string SearchText { get; set; }

  public Type PageType { get; } = typeof(SearchResultsPage);

  [ObservableProperty]
  public partial NoteSortKey? NoteSortKey { get; set; }

  [ObservableProperty]
  public partial SortDirection? NoteSortDirection { get; set; }

  [ObservableProperty]
  public partial PreviewLayoutType? PreviewLayoutType { get; set; }

  [ObservableProperty]
  public partial PreviewTileSize? PreviewTileSize { get; set; }

  [ObservableProperty]
  public partial PreviewTileRatio? PreviewTileRatio { get; set; }
}
