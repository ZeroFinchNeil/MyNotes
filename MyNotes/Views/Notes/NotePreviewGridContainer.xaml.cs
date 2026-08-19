using Microsoft.Extensions.DependencyInjection;

using MyNotes.Debugging;
using MyNotes.Models.Navigations;
using MyNotes.Services.Navigations;
using MyNotes.ViewModels.Navigations;
using MyNotes.ViewModels.Navigations.Providers;
using MyNotes.ViewModels.Notes;

namespace MyNotes.Views.Notes;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class NotePreviewGridContainer : UserControl
{
  #region Object Lifetime Management
  public NotePreviewGridContainer()
  {
    TrackReference();
    InitializeComponent();
    this.Loaded += UserListPageNotePreviewGridContainer_Loaded;
    this.Unloaded += UserListPageNotePreviewGridContainer_Unloaded;
  }

  private void UserListPageNotePreviewGridContainer_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
  }

  private void UserListPageNotePreviewGridContainer_Unloaded(object sender, RoutedEventArgs e)
  {
    ConsoleHelper.WriteLine(true, "{0}: {1}", "Container Unloaded", true);
    Bindings.StopTracking();
  }
  #endregion  

  public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(NoteViewModel), typeof(NotePreviewGridContainer), new PropertyMetadata(null));
  public NoteViewModel ViewModel
  {
    get => (NoteViewModel)GetValue(ViewModelProperty);
    set => SetValue(ViewModelProperty, value);
  }

  private void NoteItem_RootGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
  {
    _commandOverlayPersistenceTCS?.TrySetResult(true);
    VisualStateManager.GoToState(this, "CommandOverlayVisible", false);
  }

  private void NoteItem_RootGrid_PointerExited(object sender, PointerRoutedEventArgs e)
  {
    _commandOverlayPersistenceTCS?.TrySetResult(false);
    if (!_preventCommandOverlayCollapse)
    {
      VisualStateManager.GoToState(this, "CommandOverlayCollapsed", false);
    }
  }

  private bool _preventCommandOverlayCollapse = false;
  private void NoteItem_MoreButtonMenuFlyout_Opening(object sender, object e)
  {
    _preventCommandOverlayCollapse = true;
    VisualStateManager.GoToState(this, "CommandOverlayVisible", false);

    if (sender is MenuFlyout && ViewModel is not null)
    {
      NoteItem_MoveToListMenuFlyoutSubItem.Items.Clear();

      var navigationController = App.Services.GetRequiredService<NavigationController>();
      var navigationViewModelProvider = App.Services.GetRequiredService<NavigationViewModelProvider>();

      foreach (var targetNavigation in navigationController.UserLeafNavigations.OfType<NavigationUserLeafNode>())
      {
        using var lease = navigationViewModelProvider.Acquire(targetNavigation);
        if (lease?.ViewModel is UserListNavigationViewModel targetVM)
        {
          if (targetNavigation.Id == ViewModel.Note.NavigationId)
          {
            continue;
          }

          NoteItem_MoveToListMenuFlyoutSubItem.Items.Add(new MenuFlyoutItem
          {
            Text = targetNavigation.Title,
            Icon = new ImageIcon() { Source = targetVM.IconImage },
            Command = ViewModel.MoveToListCommand,
            CommandParameter = targetNavigation.Id
          });
        }
      }

      NoteItem_MoveToListMenuFlyoutSubItem.IsEnabled = NoteItem_MoveToListMenuFlyoutSubItem.Items.Count > 0;
    }
  }

  private TaskCompletionSource<bool>? _commandOverlayPersistenceTCS;
  private async void NoteItem_MoreButtonMenuFlyout_Closing(FlyoutBase sender, FlyoutBaseClosingEventArgs args)
  {
    _preventCommandOverlayCollapse = false;
    _commandOverlayPersistenceTCS = new(TaskCreationOptions.RunContinuationsAsynchronously);

    try
    {
      if (!await _commandOverlayPersistenceTCS.Task.WaitAsync(TimeSpan.FromMilliseconds(250)))
      {
        VisualStateManager.GoToState(this, "CommandOverlayCollapsed", false);
      }
    }
    catch (TimeoutException)
    {
      VisualStateManager.GoToState(this, "CommandOverlayCollapsed", false);
    }

    _commandOverlayPersistenceTCS = null;
  }

  private void NoteItem_RootGrid_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
  {
    args.TryGetPosition(sender, out var position);
    NoteItem_MoreButtonMenuFlyout.ShowAt(this, position);
  }
}