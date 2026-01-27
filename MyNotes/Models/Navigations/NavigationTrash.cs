using MyNotes.Resources;
using MyNotes.Views.Navigations;

namespace MyNotes.Models.Navigations;

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