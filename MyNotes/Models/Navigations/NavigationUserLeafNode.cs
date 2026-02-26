using MyNotes.Common.Collections;
using MyNotes.Models.Notes;
using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigations;

internal class NavigationUserLeafNode : NavigationUserNode, INavigationNoteList, INavigationInitialTarget
{
  public NavigationUserLeafNode() : base(typeof(UserListPage)) { }

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
