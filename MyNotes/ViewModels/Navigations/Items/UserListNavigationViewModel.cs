using System.Diagnostics.CodeAnalysis;

using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Commands;
using MyNotes.Common.Helpers;
using MyNotes.Common.Messages;
using MyNotes.Constants;
using MyNotes.Models.Navigations.Preferences;
using MyNotes.Models.Navigations.User;
using MyNotes.Services.Commands;

namespace MyNotes.ViewModels.Navigations.Items;

internal sealed partial class UserListNavigationViewModel : UserNavigationViewModel
{
  public override NavigationUserLeafNode Navigation { get; }

  private readonly NavigationCommandService NavigationCommandService;

  #region Object Lifetime Management
  public UserListNavigationViewModel([FromKeyedServices(CommandServiceType.Navigation)] ICommandService navigationCommandService, NavigationUserLeafNode navigation) : base(navigation)
  {
    Navigation = navigation;

    // Dependency Injection
    NavigationCommandService = (NavigationCommandService)navigationCommandService;

    SetIconImage();
    Navigation.PropertyChanged += Navigation_PropertyChanged;

    SetCommands();
    // Messengers
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
      UnregisterMessenger();
    }

    base.Dispose(disposing);
  }
  #endregion

  private async void Navigation_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    switch (e.PropertyName)
    {
      case nameof(NavigationUserLeafNode.Icon):
        SetIconImage();
        break;
    }
  }

  private void SetIconImage() => IconImage = IconHelper.GetIconImage((int)Navigation.Icon);

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
      CanExecuteFunc = () => true
    };
  }

  private void RegisterMessenger()
  {
    WeakReferenceMessenger.Default.Register<ValueChangedMessage<GroupIconBadge>, MessageToken>(this, AppMessageTokens.ChangeNavigationViewModelIconImageToken, (recipient, message) => SetIconImage());
  }

  private void UnregisterMessenger()
  {
    WeakReferenceMessenger.Default.UnregisterAll(this);
  }
}