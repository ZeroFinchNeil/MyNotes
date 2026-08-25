using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Contracts.Querying.Models;
using MyNotes.Domain.Navigations;
using MyNotes.Strings;
using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigations.Core;

internal sealed partial class NavigationBookmarks : NavigationCoreNode, INavigationNoteListNode, INavigationInitialTarget
{
  public static NavigationBookmarks Instance => field ??= new()
  {
    Id = NavigationId.Bookmarks,
    Icon = new IconSourceElement() { IconSource = new SymbolIconSource() { Symbol = Symbol.Favorite } },
    Title = LocalizedStrings.NavigationBookmarksTitle,
  };

  private NavigationBookmarks() : base(typeof(BookmarksPage)) { }

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
