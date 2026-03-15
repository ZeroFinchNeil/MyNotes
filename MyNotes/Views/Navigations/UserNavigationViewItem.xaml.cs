using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using Microsoft.Extensions.DependencyInjection;

using MyNotes.AppConstants;
using MyNotes.Common.Structures;
using MyNotes.Debugging;
using MyNotes.Models.Navigations;
using MyNotes.Services.Navigations;
using MyNotes.Templates;
using MyNotes.ViewModels.Navigations;

namespace MyNotes.Views.Navigations;

internal sealed partial class UserNavigationViewItem : DraggableNavigationViewItem
{
  #region Object Lifetime Management
  public UserNavigationViewItem()
  {
#if DEBUG
    if (Debugger.IsAttached)
    {
      ReferenceTracker.ElementReference.Add(this, $"{GetType().Name}: {GetHashCode()}");
    }
#endif
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

      var navigationService = App.Services.GetRequiredService<NavigationService>();
      var navigationViewModelProvider = App.Services.GetRequiredService<NavigationViewModelProvider>();

      foreach (var targetVM in navigationViewModelProvider.Resolve<UserCompositeNavigationViewModel>(navigationService.UserCompositeNavigations))
      {
        var targetNavigation = targetVM.Navigation;
        if (!targetNavigation.CanBeParentOf(ViewModel.Navigation))
          continue;
        MainPage_MoveToGroupMenuFlyoutSubItem.Items.Add(new MenuFlyoutItem
        {
          Text = targetNavigation.Title,
          Icon = new ImageIcon() { Source = targetVM.IconImage },
          Command = ViewModel.MoveToGroupCommand,
          CommandParameter = new SourceTargetPair<NavigationUserNode, NavigationUserCompositeNode> { Source = ViewModel.Navigation, Target = targetNavigation }
        });
      }

      MainPage_MoveToGroupMenuFlyoutSubItem.IsEnabled = MainPage_MoveToGroupMenuFlyoutSubItem.Items.Count > 0;
    }
  }
}
