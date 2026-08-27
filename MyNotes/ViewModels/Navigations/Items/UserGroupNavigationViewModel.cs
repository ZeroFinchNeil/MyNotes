using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Application.Settings.Services;
using MyNotes.Common.Commands;
using MyNotes.Common.Helpers;
using MyNotes.Common.Messages;
using MyNotes.Constants;
using MyNotes.Models.Navigations;
using MyNotes.Models.Navigations.Preferences;
using MyNotes.Models.Navigations.User;
using MyNotes.Services.Commands;
using MyNotes.ViewModels.Navigations.Items.Providers;

namespace MyNotes.ViewModels.Navigations.Items;

internal partial class UserGroupNavigationViewModel : UserNavigationViewModel
{
  public override NavigationUserCompositeNode Navigation { get; }

  private readonly LeasedNavigationViewModelCollection _childNodeViewModelLeases;
  public IReadOnlyCollection<NavigationViewModelBase> ChildNodeViewModels => _childNodeViewModelLeases.ViewModels;

  private readonly NavigationViewModelProvider NavigationViewModelProvider;
  private readonly NavigationCommandService NavigationCommandService;
  private readonly AppSettingsService AppSettingsService;

  #region Object Lifetime Management
  public UserGroupNavigationViewModel(NavigationViewModelProvider provider, [FromKeyedServices(CommandServiceType.Navigation)] ICommandService commandService, AppSettingsService appSettingsService, NavigationUserCompositeNode navigation) : base(navigation)
  {
    Navigation = navigation;

    // Dependency Injection
    NavigationViewModelProvider = provider;
    NavigationCommandService = (NavigationCommandService)commandService;
    AppSettingsService = appSettingsService;

    _childNodeViewModelLeases = new(Navigation.ChildNodes.Select(navigation => NavigationViewModelProvider.Resolve(navigation)));

    _ = SetIconImage();

    Navigation.PropertyChanged += Navigation_PropertyChanged;
    Navigation.ChildNodes.CollectionChanged += ChildNodes_CollectionChanged;

    SetCommands();
    // Register messenger
    RegisterMessenger();
  }

  protected override void Dispose(bool disposing)
  {
    if (Disposed)
    {
      return;
    }

    if (disposing)
    {
      Navigation.PropertyChanged -= Navigation_PropertyChanged;
      Navigation.ChildNodes.CollectionChanged -= ChildNodes_CollectionChanged;
      _childNodeViewModelLeases.Dispose();
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
    IconImage = await IconHelper.GetIconImage((int)Navigation.Icon, AppSettingsService.Load<GroupIconBadge, int>(GroupIconBadgeSettingsCodec.Decode, AppSettingsDescriptors.GroupIconBadge), Navigation is not NavigationUserRootNode);
  }

  private void ChildNodes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
  {
    switch (e.Action)
    {
      case NotifyCollectionChangedAction.Add:
        if (e.NewItems is IList { Count: > 0 } addedItems && addedItems[0] is INavigation addedItem)
        {
          _childNodeViewModelLeases.Insert(e.NewStartingIndex, NavigationViewModelProvider.Resolve(addedItem));
        }
        break;
      case NotifyCollectionChangedAction.Remove:
        if (e.OldItems is IList { Count: > 0 } removedItems)
        {
          _childNodeViewModelLeases.RemoveAt(e.OldStartingIndex);
        }
        break;
      case NotifyCollectionChangedAction.Replace:
        if (e.OldItems is IList { Count: > 0 } oldItems && e.NewItems is IList { Count: > 0 } replacedItems && replacedItems[0] is INavigation replacedItem
          && e.NewStartingIndex < ChildNodeViewModels.Count)
        {
          if (ChildNodeViewModels.FirstOrDefault(vm => vm.Navigation == oldItems[0]!) is UserNavigationViewModel oldViewModel)
          {
            _childNodeViewModelLeases.Replace(oldViewModel, NavigationViewModelProvider.Resolve(replacedItem));
          }
        }
        break;
      case NotifyCollectionChangedAction.Move:
        if (e.NewItems is IList { Count: > 0 } && e.OldItems is IList { Count: > 0 } && e.NewStartingIndex < ChildNodeViewModels.Count && e.OldStartingIndex < ChildNodeViewModels.Count)
        {
          _childNodeViewModelLeases.Move(e.OldStartingIndex, e.NewStartingIndex);
        }
        break;
      case NotifyCollectionChangedAction.Reset:
        _childNodeViewModelLeases.Clear();
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
      {
        action.Invoke(viewmodel);
      }

      if (viewmodel is UserGroupNavigationViewModel groupViewModel)
      {
        foreach (var childViewModel in groupViewModel.ChildNodeViewModels)
        {
          stack.Push(childViewModel);
        }
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
      {
        if (condition.Invoke(viewmodel))
        {
          return viewmodel;
        }
      }

      if (viewmodel is UserGroupNavigationViewModel groupViewModel)
      {
        foreach (var childViewModel in groupViewModel.ChildNodeViewModels)
        {
          stack.Push(childViewModel);
        }
      }
    }

    return null;
  }

  public override AsyncCommand AddListCommand { get; protected set; }
  public override AsyncCommand AddGroupCommand { get; protected set; }
  public override AsyncCommand ChangeTitleAndIconCommand { get; protected set; }
  public override AsyncCommand DeleteCommand { get; protected set; }
  public override AsyncCommand<NavigationUserCompositeNode> MoveToGroupCommand { get; protected set; }
  public override AsyncCommand SetAsStartPageCommand { get; protected set; }

  [MemberNotNull(nameof(AddListCommand), nameof(AddGroupCommand), nameof(ChangeTitleAndIconCommand), nameof(DeleteCommand), nameof(MoveToGroupCommand), nameof(SetAsStartPageCommand))]
  private void SetCommands()
  {
    AddListCommand = new()
    {
      ExecuteFunc = () => NavigationCommandService.AddNavigationAsync(Navigation, false)
    };

    AddGroupCommand = new()
    {
      ExecuteFunc = () => NavigationCommandService.AddNavigationAsync(Navigation, true)
    };

    ChangeTitleAndIconCommand = new()
    {
      ExecuteFunc = () => NavigationCommandService.ChangeNavigationTitleAndIconAsync(Navigation)
    };

    DeleteCommand = new()
    {
      ExecuteFunc = () => NavigationCommandService.DeleteNavigationAsync(Navigation)
    };

    MoveToGroupCommand = new()
    {
      ExecuteFunc = (targetGroupNavigation) => NavigationCommandService.MoveToGroupAsync(Navigation, targetGroupNavigation)
    };

    SetAsStartPageCommand = new()
    {
      ExecuteFunc = () => NavigationCommandService.SetAsStartPageAsync(Navigation),
      CanExecuteFunc = () => false
    };
  }

  private void RegisterMessenger()
  {
    WeakReferenceMessenger.Default.Register<ValueChangedMessage<GroupIconBadge>, MessageToken>(this, AppMessageTokens.ChangeNavigationViewModelIconImageToken, async (recipient, message) => await SetIconImage());
  }

  private void UnregisterMessenger()
  {
    WeakReferenceMessenger.Default.UnregisterAll(this);
  }
}
