using Microsoft.Extensions.DependencyInjection;

using MyNotes.Models.Navigations.User;
using MyNotes.Services.Navigations;
using MyNotes.Templates;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Navigations.Providers;

namespace MyNotes.Views.Navigations;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class UserNavigationViewItem : DraggableNavigationViewItem
{
  #region Object Lifetime Management
  public UserNavigationViewItem()
  {
    TrackReference();
    InitializeComponent();
    this.Loaded += MainPageUserNavigationViewItem_Loaded;
    this.Unloaded += MainPageUserNavigationViewItem_Unloaded;
  }

  private void MainPageUserNavigationViewItem_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
  }

  private void MainPageUserNavigationViewItem_Unloaded(object sender, RoutedEventArgs e)
  {
    Bindings.StopTracking();
  }
  #endregion

  public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(UserNavigationViewModel), typeof(UserNavigationViewItem), new PropertyMetadata(null));
  public UserNavigationViewModel ViewModel
  {
    get => (UserNavigationViewModel)GetValue(ViewModelProperty);
    set => SetValue(ViewModelProperty, value);
  }

  private void MenuFlyout_Opening(object? sender, object e)
  {
    if (sender is MenuFlyout && ViewModel is not null)
    {
      MainPage_MoveToGroupMenuFlyoutSubItem.Items.Clear();

      var navigationController = App.Services.GetRequiredService<NavigationController>();
      var navigationViewModelProvider = App.Services.GetRequiredService<NavigationViewModelProvider>();

      foreach (var targetNavigation in navigationController.UserCompositeNavigations.OfType<NavigationUserCompositeNode>())
      {
        using var lease = navigationViewModelProvider.Acquire(targetNavigation);
        if (lease?.ViewModel is UserGroupNavigationViewModel targetViewModel)
        {
          if (!targetNavigation.CanBeParentOf(ViewModel.Navigation))
          {
            continue;
          }

          MainPage_MoveToGroupMenuFlyoutSubItem.Items.Add(new MenuFlyoutItem
          {
            Text = targetNavigation.Title,
            Icon = new ImageIcon() { Source = targetViewModel.IconImage },
            Command = ViewModel.MoveToGroupCommand,
            CommandParameter = targetNavigation
          });
        }
      }

      MainPage_MoveToGroupMenuFlyoutSubItem.IsEnabled = MainPage_MoveToGroupMenuFlyoutSubItem.Items.Count > 0;
    }
  }
}
