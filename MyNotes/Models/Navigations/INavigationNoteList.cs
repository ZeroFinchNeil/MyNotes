using MyNotes.Common.Collections;
using MyNotes.Models.Notes;

namespace MyNotes.Models.Navigations;

internal interface INavigationNoteList : INavigation
{
  public NoteSortKey? NoteSortKey { get; set; }

  public SortDirection? NoteSortDirection { get; set; }

  public PreviewLayoutType? PreviewLayoutType { get; set; }

  public PreviewTileSize? PreviewTileSize { get; set; }

  public PreviewTileRatio? PreviewTileRatio { get; set; }
}
