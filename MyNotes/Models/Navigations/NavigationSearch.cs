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

  public NoteSortKey NoteSortKey
  {
    get;
    set
    {
      if (Enum.IsDefined(value))
      {
        SetProperty(ref field, value);
      }
    }
  }

  public SortDirection NoteSortDirection
  {
    get;
    set
    {
      if (Enum.IsDefined(value))
      {
        SetProperty(ref field, value);
      }
    }
  }

  public PreviewLayoutType PreviewLayoutType
  {
    get;
    set
    {
      if (Enum.IsDefined(value))
      {
        SetProperty(ref field, value);
      }
    }
  }

  public PreviewTileSize PreviewTileSize
  {
    get;
    set
    {
      if (Enum.IsDefined(value))
      {
        SetProperty(ref field, value);
      }
    }
  }

  public PreviewTileRatio PreviewTileRatio
  {
    get;
    set
    {
      if (Enum.IsDefined(value))
      {
        SetProperty(ref field, value);
      }
    }
  }
}
