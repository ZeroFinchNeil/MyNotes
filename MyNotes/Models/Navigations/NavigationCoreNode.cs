using CommunityToolkit.Mvvm.ComponentModel;

namespace MyNotes.Models.Navigations;

[Debugging.ReferenceTracker]
internal abstract partial class NavigationCoreNode : ObservableObject, INavigationNode
{
  public NavigationCoreNode(Type pageType)
  {
    TrackReference();
    PageType = pageType;
  }

  public required NavigationId Id { get; init; }

  [ObservableProperty]
  public required partial IconElement Icon { get; set; }

  [ObservableProperty]
  public required partial string Title { get; set; }

  public Type PageType { get; init; }
}