using MyNotes.ViewModels.Notes;

namespace MyNotes.Views.Notes;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class NotePreviewListContainer : UserControl
{
  #region Object Lifetime Management
  public NotePreviewListContainer()
  {
    TrackReference();
    InitializeComponent();

    this.Loaded += UserListPageNotePreviewListContainer_Loaded;
    this.Unloaded += UserListPageNotePreviewListContainer_Unloaded;
  }

  private void UserListPageNotePreviewListContainer_Loaded(object sender, RoutedEventArgs e)
  {
    Bindings.Update();
  }

  private void UserListPageNotePreviewListContainer_Unloaded(object sender, RoutedEventArgs e)
  {
    Bindings.StopTracking();
  }
  #endregion

  public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(NoteViewModel), typeof(NotePreviewListContainer), new PropertyMetadata(null));
  public NoteViewModel ViewModel
  {
    get => (NoteViewModel)GetValue(ViewModelProperty);
    set => SetValue(ViewModelProperty, value);
  }
}
