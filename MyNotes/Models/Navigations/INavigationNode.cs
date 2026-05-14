using MyNotes.Domain.ValueObjects;

namespace MyNotes.Models.Navigations;

internal interface INavigationNode : INavigation
{
  public NavigationId Id { get; }
  public string Title { get; set; }
  public Type PageType { get; }
}
