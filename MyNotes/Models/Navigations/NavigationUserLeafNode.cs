using MyNotes.Common.Querying;
using MyNotes.Shared.Enums.Navigations;
using MyNotes.Shared.Enums.Notes;
using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigations;

internal partial class NavigationUserLeafNode : NavigationUserNode, INavigationNoteList, INavigationInitialTarget
{
  public NavigationUserLeafNode() : base(typeof(UserListPage)) { }

  public NoteSortKey? NoteSortKey
  {
    get;
    set
    {
      if (value is null || Enum.IsDefined(value.Value))
      {
        SetProperty(ref field, value);
      }
    }
  }

  public SortDirection? NoteSortDirection
  {
    get;
    set
    {
      if (value is null || Enum.IsDefined(value.Value))
      {
        SetProperty(ref field, value);
      }
    }
  }

  public PreviewLayoutType? PreviewLayoutType
  {
    get;
    set
    {
      if (value is null || Enum.IsDefined(value.Value))
      {
        SetProperty(ref field, value);
      }
    }
  }

  public PreviewTileSize? PreviewTileSize
  {
    get;
    set
    {
      if (value is null || Enum.IsDefined(value.Value))
      {
        SetProperty(ref field, value);
      }
    }
  }

  public PreviewTileRatio? PreviewTileRatio
  {
    get;
    set
    {
      if (value is null || Enum.IsDefined(value.Value))
      {
        SetProperty(ref field, value);
      }
    }
  }
}
