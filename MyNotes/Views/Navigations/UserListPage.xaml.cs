using Microsoft.Extensions.DependencyInjection;

using MyNotes.Common.Structures;
using MyNotes.Debugging;
using MyNotes.Models.Navigations;
using MyNotes.Models.Notes;
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

  private void UserListPage_MoreButtonMenuFlyout_Opening(object sender, object e)
  {

  }

  private void UserListPage_NoteSortKeyRadioMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    if (sender is RadioMenuFlyoutItem item)
    {
      ViewModel?.Navigation.NoteSortKey = item.Tag switch
      {
        int intValue => (NoteSortKey)intValue,
        NoteSortKey noteSortKey => noteSortKey,
        _ => throw new ArgumentException("Type mismatch")
      };
      UserListPage_NotesListGridView.UpdateLayout();
    }
  }

  private void UserListPage_NoteSortDirectionRadioMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
  {
    if (sender is RadioMenuFlyoutItem item)
    {
      ViewModel?.Navigation.NoteSortDirection = item.Tag switch
      {
        int intValue => (SortDirection)intValue,
        SortDirection sortDirection => sortDirection,
        _ => throw new ArgumentException("Type mismatch")
      };
      UserListPage_NotesListGridView.UpdateLayout();
    }
  }

  private bool Equals(NoteSortKey key1, NoteSortKey key2) => key1 == key2;
  private bool Equals(SortDirection key1, SortDirection key2) => key1 == key2;
}
