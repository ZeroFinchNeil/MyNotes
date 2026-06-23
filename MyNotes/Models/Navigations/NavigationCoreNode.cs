using CommunityToolkit.Mvvm.ComponentModel;

using MyNotes.Domain.ValueObjects;

namespace MyNotes.Models.Navigations;

[Debugging.Attributes.ReferenceTracker]
internal abstract partial class NavigationCoreNode : ObservableObject, INavigationNode
{
  protected NavigationCoreNode(Type pageType)
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