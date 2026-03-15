using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Commands;
using MyNotes.Common.Messages;
using MyNotes.Common.Structures;
using MyNotes.AppConstants;
using MyNotes.Helpers;
using MyNotes.Models.Navigations;
using MyNotes.Models.Settings;
using MyNotes.Services.Commands;
using MyNotes.Services.Settings;

namespace MyNotes.ViewModels.Navigations;

internal partial class UserCompositeNavigationViewModel : UserNavigationViewModel
{
  public override NavigationUserCompositeNode Navigation { get; }
  public ObservableCollection<NavigationViewModelBase> ChildNodeViewModels { get; }

  private readonly NavigationViewModelProvider NavigationViewModelProvider;
  private readonly NavigationCommandService NavigationCommandService;
  private readonly SettingsService SettingsService;

  #region Object Lifetime Management
  public UserCompositeNavigationViewModel(NavigationViewModelProvider provider, [FromKeyedServices(CommandServiceType.Navigation)] ICommandService commandService, SettingsService settingsService, NavigationUserCompositeNode navigation) : base(navigation)
  {
    Navigation = navigation;

    // Dependency Injection
    NavigationViewModelProvider = provider;
    NavigationCommandService = (NavigationCommandService)commandService;
    SettingsService = settingsService;

    ChildNodeViewModels = [.. Navigation.ChildNodes.Select(NavigationViewModelProvider.Resolve)];

    _ = SetIconImage();

    Navigation.PropertyChanged += Navigation_PropertyChanged;
    Navigation.ChildNodes.CollectionChanged += ChildNodes_CollectionChanged;

    // Register messenger
    RegisterMessenger();
  }

  protected override void Dispose(bool disposing)
  {
    if (Disposed)
      return;

    if (disposing)
    {
      Navigation.PropertyChanged -= Navigation_PropertyChanged;
      Navigation.ChildNodes.CollectionChanged -= ChildNodes_CollectionChanged;
      UnregisterMessenger();
    }

    base.Dispose(disposing);
  }
  #endregion

  private async void Navigation_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    switch (e.PropertyName)
    {
      case nameof(NavigationUserCompositeNode.Icon):
        await SetIconImage();
        break;
    }
  }

  private async Task SetIconImage()
  {
    IconImage = await IconHelper.GetIconImage((short)Navigation.Icon, (GroupIconBadge)SettingsService.Load(AppSettingsDescriptors.GroupIconBadge), Navigation is not NavigationUserRootNode);
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

    WeakReferenceMessenger.Default.Send(new ValueChangedMessage<bool>(true), AppMessageTokens.NavigationCollectionChangedToken);
  }

  public void ForEachDescendant(Action<NavigationViewModelBase> action, bool containsSelf = true)
  {
    Stack<NavigationViewModelBase> stack = new();
    stack.Push(this);

    while (stack.Count > 0)
    {
      var viewmodel = stack.Pop();
      if (containsSelf || viewmodel != this)
        action.Invoke(viewmodel);

      if (viewmodel is UserCompositeNavigationViewModel compositeViewModel)
      {
        foreach (var childViewModel in compositeViewModel.ChildNodeViewModels)
          stack.Push(childViewModel);
      }
    }
  }

  public NavigationViewModelBase? FirstDescendantOrDefault(Func<NavigationViewModelBase, bool> condition, bool containsSelf = true)
  {
    Stack<NavigationViewModelBase> stack = new();
    stack.Push(this);

    while (stack.Count > 0)
    {
      var viewmodel = stack.Pop();
      if (containsSelf || viewmodel != this)
        if (condition.Invoke(viewmodel))
          return viewmodel;

      if (viewmodel is UserCompositeNavigationViewModel compositeViewModel)
      {
        foreach (var childViewModel in compositeViewModel.ChildNodeViewModels)
          stack.Push(childViewModel);
      }
    }

    return null;
  }

  public override Command<NavigationUserNode> AddListCommand => NavigationCommandService.AddListCommand;
  public override Command<NavigationUserNode> AddGroupCommand => NavigationCommandService.AddGroupCommand;
  public override Command<NavigationUserNode> UpdateCommand => NavigationCommandService.UpdateCommand;
  public override Command<NavigationUserNode> DeleteCommand => NavigationCommandService.DeleteCommand;
  public override Command<SourceTargetPair<NavigationUserNode, NavigationUserCompositeNode>> MoveToGroupCommand => NavigationCommandService.MoveToGroupCommand;
  public override Command<NavigationUserNode> SetAsStartPageCommand => NavigationCommandService.SetAsStartPageCommand;

  private void RegisterMessenger()
  {
    WeakReferenceMessenger.Default.Register<ValueChangedMessage<GroupIconBadge>, MessageToken>(this, AppMessageTokens.ChangeNavigationViewModelIconImageToken, async (recipient, message) => await SetIconImage());
  }

  private void UnregisterMessenger()
  {
    WeakReferenceMessenger.Default.UnregisterAll(this);
  }
}
