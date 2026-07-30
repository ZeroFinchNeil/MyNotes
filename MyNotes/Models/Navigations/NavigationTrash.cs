using CommunityToolkit.Mvvm.ComponentModel;

using MyNotes.Application.Contracts.Navigations.Models;
using MyNotes.Application.Contracts.Notes.Models;
using MyNotes.Application.Contracts.Querying.Models;
using MyNotes.Domain.Navigations;
using MyNotes.Strings;
using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigations;

internal sealed partial class NavigationTrash : NavigationCoreNode, INavigationNoteList
{
  public static NavigationTrash Instance => field ??= new()
  {
    Id = NavigationId.Trash,
    Icon = new IconSourceElement() { IconSource = new SymbolIconSource() { Symbol = Symbol.Delete } },
    Title = LocalizedStrings.NavigationTrashTitle
  };

  private NavigationTrash() : base(typeof(TrashPage)) { }

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