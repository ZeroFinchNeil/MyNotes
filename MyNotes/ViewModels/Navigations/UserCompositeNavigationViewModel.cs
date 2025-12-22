using MyNotes.Common.Commands;
using MyNotes.Models.Navigations;
using MyNotes.Services.Navigation;

namespace MyNotes.ViewModels.Navigations;

internal sealed partial class UserCompositeNavigationViewModel : NavigationViewModelBase
{
  public override NavigationUserCompositeNode Navigation { get; }
  public ObservableCollection<NavigationViewModelBase> ChildNodeViewModels { get; }

  private readonly NavigationViewModelFactory NavigationViewModelFactory;
  private readonly NavigationService NavigationService;

  public UserCompositeNavigationViewModel(NavigationViewModelFactory factory, NavigationService navigationService, NavigationUserCompositeNode navigation)
  {
    Navigation = navigation;

    // Dependency Injection
    NavigationViewModelFactory = factory;
    NavigationService = navigationService;

    ChildNodeViewModels = [.. Navigation.ChildNodes.Select(NavigationViewModelFactory.Resolve)];
    Navigation.ChildNodes.CollectionChanged += ChildNodes_CollectionChanged;
  }

  private void ChildNodes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
  {
    switch (e.Action)
    {
      case NotifyCollectionChangedAction.Add:
        if (e.NewItems is IList { Count: > 0 } addedItems && addedItems[0] is INavigation addedItem)
        {
          ChildNodeViewModels.Insert(e.NewStartingIndex, NavigationViewModelFactory.Resolve(addedItem));
        }
        break;
      case NotifyCollectionChangedAction.Remove:
        if (e.OldItems is IList { Count: > 0 } removedItems)
        {
          ChildNodeViewModels.RemoveAt(e.OldStartingIndex);
        }
        break;
      case NotifyCollectionChangedAction.Replace:
        if (e.NewItems is IList { Count: > 0 } replacedItems && replacedItems[0] is INavigation replacedItem
          && e.NewStartingIndex < ChildNodeViewModels.Count)
        {
          ChildNodeViewModels[e.NewStartingIndex] = NavigationViewModelFactory.Resolve(replacedItem);
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

  public Command<NavigationViewModelBase>? ShowAddListDialogCommand => NavigationService.ShowAddListDialogCommand;
  public Command<NavigationViewModelBase>? ShowAddGroupDialogCommand => NavigationService.ShowAddGroupDialogCommand;
  public Command<NavigationViewModelBase>? ShowUpdateDialogCommand => NavigationService.ShowUpdateDialogCommand;
  public Command<NavigationViewModelBase>? ShowConfirmDeleteDialogCommand => NavigationService.ShowConfirmDeleteDialogCommand;
}
