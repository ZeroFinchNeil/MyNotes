using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.UI.Xaml.Controls.AnimatedVisuals;

using MyNotes.Resources;
using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigations;

internal abstract class NavigationCoreNode : ObservableObject, INavigationNode
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

  public Type PageType { get; init; }

  public NavigationCoreNode(Type pageType) => PageType = pageType;
}

#region Core Nodes

internal sealed class NavigationHome : NavigationCoreNode
{
  public static NavigationHome Instance => field ??= new()
  {
    Id = NavigationId.Home,
    Icon = new IconSourceElement() { IconSource = new SymbolIconSource() { Symbol = Symbol.Home } },
    Title = LocalizedStrings.NavigationHomeTitle
  };

  private NavigationHome() : base(typeof(HomePage)) { }
}

internal sealed class NavigationBookmarks : NavigationCoreNode
{
  public static NavigationBookmarks Instance => field ??= new()
  {
    Id = NavigationId.Bookmarks,
    Icon = new IconSourceElement() { IconSource = new SymbolIconSource() { Symbol = Symbol.Bookmarks } },
    Title = LocalizedStrings.NavigationBookmarksTitle,
  };

  private NavigationBookmarks() : base(typeof(HomePage)) { }
}

internal sealed class NavigationTrash : NavigationCoreNode
{
  public static NavigationTrash Instance => field ??= new()
  {
    Id = NavigationId.Empty,
    Icon = new IconSourceElement() { IconSource = new SymbolIconSource() { Symbol = Symbol.Delete } },
    Title = LocalizedStrings.NavigationTrashTitle
  };

  private NavigationTrash() : base(typeof(HomePage)) { }
}

internal sealed class NavigationSettings : NavigationCoreNode
{
  public static NavigationSettings Instance => field ??= new()
  {
    Id = NavigationId.Empty,
    Icon = new AnimatedIcon() { Source = new AnimatedSettingsVisualSource() },
    Title = LocalizedStrings.NavigationSettingsTitle
  };

  private NavigationSettings() : base(typeof(SettingsPage)) { }
}
#endregion
