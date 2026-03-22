using CommunityToolkit.Mvvm.ComponentModel;

using MyNotes.Common.Collections;
using MyNotes.Models.Notes;
using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigations;

internal partial class NavigationUserLeafNode : NavigationUserNode, INavigationNoteList, INavigationInitialTarget
{
  public NavigationUserLeafNode() : base(typeof(UserListPage)) { }

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
