using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Messages;
using MyNotes.Models.Navigations;
using MyNotes.Resources;
using MyNotes.Services.Commands;
using MyNotes.Services.Settings;

namespace MyNotes.ViewModels.Navigations;

internal sealed class UserRootNavigationViewModel : UserCompositeNavigationViewModel
{
  public UserRootNavigationViewModel(NavigationViewModelProvider provider, [FromKeyedServices(CommandServiceType.Navigation)] ICommandService navigationViewModelCommandService, SettingsService settingsService, NavigationUserRootNode navigation)
    : base(provider, navigationViewModelCommandService, settingsService, navigation)
  {
    RegisterMessenger();
  }

  private List<UserCompositeNavigationViewModel> GetAllGroupNavigationViewModels()
  {
    List<UserCompositeNavigationViewModel> viewmodels = new();

    Queue<UserCompositeNavigationViewModel> queue = new();
    queue.Enqueue(this);
    while (queue.Count > 0)
    {
      var viewmodel = queue.Dequeue();
      viewmodels.Add(viewmodel);
      foreach (var childViewModel in viewmodel.ChildNodeViewModels)
      {
        if (childViewModel is UserCompositeNavigationViewModel compositeViewModel)
          queue.Enqueue(compositeViewModel);
      }
    }

    return viewmodels;
  }

  protected override void Dispose(bool disposing)
  {
    if (_disposed)
      return;

    if (disposing)
    {
      UnregisterMessenger();
      base.Dispose(disposing);
    }

    _disposed = true;
  }

  private void RegisterMessenger()
  {
    WeakReferenceMessenger.Default.Register<RequestMessage<IReadOnlyList<UserCompositeNavigationViewModel>>, MessageToken>(this, MessageTokens.GetAllGroupNavigationViewModelsToken, (recipient, message) =>
    {
      message.Reply(GetAllGroupNavigationViewModels());
    });
  }

  private void UnregisterMessenger() => WeakReferenceMessenger.Default.UnregisterAll(this);
}
