using MyNotes.ViewModels.Notes;

namespace MyNotes.Views.Navigations;

internal sealed partial class UserListPageNoteItemListContainer : UserControl
{
  public UserListPageNoteItemListContainer()
  {
    InitializeComponent();

    this.Loaded += UserListPageNoteItemListContainer_Loaded;
    this.Unloaded += UserListPageNoteItemListContainer_Unloaded;
  }

  public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(NoteViewModel), typeof(UserListPageNoteItemListContainer), new PropertyMetadata(null));
  public NoteViewModel ViewModel
  {
    get => (NoteViewModel)GetValue(ViewModelProperty);
    set => SetValue(ViewModelProperty, value);
  }

  private void UserListPageNoteItemListContainer_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
  }

  private void UserListPageNoteItemListContainer_Unloaded(object sender, RoutedEventArgs e)
  {
    Bindings.StopTracking();
  }
}
