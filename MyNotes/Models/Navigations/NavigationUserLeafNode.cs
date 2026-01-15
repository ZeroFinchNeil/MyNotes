using MyNotes.Common.Structures;
using MyNotes.Models.Notes;
using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigations;

internal class NavigationUserLeafNode : NavigationUserNode
{
  public NavigationUserLeafNode() : base(typeof(UserListPage)) { }

  public required NoteSortKey NoteSortKey
  {
    get;
    set => SetProperty(ref field, value);
  }

  public required SortDirection NoteSortDirection
  {
    get;
    set => SetProperty(ref field, value);
  }

  public required PreviewLayoutType PreviewLayoutType
  {
    get;
    set => SetProperty(ref field, value);
  }

  public required PreviewTileSize PreviewTileSize
  {
    get;
    set => SetProperty(ref field, value);
  }

  public required PreviewTileRatio PreviewTileRatio
  {
    get;
    set => SetProperty(ref field, value);
  }
}
