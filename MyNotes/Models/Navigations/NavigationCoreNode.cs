using CommunityToolkit.Mvvm.ComponentModel;

using MyNotes.Debugging;

namespace MyNotes.Models.Navigations;

internal abstract class NavigationCoreNode : ObservableObject, INavigationNode
{
  public NavigationCoreNode(Type pageType)
  {
#if DEBUG
    ReferenceTracker.NavigationReference.Add(this, $"{GetType().Name.Replace("Navigation", ""),15}: {GetHashCode()}");
#endif
    PageType = pageType;
  }

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
}