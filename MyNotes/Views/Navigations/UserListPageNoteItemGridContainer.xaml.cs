using MyNotes.Models.Notes;
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
    set
    {
      if (IsLoaded)
      {
        SetNotePropertyChangedEventHandler(GetValue(ViewModelProperty) as NoteViewModel, value);
      }

      SetValue(ViewModelProperty, value);
    }
  }

  private void SetNotePropertyChangedEventHandler(NoteViewModel? oldViewModel, NoteViewModel? newViewModel)
  {
    oldViewModel?.Note.PropertyChanged -= Note_PropertyChanged;
    newViewModel?.Note.PropertyChanged += Note_PropertyChanged;
  }

  private void Note_PropertyChanged(object? sender, PropertyChangedEventArgs e)
  {
    if (e.PropertyName == nameof(Note.Body))
    {
      SetPreview();
    }
  }

  private void SetPreview()
  {
    NoteItem_PreviewRichEditBox.IsReadOnly = false;
    NoteItem_PreviewRichEditBox.Document.SetText(TextSetOptions.FormatRtf, ViewModel.Note.Body);
    NoteItem_PreviewRichEditBox.IsReadOnly = true;
  }

  private void UserListPageNoteItemGridContainer_Loaded(object sender, RoutedEventArgs e)
  {
    SetPreview();
    ViewModel.Note.PropertyChanged += Note_PropertyChanged;
    Bindings.Update();
  }

  private void UserListPageNoteItemGridContainer_Unloaded(object sender, RoutedEventArgs e)
  {
    ViewModel.Note.PropertyChanged -= Note_PropertyChanged;
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