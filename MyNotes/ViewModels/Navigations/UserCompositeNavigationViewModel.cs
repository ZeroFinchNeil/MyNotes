using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Commands;
using MyNotes.Models.Navigations;
using MyNotes.Services.Commands;

namespace MyNotes.ViewModels.Navigations;

internal sealed partial class UserCompositeNavigationViewModel : UserNavigationViewModel
{
  public override NavigationUserCompositeNode Navigation { get; }
  public ObservableCollection<NavigationViewModelBase> ChildNodeViewModels { get; }

  private readonly NavigationViewModelProvider NavigationViewModelProvider;
  private readonly NavigationCommandService NavigationCommandService;

  public UserCompositeNavigationViewModel(NavigationViewModelProvider provider, [FromKeyedServices(CommandServiceType.Navigation)] ICommandService navigationCommandService, NavigationUserCompositeNode navigation)
  {
    Navigation = navigation;

    // Dependency Injection
    NavigationViewModelProvider = provider;
    NavigationCommandService = (NavigationCommandService)navigationCommandService;

    ChildNodeViewModels = [.. Navigation.ChildNodes.Select(NavigationViewModelProvider.Resolve)];
    Navigation.ChildNodes.CollectionChanged += ChildNodes_CollectionChanged;
  }

  private void ChildNodes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
  {
    switch (e.Action)
    {
      case NotifyCollectionChangedAction.Add:
        if (e.NewItems is IList { Count: > 0 } addedItems && addedItems[0] is INavigation addedItem)
        {
          ChildNodeViewModels.Insert(e.NewStartingIndex, NavigationViewModelProvider.Resolve(addedItem));
        }
        break;
      case NotifyCollectionChangedAction.Remove:
        if (e.OldItems is IList { Count: > 0 })
        {
          ChildNodeViewModels.RemoveAt(e.OldStartingIndex);
        }
        break;
      case NotifyCollectionChangedAction.Replace:
        if (e.NewItems is IList { Count: > 0 } replacedItems && replacedItems[0] is INavigation replacedItem
          && e.NewStartingIndex < ChildNodeViewModels.Count)
        {
          ChildNodeViewModels[e.NewStartingIndex] = NavigationViewModelProvider.Resolve(replacedItem);
        }
        break;
      case NotifyCollectionChangedAction.Move:
        if (e.NewItems is IList { Count: > 0 } && e.OldItems is IList { Count: > 0 } && e.NewStartingIndex < ChildNodeViewModels.Count && e.OldStartingIndex < ChildNodeViewModels.Count)
        {
          ChildNodeViewModels.Move(e.OldStartingIndex, e.NewStartingIndex);
        }
        break;
      case NotifyCollectionChangedAction.Reset:
        ChildNodeViewModels.Clear();
        break;
    }
  }

  protected override void Dispose(bool disposing)
  {
    if (_disposed)
      return;

    if (disposing)
    {
      Navigation.ChildNodes.CollectionChanged -= ChildNodes_CollectionChanged;
    }

    _disposed = true;
  }

  public void ForEachDescendantAndSelf(Action<NavigationViewModelBase> action)
  {
    Stack<NavigationViewModelBase> stack = new();
    stack.Push(this);

    while (stack.Count > 0)
    {
      var viewmodel = stack.Pop();
      action.Invoke(viewmodel);

      if (viewmodel is UserCompositeNavigationViewModel compositeViewModel)
      {
        foreach (var childViewModel in compositeViewModel.ChildNodeViewModels)
          stack.Push(childViewModel);
      }
    }
  }

  public override Command<NavigationViewModelBase>? AddListCommand => NavigationCommandService.AddListCommand;
  public override Command<NavigationViewModelBase>? AddGroupCommand => NavigationCommandService.AddGroupCommand;
  public override Command<NavigationViewModelBase>? UpdateCommand => NavigationCommandService.UpdateCommand;
  public override Command<NavigationViewModelBase>? DeleteCommand => NavigationCommandService.DeleteCommand;
  public override Command<(NavigationViewModelBase SourceItemViewModel, NavigationViewModelBase TargetGroupViewModel)>? MoveToGroupCommand => NavigationCommandService.MoveToGroupCommand;
}
