using MyNotes.Common.Collections;
using MyNotes.Models.Notes;
using MyNotes.Resources;
using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigations;

internal sealed class NavigationTrash : NavigationCoreNode, INavigationNoteList
{
  public static NavigationTrash Instance => field ??= new()
  {
    Id = NavigationId.Empty,
    Icon = new IconSourceElement() { IconSource = new SymbolIconSource() { Symbol = Symbol.Delete } },
    Title = LocalizedStrings.NavigationTrashTitle
  };

  private NavigationTrash() : base(typeof(TrashPage)) { }

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