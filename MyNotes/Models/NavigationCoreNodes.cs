using Microsoft.UI.Xaml.Controls.AnimatedVisuals;

using MyNotes.Resources;
using MyNotes.Views.Navigations;

namespace MyNotes.Models;

public sealed class NavigationHome : NavigationCoreNode
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

public sealed class NavigationBookmarks : NavigationCoreNode
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

public sealed class NavigationTrash : NavigationCoreNode
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

public sealed class NavigationSettings : NavigationCoreNode
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