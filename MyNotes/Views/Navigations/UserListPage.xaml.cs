using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Navigations;
using MyNotes.ViewModels.Navigations;

namespace MyNotes.Views.Navigations;

public sealed partial class UserListPage : Page
{
  private UserLeafNavigationViewModel? ViewModel;

  public UserListPage()
  {
    InitializeComponent();
  }

  protected override void OnNavigatedTo(NavigationEventArgs e)
  {
    if (e.Parameter is NavigationUserLeafNode navigation)
    {
      var provider = App.Instance.Services.GetRequiredService<NavigationViewModelProvider>();
      ViewModel = provider.Resolve(navigation) as UserLeafNavigationViewModel;
    }
  }
}
