using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.AppConstants;
using MyNotes.Common.Messages;
using MyNotes.Models.Navigations;
using MyNotes.Services.Commands;
using MyNotes.Services.Settings;
using MyNotes.ViewModels.Navigations.Providers;

namespace MyNotes.ViewModels.Navigations;

internal sealed class UserRootNavigationViewModel : UserCompositeNavigationViewModel
{
  #region Object Lifetime Management
  public UserRootNavigationViewModel(NavigationViewModelProvider provider, [FromKeyedServices(CommandServiceType.Navigation)] ICommandService navigationCommandService, SettingsService settingsService, NavigationUserRootNode navigation)
    : base(provider, navigationCommandService, settingsService, navigation)
  {
  }

  protected override void Dispose(bool disposing)
  {
    if (Disposed)
      return;

    if (disposing)
    {
      base.Dispose(disposing);
    }

    base.Dispose(disposing);
  }
  #endregion

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

  private List<UserLeafNavigationViewModel> GetAllListNavigationViewModels()
  {
    List<UserLeafNavigationViewModel> viewmodels = new();
    Queue<UserCompositeNavigationViewModel> queue = new();

    queue.Enqueue(this);
    while (queue.Count > 0)
    {
      var viewmodel = queue.Dequeue();
      foreach (var childViewModel in viewmodel.ChildNodeViewModels)
      {
        switch (childViewModel)
        {
          case UserLeafNavigationViewModel leaf:
            viewmodels.Add(leaf);
            break;
          case UserCompositeNavigationViewModel composite:
            queue.Enqueue(composite);
            break;
        }
      }
    }

    return viewmodels;
  }
}
