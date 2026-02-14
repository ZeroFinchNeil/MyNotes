using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using MyNotes.Common.Structures;
using MyNotes.Constants;
using MyNotes.Debugging;
using MyNotes.Models.Navigations;
using MyNotes.Templates;
using MyNotes.ViewModels.Navigations;

namespace MyNotes.Views.Navigations;

internal sealed partial class UserNavigationViewItem : DraggableNavigationViewItem
{
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

  public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(UserNavigationViewModel), typeof(UserNavigationViewItem), new PropertyMetadata(null));
  public UserNavigationViewModel ViewModel
  {
    get => (UserNavigationViewModel)GetValue(ViewModelProperty);
    set => SetValue(ViewModelProperty, value);
  }

  private void MainPageUserNavigationViewItem_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
  }

  private void MainPageUserNavigationViewItem_Unloaded(object sender, RoutedEventArgs e)
  {
    Bindings.StopTracking();
  }

  private void MenuFlyout_Opening(object? sender, object e)
  {
    if (sender is MenuFlyout && ViewModel is not null)
    {
      MainPage_MoveToGroupMenuFlyoutSubItem.Items.Clear();
      RequestMessage<IReadOnlyList<UserCompositeNavigationViewModel>> message = new();
      WeakReferenceMessenger.Default.Send(message, MessageTokens.GetAllGroupNavigationViewModelsToken);

      if (message.HasReceivedResponse)
      {
        foreach (var targetVM in message.Response)
        {
          NavigationUserCompositeNode targetNavigation = targetVM.Navigation;
          if (!targetNavigation.CanBeParentOf(ViewModel.Navigation))
            continue;
          MainPage_MoveToGroupMenuFlyoutSubItem.Items.Add(new MenuFlyoutItem
          {
            Text = targetNavigation.Title,
            Icon = new ImageIcon() { Source = targetVM.IconImage },
            Command = ViewModel.MoveToGroupCommand,
            CommandParameter = new SourceTargetPair<NavigationViewModelBase, NavigationViewModelBase> { Source = ViewModel, Target = targetVM }
          });
        }

        MainPage_MoveToGroupMenuFlyoutSubItem.IsEnabled = MainPage_MoveToGroupMenuFlyoutSubItem.Items.Count > 0;
      }
      else
        MainPage_MoveToGroupMenuFlyoutSubItem.IsEnabled = false;
    }
  }
}
