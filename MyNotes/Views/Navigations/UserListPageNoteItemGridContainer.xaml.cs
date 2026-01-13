using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

using MyNotes.Common.Structures;
using MyNotes.Models.Navigations;
using MyNotes.Resources;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Notes;

namespace MyNotes.Views.Navigations;

internal sealed partial class UserListPageNoteItemGridContainer : UserControl
{
  public UserListPageNoteItemGridContainer()
  {
    InitializeComponent();
    this.Loaded += UserListPageNoteItemGridContainer_Loaded;
    this.Unloaded += UserListPageNoteItemGridContainer_Unloaded;
  }

  public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(NoteViewModel), typeof(UserListPageNoteItemGridContainer), new PropertyMetadata(null));
  public NoteViewModel ViewModel
  {
    get => (NoteViewModel)GetValue(ViewModelProperty);
    set => SetValue(ViewModelProperty, value);
  }

  private void UserListPageNoteItemGridContainer_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
  }

  private void UserListPageNoteItemGridContainer_Unloaded(object sender, RoutedEventArgs e)
  {
    Bindings.StopTracking();
  }

  private void NoteItem_RootGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
  {
    VisualStateManager.GoToState(this, "PointerEntered", false);
  }

  private void NoteItem_RootGrid_PointerExited(object sender, PointerRoutedEventArgs e)
  {
    VisualStateManager.GoToState(this, "PointerExited", false);
  }

  private void MenuFlyout_Opening(object sender, object e)
  {
    if (sender is MenuFlyout && ViewModel is not null)
    {
      NoteItem_MoveToListMenuFlyoutSubItem.Items.Clear();
      RequestMessage<IReadOnlyList<UserLeafNavigationViewModel>> message = new();
      WeakReferenceMessenger.Default.Send(message, MessageTokens.GetAllListNavigationViewModelsToken);

      if (message.HasReceivedResponse)
      {
        foreach (var targetVM in message.Response)
        {
          if (targetVM.Navigation.Id == ViewModel.Note.NavigationId)
            continue;
          NoteItem_MoveToListMenuFlyoutSubItem.Items.Add(new MenuFlyoutItem
          {
            Text = targetVM.Navigation.Title,
            Icon = new ImageIcon() { Source = targetVM.IconImage },
            Command = ViewModel.MoveToListCommand,
            CommandParameter = new SourceTargetPair<NavigationId, NavigationId> { Source = ViewModel.Note.NavigationId, Target = targetVM.Navigation.Id }
          });
        }

        NoteItem_MoveToListMenuFlyoutSubItem.IsEnabled = NoteItem_MoveToListMenuFlyoutSubItem.Items.Count > 0;
      }
      else
        NoteItem_MoveToListMenuFlyoutSubItem.IsEnabled = false;
    }
  }

  private void NoteItem_RootGrid_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
  {
    args.TryGetPosition(sender, out var position);
    NoteItem_MoreButtonMenuFlyout.ShowAt(this, position);
  }
}