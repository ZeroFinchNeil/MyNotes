using CommunityToolkit.Mvvm.ComponentModel;

using MyNotes.Common.Collections;
using MyNotes.Models.Notes;
using MyNotes.Resources;
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
  public partial NoteSortKey? NoteSortKey { get; set; }

  [ObservableProperty]
  public partial SortDirection? NoteSortDirection { get; set; }

  [ObservableProperty]
  public partial PreviewLayoutType? PreviewLayoutType { get; set; }

  [ObservableProperty]
  public partial PreviewTileSize? PreviewTileSize { get; set; }

  [ObservableProperty]
  public partial PreviewTileRatio? PreviewTileRatio { get; set; }
}