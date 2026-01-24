using CommunityToolkit.Mvvm.ComponentModel;

using MyNotes.Common.Collections;
using MyNotes.Models.Notes;
using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigations;

internal sealed class NavigationSearch : ObservableObject, INavigation, INavigationNoteList
{
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

  public NavigationSearch() { }

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
