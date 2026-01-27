using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Media.Imaging;

using MyNotes.Common.Collections;
using MyNotes.Common.Commands;
using MyNotes.Common.Messages;
using MyNotes.Common.Structures;
using MyNotes.Constants;
using MyNotes.Helpers;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
using MyNotes.Models.Settings;
using MyNotes.Services.Commands;
using MyNotes.Services.Notes;
using MyNotes.Services.Settings;
using MyNotes.Services.Windows;
using MyNotes.ViewModels.Notes;

namespace MyNotes.ViewModels.Navigations;

internal sealed partial class UserLeafNavigationViewModel : UserNavigationViewModel
{
  public override NavigationUserLeafNode Navigation { get; }

  private readonly NavigationViewModelCommandService NavigationViewModelCommandService;

  public UserLeafNavigationViewModel([FromKeyedServices(CommandServiceType.NavigationViewModel)] ICommandService navigationViewModelCommandService, NavigationUserLeafNode navigation) : base(navigation)
  {
    Navigation = navigation;

    // Dependency Injection
    NavigationViewModelCommandService = (NavigationViewModelCommandService)navigationViewModelCommandService;

    SetIconImage();
    Navigation.PropertyChanged += Navigation_PropertyChanged;

    // Messengers
    RegisterMessenger();
  }

  private async void Navigation_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    switch (e.PropertyName)
    {
      case nameof(NavigationUserLeafNode.Icon):
        SetIconImage();
        break;
    }
  }

  private void SetIconImage() => IconImage = new BitmapImage() { UriSource = IconHelper.GetMainUri((short)Navigation.Icon) };

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

  public override Command<NavigationViewModelBase> AddListCommand => NavigationViewModelCommandService.AddListCommand;
  public override Command<NavigationViewModelBase> AddGroupCommand => NavigationViewModelCommandService.AddGroupCommand;
  public override Command<NavigationViewModelBase> UpdateCommand => NavigationViewModelCommandService.UpdateCommand;
  public override Command<NavigationViewModelBase> DeleteCommand => NavigationViewModelCommandService.DeleteCommand;
  public override Command<SourceTargetPair<NavigationViewModelBase, NavigationViewModelBase>> MoveToGroupCommand => NavigationViewModelCommandService.MoveToGroupCommand;
  public override Command<NavigationViewModelBase> SetAsStartPageCommand => NavigationViewModelCommandService.SetAsStartPageCommand;

  private void RegisterMessenger()
  {
    WeakReferenceMessenger.Default.Register<ValueChangedMessage<GroupIconBadge>, MessageToken>(this, MessageTokens.ChangeNavigationViewModelIconImageToken, (recipient, message) => SetIconImage());
  }

  private void UnregisterMessenger()
  {
    WeakReferenceMessenger.Default.UnregisterAll(this);
  }
}