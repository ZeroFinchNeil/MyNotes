using CommunityToolkit.Mvvm.ComponentModel;

using MyNotes.Debugging;

namespace MyNotes.Models.Navigations;

internal abstract partial class NavigationCoreNode : ObservableObject, INavigationNode
{
  public NavigationCoreNode(Type pageType)
  {
#if DEBUG
    if (Debugger.IsAttached)
    {
      ReferenceTracker.NavigationReference.Add(this, $"{GetType().Name.Replace("Navigation", ""),15}: {GetHashCode()}");
    }
#endif
    PageType = pageType;
  }

  public required NavigationId Id { get; init; }

  [ObservableProperty]
  public required partial IconElement Icon { get; set; }

  [ObservableProperty]
  public required partial string Title { get; set; }

  public Type PageType { get; init; }
}