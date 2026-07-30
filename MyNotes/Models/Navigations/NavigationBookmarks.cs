using CommunityToolkit.Mvvm.ComponentModel;

using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Contracts.Querying.Models;
using MyNotes.Domain.Navigations;
using MyNotes.Strings;
using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigations;

internal sealed partial class NavigationBookmarks : NavigationCoreNode, INavigationNoteList, INavigationInitialTarget
{
  public static NavigationBookmarks Instance => field ??= new()
  {
    Id = NavigationId.Bookmarks,
    Icon = new IconSourceElement() { IconSource = new SymbolIconSource() { Symbol = Symbol.Favorite } },
    Title = LocalizedStrings.NavigationBookmarksTitle,
  };

  private NavigationBookmarks() : base(typeof(BookmarksPage)) { }

  [ObservableProperty]
  public partial NoteSortKey NoteSortKey { get; set; }

  [ObservableProperty]
  public partial SortDirection NoteSortDirection { get; set; }

  [ObservableProperty]
  public partial PreviewLayoutType PreviewLayoutType { get; set; }

  [ObservableProperty]
  public partial PreviewTileSize PreviewTileSize { get; set; }

  [ObservableProperty]
  public partial PreviewTileRatio PreviewTileRatio { get; set; }
}
