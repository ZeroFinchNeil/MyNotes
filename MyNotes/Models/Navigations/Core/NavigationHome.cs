using MyNotes.Domain.Navigations;
using MyNotes.Strings;
using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigations.Core;

internal sealed partial class NavigationHome : NavigationCoreNode, INavigationInitialTarget
{
  public static NavigationHome Instance => field ??= new()
  {
    Id = NavigationId.Home,
    Icon = new IconSourceElement() { IconSource = new SymbolIconSource() { Symbol = Symbol.Home } },
    Title = LocalizedStrings.NavigationHomeTitle
  };

  private NavigationHome() : base(typeof(HomePage)) { }
}