using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Contracts.Querying.Models;
using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigations.User;

internal sealed partial class NavigationUserLeafNode : NavigationUserNode, INavigationNoteListNode, INavigationInitialTarget
{
  public NavigationUserLeafNode() : base(typeof(UserListPage)) { }

  public required NoteSortKey NoteSortKey
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

  public required SortDirection NoteSortDirection
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

  public required PreviewLayoutType PreviewLayoutType
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

  public required PreviewTileSize PreviewTileSize
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

  public required PreviewTileRatio PreviewTileRatio
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
