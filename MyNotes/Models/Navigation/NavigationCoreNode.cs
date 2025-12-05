using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Controls.AnimatedVisuals;

using MyNotes.Resources;
using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigation;

internal abstract class NavigationCoreNode : ObservableObject, INavigationCoreNode
{
  public required NavigationId Id { get; init; }

  public required IconElement Icon
  {
    get;
    set => SetProperty(ref field, value);
  }

  public required string Title
  {
    get;
    set => SetProperty(ref field, value);
  }

  public required Type PageType
  {
    get;
    set => SetProperty(ref field, value);
  }
}

#region Core Nodes

internal sealed class NavigationHome : NavigationCoreNode
{
  public static NavigationHome Instance => field ??= new()
  {
    Id = NavigationId.Home,
    Icon = new IconSourceElement() { IconSource = new SymbolIconSource() { Symbol = Symbol.Home } },
    Title = LocalizedStrings.NavigationHomeTitle,
    PageType = typeof(HomePage)
  };

  private NavigationHome() { }
}

internal sealed class NavigationBookmarks : NavigationCoreNode
{
  public static NavigationBookmarks Instance => field ??= new()
  {
    Id = NavigationId.Bookmarks,
    Icon = new IconSourceElement() { IconSource = new SymbolIconSource() { Symbol = Symbol.Bookmarks } },
    Title = LocalizedStrings.NavigationBookmarksTitle,
    PageType = typeof(HomePage)
  };

  private NavigationBookmarks() { }
}

internal sealed class NavigationTrash : NavigationCoreNode
{
  public static NavigationTrash Instance => field ??= new()
  {
    Id = NavigationId.Empty,
    Icon = new IconSourceElement() { IconSource = new SymbolIconSource() { Symbol = Symbol.Delete } },
    Title = LocalizedStrings.NavigationTrashTitle,
    PageType = typeof(HomePage)
  };

  private NavigationTrash() { }
}

internal sealed class NavigationSettings : NavigationCoreNode
{
  public static NavigationSettings Instance => field ??= new()
  {
    Id = NavigationId.Empty,
    Icon = new AnimatedIcon() { Source = new AnimatedSettingsVisualSource() },
    Title = LocalizedStrings.NavigationSettingsTitle,
    PageType = typeof(SettingsPage)
  };

  private NavigationSettings() { }
}
#endregion
