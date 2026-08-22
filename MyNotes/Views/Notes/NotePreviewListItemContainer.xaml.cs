using MyNotes.ViewModels.Notes;

namespace MyNotes.Views.Notes;

internal sealed partial class NotePreviewListItemContainer : UserControl
{
  #region Object Lifetime Management
  public NotePreviewListItemContainer()
  {
    InitializeComponent();

    this.Loaded += UserListPageNotePreviewListItemContainer_Loaded;
    this.Unloaded += UserListPageNotePreviewListItemContainer_Unloaded;
  }

  private void UserListPageNotePreviewListItemContainer_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
  }

  private void UserListPageNotePreviewListItemContainer_Unloaded(object sender, RoutedEventArgs e)
  {
    Bindings.StopTracking();
  }
  #endregion

  public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(NotePreviewViewModel), typeof(NotePreviewListItemContainer), new PropertyMetadata(null));
  public NotePreviewViewModel ViewModel
  {
    get => (NotePreviewViewModel)GetValue(ViewModelProperty);
    set => SetValue(ViewModelProperty, value);
  }

  private NoteViewModel? NoteViewModel => ViewModel?.NoteViewModel;
}
