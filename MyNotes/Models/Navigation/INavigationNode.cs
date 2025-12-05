namespace MyNotes.Models.Navigation;

internal interface INavigationNode : INavigation
{
  public NavigationId Id { get; }
  public string Title { get; set; }
  public Type PageType { get; set; }
}
