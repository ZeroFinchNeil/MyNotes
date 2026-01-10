using MyNotes.ViewModels.Notes;

namespace MyNotes.Views.Navigations;

internal sealed partial class UserListPageNoteItemListContainer : UserControl
{
  public UserListPageNoteItemListContainer()
  {
    InitializeComponent();
  }
  public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(NoteViewModel), typeof(UserListPageNoteItemListContainer), new PropertyMetadata(null));
  public NoteViewModel ViewModel
  {
    get => (NoteViewModel)GetValue(ViewModelProperty);
    set => SetValue(ViewModelProperty, value);
  }
}
