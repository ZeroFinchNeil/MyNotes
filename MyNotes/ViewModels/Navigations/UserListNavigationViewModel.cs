using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Commands;
using MyNotes.Common.Messages;
using MyNotes.Common.Structures;
using MyNotes.Constants;
using MyNotes.Helpers;
using MyNotes.Models.Navigations;
using MyNotes.Services.Commands;
using MyNotes.Shared.Enums.Settings;

namespace MyNotes.ViewModels.Navigations;

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

  public override AsyncCommand<NavigationUserNode> AddListCommand => NavigationCommandService.AddListCommand;
  public override Command<NavigationUserNode> AddGroupCommand => NavigationCommandService.AddGroupCommand;
  public override Command<NavigationUserNode> UpdateCommand => NavigationCommandService.UpdateCommand;
  public override Command<NavigationUserNode> DeleteCommand => NavigationCommandService.DeleteCommand;
  public override Command<SourceTargetPair<NavigationUserNode, NavigationUserCompositeNode>> MoveToGroupCommand => NavigationCommandService.MoveToGroupCommand;
  public override Command<NavigationUserNode> SetAsStartPageCommand => NavigationCommandService.SetAsStartPageCommand;

  private void RegisterMessenger()
  {
    WeakReferenceMessenger.Default.Register<ValueChangedMessage<GroupIconBadge>, MessageToken>(this, AppMessageTokens.ChangeNavigationViewModelIconImageToken, (recipient, message) => SetIconImage());
  }

  private void UnregisterMessenger()
  {
    WeakReferenceMessenger.Default.UnregisterAll(this);
  }
}