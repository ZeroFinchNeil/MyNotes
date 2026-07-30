using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Contracts.Querying.Models;

namespace MyNotes.Models.Navigations;

internal interface INavigationNoteList : INavigation
{
  public NoteSortKey NoteSortKey { get; set; }

  public SortDirection NoteSortDirection { get; set; }

  public PreviewLayoutType PreviewLayoutType { get; set; }

  public PreviewTileSize PreviewTileSize { get; set; }

  public PreviewTileRatio PreviewTileRatio { get; set; }
}
