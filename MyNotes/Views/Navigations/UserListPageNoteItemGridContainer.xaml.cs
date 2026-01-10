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
}
