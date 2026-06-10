using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.Mvvm.ComponentModel;

using MyNotes.Domain.ValueObjects;
using MyNotes.Templates;

namespace MyNotes.Models.Navigations;

[Debugging.ReferenceTracker]
internal abstract partial class NavigationUserNode : ObservableObject, INavigationNode
{
  protected NavigationUserNode(Type pageType)
  {
    TrackReference();
    PageType = pageType;
  }

  public required NavigationId Id { get; init; }
  public Type PageType { get; init; }

  [ObservableProperty]
  public required partial NavigationUserCompositeNode Parent { get; set; }

  [ObservableProperty]
  public required partial Icon Icon { get; set; }

  [ObservableProperty]
  public required partial string Title { get; set; }

  public bool TryGetPrevious([NotNullWhen(true)] out NavigationUserNode? previousNode)
  {
    int index = Parent.ChildNodes.IndexOf(this);
    previousNode = index > 0 ? Parent.ChildNodes[index - 1] : null;
    return previousNode is not null;
  }

  public bool TryGetNext([NotNullWhen(true)] out NavigationUserNode? nextNode)
  {
    int index = Parent.ChildNodes.IndexOf(this);
    nextNode = index >= 0 && index < Parent.ChildNodes.Count - 1 ? Parent.ChildNodes[index + 1] : null;
    return nextNode is not null;
  }

  public override string ToString() => $"{Id.Value} ({Title})";
}
