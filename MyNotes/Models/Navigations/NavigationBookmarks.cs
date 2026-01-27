using MyNotes.Common.Collections;
using MyNotes.Models.Notes;
using MyNotes.Resources;
using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigations;

internal sealed class NavigationBookmarks : NavigationCoreNode, INavigationNoteList
{
  public static NavigationBookmarks Instance => field ??= new()
  {
    Id = NavigationId.Bookmarks,
    Icon = new IconSourceElement() { IconSource = new SymbolIconSource() { Symbol = Symbol.Favorite } },
    Title = LocalizedStrings.NavigationBookmarksTitle,
  };

  private NavigationBookmarks() : base(typeof(BookmarksPage)) { }

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
