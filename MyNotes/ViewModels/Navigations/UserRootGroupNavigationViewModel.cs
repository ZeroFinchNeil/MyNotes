using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Navigations;
using MyNotes.Services.Commands;
using MyNotes.Services.Settings;
using MyNotes.ViewModels.Navigations.Providers;

namespace MyNotes.ViewModels.Navigations;

internal sealed class UserRootGroupNavigationViewModel : UserGroupNavigationViewModel
{
  #region Object Lifetime Management
  public UserRootGroupNavigationViewModel(NavigationViewModelProvider provider, [FromKeyedServices(CommandServiceType.Navigation)] ICommandService navigationCommandService, ViewStateSettingsService viewStateSettingsService, NavigationUserRootNode navigation)
    : base(provider, navigationCommandService, viewStateSettingsService, navigation)
  {
  }

  protected override void Dispose(bool disposing)
  {
    if (Disposed)
    {
      return;
    }

    if (disposing)
    {
      base.Dispose(disposing);
    }

    base.Dispose(disposing);
  }
  #endregion

  public IReadOnlyList<UserGroupNavigationViewModel> GetAllGroupNavigationViewModels()
  {
    List<UserGroupNavigationViewModel> viewmodels = new();

    Queue<UserGroupNavigationViewModel> queue = new();
    queue.Enqueue(this);
    while (queue.Count > 0)
    {
      var viewmodel = queue.Dequeue();
      viewmodels.Add(viewmodel);
      foreach (var childViewModel in viewmodel.ChildNodeViewModels)
      {
        if (childViewModel is UserGroupNavigationViewModel groupViewModel)
        {
          queue.Enqueue(groupViewModel);
        }
      }
    }

    return viewmodels;
  }

  public IReadOnlyList<UserListNavigationViewModel> GetAllListNavigationViewModels()
  {
    List<UserListNavigationViewModel> viewmodels = new();
    Queue<UserGroupNavigationViewModel> queue = new();

    queue.Enqueue(this);
    while (queue.Count > 0)
    {
      var viewmodel = queue.Dequeue();
      foreach (var childViewModel in viewmodel.ChildNodeViewModels)
      {
        switch (childViewModel)
        {
          case UserListNavigationViewModel leaf:
            viewmodels.Add(leaf);
            break;
          case UserGroupNavigationViewModel composite:
            queue.Enqueue(composite);
            break;
        }
      }
    }

    return viewmodels;
  }

  public IReadOnlyList<UserNavigationViewModel> GetAllNavigationViewModels()
  {
    List<UserNavigationViewModel> viewmodels = new();
    Queue<UserGroupNavigationViewModel> queue = new();

    queue.Enqueue(this);
    while (queue.Count > 0)
    {
      var viewmodel = queue.Dequeue();
      foreach (var childViewModel in viewmodel.ChildNodeViewModels)
      {
        switch (childViewModel)
        {
          case UserListNavigationViewModel leaf:
            viewmodels.Add(leaf);
            break;
          case UserGroupNavigationViewModel composite:
            viewmodels.Add(composite);
            queue.Enqueue(composite);
            break;
        }
      }
    }

    return viewmodels;
  }
}
