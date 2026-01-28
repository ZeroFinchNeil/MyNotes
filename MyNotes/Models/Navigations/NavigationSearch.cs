using CommunityToolkit.Mvvm.ComponentModel;

using MyNotes.Common.Collections;
using MyNotes.Debugging;
using MyNotes.Models.Notes;
using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigations;

internal sealed class NavigationSearch : ObservableObject, INavigation, INavigationNoteList
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

  public required string Title
  {
    get;
    set => SetProperty(ref field, value);
  }

  public required string SearchText
  {
    get;
    set => SetProperty(ref field, value);
  }

  public Type PageType { get; } = typeof(SearchResultsPage);

  public NoteSortKey? NoteSortKey
  {
    get;
    set => SetProperty(ref field, value);
  }

  public SortDirection? NoteSortDirection
  {
    get;
    set => SetProperty(ref field, value);
  }

  public PreviewLayoutType? PreviewLayoutType
  {
    get;
    set => SetProperty(ref field, value);
  }

  public PreviewTileSize? PreviewTileSize
  {
    get;
    set => SetProperty(ref field, value);
  }

  public PreviewTileRatio? PreviewTileRatio
  {
    get;
    set => SetProperty(ref field, value);
  }
}
