using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media.Imaging;

using MyNotes.Common.Commands;
using MyNotes.Common.Messages;
using MyNotes.Helpers;
using MyNotes.Models.Navigations;
using MyNotes.Models.Settings;
using MyNotes.Resources;
using MyNotes.Services.Commands;

namespace MyNotes.ViewModels.Navigations;

internal sealed partial class UserLeafNavigationViewModel : UserNavigationViewModel
{
  public override NavigationUserLeafNode Navigation { get; }

  private readonly NavigationCommandService NavigationCommandService;

  public UserLeafNavigationViewModel([FromKeyedServices(CommandServiceType.Navigation)] ICommandService navigationCommandService,  NavigationUserLeafNode navigation)
  {
    Navigation = navigation;

    // Dependency Injection
    NavigationCommandService = (NavigationCommandService)navigationCommandService;

    SetIconImage();
    Navigation.PropertyChanged += Navigation_PropertyChanged;

    // Messengers
    RegisterMessenger();
  }

  private async void Navigation_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    switch (e.PropertyName)
    {
      case nameof(NavigationUserCompositeNode.Icon):
        SetIconImage();
        break;
    }
  }

  private void SetIconImage() => IconImage = new BitmapImage() { UriSource = IconHelper.GetMainUri(Navigation.Icon) };

  protected override void Dispose(bool disposing)
  {
    if (_disposed)
      return;

    if (disposing)
    {
      Navigation.PropertyChanged -= Navigation_PropertyChanged;
      UnregisterMessenger();
    }

    _disposed = true;
  }

  public override Command<NavigationViewModelBase>? AddListCommand => NavigationCommandService.AddListCommand;
  public override Command<NavigationViewModelBase>? AddGroupCommand => NavigationCommandService.AddGroupCommand;
  public override Command<NavigationViewModelBase>? UpdateCommand => NavigationCommandService.UpdateCommand;
  public override Command<NavigationViewModelBase>? DeleteCommand => NavigationCommandService.DeleteCommand;
  public override Command<(NavigationViewModelBase SourceItemViewModel, NavigationViewModelBase TargetGroupViewModel)>? MoveToGroupCommand => NavigationCommandService.MoveToGroupCommand;

  private void RegisterMessenger()
  {
    WeakReferenceMessenger.Default.Register<ValueChangedMessage<GroupIconBadge>, MessageToken>(this, MessageTokens.ChangeNavigationViewModelIconImageToken, (recipient, message) => SetIconImage());
  }

  private void UnregisterMessenger()
  {
    WeakReferenceMessenger.Default.UnregisterAll(this);
  }
}
