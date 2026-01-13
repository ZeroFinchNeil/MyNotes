using Microsoft.Extensions.DependencyInjection;

using MyNotes.Debugging;
using MyNotes.Models.Navigations;
using MyNotes.ViewModels.Navigations;

namespace MyNotes.Views.Navigations;

public sealed partial class UserListPage : Page
{
  private UserLeafNavigationViewModel? ViewModel;

  public UserListPage()
  {
    InitializeComponent();
    this.Loaded += UserListPage_Loaded;
  }

  protected override async void OnNavigatedTo(NavigationEventArgs e)
  {
    if (e.Parameter is NavigationUserLeafNode navigation)
    {
      var provider = App.Instance.Services.GetRequiredService<NavigationViewModelProvider>();
      ViewModel = provider.Resolve(navigation) as UserLeafNavigationViewModel;
      if (ViewModel is not null)
      {
        await ViewModel.LoadNoteViewModels();
        this.Unloaded += UserListPage_Unloaded;
      }

#if DEBUG
      ReferenceTracker.UserListPageReference.Add(this, ViewModel?.GetHashCode());
#endif
    }
  }

  private void UserListPage_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
  }

  private void UserListPage_Unloaded(object sender, RoutedEventArgs e)
  {
    ViewModel?.UnloadNoteViewModels();
    Bindings.StopTracking();
  }
}
