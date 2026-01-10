using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Commands;
using MyNotes.Common.Messages;
using MyNotes.Helpers;
using MyNotes.Models.Navigations;
using MyNotes.Models.Settings;
using MyNotes.Resources;
using MyNotes.Services.Commands;
using MyNotes.Services.Settings;

namespace MyNotes.ViewModels.Navigations;

internal partial class UserCompositeNavigationViewModel : UserNavigationViewModel
{
  public override NavigationUserCompositeNode Navigation { get; }
  public ObservableCollection<NavigationViewModelBase> ChildNodeViewModels { get; }

  private readonly NavigationViewModelProvider NavigationViewModelProvider;
  private readonly NavigationViewModelCommandService NavigationViewModelCommandService;
  private readonly SettingsService SettingsService;

  public UserCompositeNavigationViewModel(NavigationViewModelProvider provider, [FromKeyedServices(CommandServiceType.Navigation)] ICommandService commandService, SettingsService settingsService, NavigationUserCompositeNode navigation)
  {
    Navigation = navigation;

    // Dependency Injection
    NavigationViewModelProvider = provider;
    NavigationViewModelCommandService = (NavigationViewModelCommandService)commandService;
    SettingsService = settingsService;

    ChildNodeViewModels = [.. Navigation.ChildNodes.Select(NavigationViewModelProvider.Resolve)];

    _ = SetIconImage();

    Navigation.PropertyChanged += Navigation_PropertyChanged;
    Navigation.ChildNodes.CollectionChanged += ChildNodes_CollectionChanged;

    // Register messenger
    RegisterMessenger();
  }

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
    IconImage = await IconHelper.GetIconImage(Navigation.Icon, (GroupIconBadge)SettingsService.Load(SettingsDescriptors.GroupIconBadge), Navigation is not NavigationUserRootNode);
  }

  private void ChildNodes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
  {
    switch (e.Action)
    {
      case NotifyCollectionChangedAction.Add:
        if (e.NewItems is IList { Count: > 0 } addedItems && addedItems[0] is INavigation addedItem)
        {
          Console.WriteLine("{0}: {1}", "Added", $"{e.NewStartingIndex}");
          ChildNodeViewModels.Insert(e.NewStartingIndex, NavigationViewModelProvider.Resolve(addedItem));
        }
        break;
      case NotifyCollectionChangedAction.Remove:
        if (e.OldItems is IList { Count: > 0 })
        {
          Console.WriteLine("{0}: {1}", "Removed", $"{e.OldStartingIndex}");
          ChildNodeViewModels.RemoveAt(e.OldStartingIndex);
        }
        break;
      case NotifyCollectionChangedAction.Replace:
        if (e.NewItems is IList { Count: > 0 } replacedItems && replacedItems[0] is INavigation replacedItem
          && e.NewStartingIndex < ChildNodeViewModels.Count)
        {
          Console.WriteLine("{0}: {1}", "Replaced", $"{e.NewStartingIndex}");
          ChildNodeViewModels[e.NewStartingIndex] = NavigationViewModelProvider.Resolve(replacedItem);
        }
        break;
      case NotifyCollectionChangedAction.Move:
        if (e.NewItems is IList { Count: > 0 } && e.OldItems is IList { Count: > 0 } && e.NewStartingIndex < ChildNodeViewModels.Count && e.OldStartingIndex < ChildNodeViewModels.Count)
        {
          Console.WriteLine("{0}: {1}", "Moved", $"{e.OldStartingIndex} -> {e.NewStartingIndex}");
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
      Navigation.PropertyChanged -= Navigation_PropertyChanged;
      Navigation.ChildNodes.CollectionChanged -= ChildNodes_CollectionChanged;
      UnregisterMessenger();
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

  public override Command<NavigationViewModelBase>? AddListCommand => NavigationViewModelCommandService.AddListCommand;
  public override Command<NavigationViewModelBase>? AddGroupCommand => NavigationViewModelCommandService.AddGroupCommand;
  public override Command<NavigationViewModelBase>? UpdateCommand => NavigationViewModelCommandService.UpdateCommand;
  public override Command<NavigationViewModelBase>? DeleteCommand => NavigationViewModelCommandService.DeleteCommand;
  public override Command<(NavigationViewModelBase SourceItemViewModel, NavigationViewModelBase TargetGroupViewModel)>? MoveToGroupCommand => NavigationViewModelCommandService.MoveToGroupCommand;

  private void RegisterMessenger()
  {
    WeakReferenceMessenger.Default.Register<ValueChangedMessage<GroupIconBadge>, MessageToken>(this, MessageTokens.ChangeNavigationViewModelIconImageToken, async (recipient, message) => await SetIconImage());
  }

  private void UnregisterMessenger()
  {
    WeakReferenceMessenger.Default.UnregisterAll(this);
  }
}
