using MyNotes.Domain.Notes;
using MyNotes.Views.Media;

namespace MyNotes.Views.Windows;

[Debugging.Attributes.ReferenceTracker]
internal sealed partial class ImageViewerWindow : Window
{
  #region Object Lifetime Management

  public ImageViewerWindow(NoteId noteId)
  {
    TrackReference();
    InitializeComponent();

    this.Content = new ImageViewerPage(noteId);
    this.Closed += ImageViewerWindow_Closed;
  }
  public bool IsClosed { get; private set; }

  private void ImageViewerWindow_Closed(object sender, WindowEventArgs args)
  {
    IsClosed = true;
  }
  #endregion
}
