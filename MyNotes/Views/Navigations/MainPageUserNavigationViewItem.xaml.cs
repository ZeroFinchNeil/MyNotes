using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using MyNotes.Models.Navigations;
using MyNotes.Resources;
using MyNotes.Templates;
using MyNotes.ViewModels.Navigations;


namespace MyNotes.Views.Navigations;

internal sealed partial class MainPageUserNavigationViewItem : DraggableNavigationViewItem
{
  public MainPageUserNavigationViewItem()
  {
    InitializeComponent();
    this.Loaded += MainPageUserNavigationViewItem_Loaded;
    this.Unloaded += MainPageUserNavigationViewItem_Unloaded;
  }

  public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(UserNavigationViewModel), typeof(MainPageUserNavigationViewItem), new PropertyMetadata(null));
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
      NavigationUserNode? currentGroup = ViewModel switch
      {
        UserLeafNavigationViewModel leaf => leaf.Navigation.Parent,
        UserCompositeNavigationViewModel composite => composite.Navigation,
        _ => null
      };

      MainPage_MoveToGroupMenuFlyoutSubItem.Items.Clear();
      RequestMessage<IReadOnlyList<UserCompositeNavigationViewModel>> message = new();
      WeakReferenceMessenger.Default.Send(message, MessageTokens.GetAllGroupNavigationViewModelsToken);

      if (message.HasReceivedResponse)
      {
        foreach (var targetVM in message.Response)
        {
          if (targetVM.Navigation == currentGroup)
            continue;
          MainPage_MoveToGroupMenuFlyoutSubItem.Items.Add(new MenuFlyoutItem
          {
            Text = targetVM.Navigation.Title,
            Icon = new ImageIcon() { Source = targetVM.IconImage },
            Command = ViewModel.MoveToGroupCommand,
            CommandParameter = (ViewModel as NavigationViewModelBase, targetVM as NavigationViewModelBase)
          });
        }
      }
    }
  }
}
